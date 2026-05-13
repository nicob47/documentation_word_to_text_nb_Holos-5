using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using H.Avalonia.Services;
using H.Core.Enumerations;
using H.Core.Models.Results;
using H.Core.Services.Analysis;
using H.Core.Services.StorageService;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Prism.Commands;
using Prism.Regions;

namespace H.Avalonia.ViewModels.Results;

/// <summary>
/// Phase 5 vertical slice: pulls the ICBM / IPCC Tier 2 carbon results from
/// <see cref="IFarmAnalysisService"/> for the active farm and exposes them as a flat
/// <see cref="FieldAnalysisYearResult"/> collection that the view's DataGrid binds to.
/// </summary>
public class GHGResultsViewModel : ResultsViewModelBase
{
    #region Fields

    private readonly ILogger _logger;
    private readonly IFarmAnalysisService _farmAnalysisService;
    private readonly INotificationManagerService? _notificationManager;

    private ObservableCollection<FieldAnalysisYearResult> _yearResults = new();
    private ObservableCollection<ShelterbeltYearResult> _shelterbeltYearResults = new();
    private string _farmName = string.Empty;
    private string _carbonModellingStrategy = string.Empty;
    private bool _hasResults;
    private bool _hasShelterbeltResults;
    private string? _lastErrorMessage;
    private CarbonModellingStrategies _selectedStrategy;
    private bool _suppressStrategyReanalysis;
    private ISeries[] _soilCarbonTrendSeries = Array.Empty<ISeries>();
    private Axis[] _soilCarbonTrendXAxes = Array.Empty<Axis>();

    #endregion

    #region Constructors

    /// <summary>
    /// Design-time / fallback constructor. Avalonia's XAML previewer and Prism's container can
    /// reach this when the full DI ctor's dependencies don't resolve — using <see cref="NullLogger"/>
    /// instead of null! prevents an NRE at <see cref="RunAnalysis"/> time and surfaces the
    /// configuration problem as a clean log line rather than a crash.
    /// </summary>
    public GHGResultsViewModel()
    {
        _logger = NullLogger.Instance;
        _farmAnalysisService = null!;
        _notificationManager = null;
        RecalculateCommand = new DelegateCommand(RunAnalysis);
        ExportFieldResultsCommand = new DelegateCommand(ExportFieldResultsToCsv, () => this.HasResults);
    }

    public GHGResultsViewModel(
        ILogger logger,
        IStorageService storageService,
        IFarmAnalysisService farmAnalysisService,
        INotificationManagerService? notificationManager = null)
        : base()
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(storageService);
        ArgumentNullException.ThrowIfNull(farmAnalysisService);

        _logger = logger;
        _farmAnalysisService = farmAnalysisService;
        _notificationManager = notificationManager;
        StorageService = storageService;

        RecalculateCommand = new DelegateCommand(RunAnalysis);
        ExportFieldResultsCommand = new DelegateCommand(ExportFieldResultsToCsv, () => this.HasResults);
        this.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(HasResults))
            {
                ExportFieldResultsCommand.RaiseCanExecuteChanged();
            }
        };
    }

    #endregion

    #region Properties

    public ObservableCollection<FieldAnalysisYearResult> YearResults
    {
        get => _yearResults;
        set => SetProperty(ref _yearResults, value);
    }

    /// <summary>
    /// Per-shelterbelt, per-year results. Empty when the active farm has no shelterbelt
    /// components — the view hides the shelterbelt section in that case (via
    /// <see cref="HasShelterbeltResults"/>).
    /// </summary>
    public ObservableCollection<ShelterbeltYearResult> ShelterbeltYearResults
    {
        get => _shelterbeltYearResults;
        set => SetProperty(ref _shelterbeltYearResults, value);
    }

    public bool HasShelterbeltResults
    {
        get => _hasShelterbeltResults;
        set => SetProperty(ref _hasShelterbeltResults, value);
    }

    /// <summary>
    /// One <see cref="LineSeries{TValue}"/> per field, plotting <see cref="FieldAnalysisYearResult.SoilCarbon"/>
    /// across the union of all years. Field rows that don't cover a particular year contribute a
    /// null sentinel for that x-axis slot so LiveCharts draws a gap rather than a synthetic zero.
    /// </summary>
    public ISeries[] SoilCarbonTrendSeries
    {
        get => _soilCarbonTrendSeries;
        set => SetProperty(ref _soilCarbonTrendSeries, value);
    }

    public Axis[] SoilCarbonTrendXAxes
    {
        get => _soilCarbonTrendXAxes;
        set => SetProperty(ref _soilCarbonTrendXAxes, value);
    }

    public string FarmName
    {
        get => _farmName;
        set => SetProperty(ref _farmName, value);
    }

    public string CarbonModellingStrategy
    {
        get => _carbonModellingStrategy;
        set => SetProperty(ref _carbonModellingStrategy, value);
    }

    /// <summary>
    /// True when the last analysis produced at least one row. The view toggles its empty-state
    /// placeholder on this.
    /// </summary>
    public bool HasResults
    {
        get => _hasResults;
        set => SetProperty(ref _hasResults, value);
    }

    /// <summary>
    /// Non-null when the last analysis run threw. Surfaces a banner in the view rather than
    /// silently swallowing the error — the carbon pipeline has many ways to misconfigure data.
    /// </summary>
    public string? LastErrorMessage
    {
        get => _lastErrorMessage;
        set => SetProperty(ref _lastErrorMessage, value);
    }

    public DelegateCommand RecalculateCommand { get; }

    /// <summary>
    /// Writes the current field-level results to a CSV file under
    /// <c>{Documents}/Holos5/Exports/</c> with a timestamped filename. Disabled when there are no
    /// results to export. Errors surface through <see cref="LastErrorMessage"/> just like the
    /// analysis errors.
    /// </summary>
    public DelegateCommand ExportFieldResultsCommand { get; }

    /// <summary>
    /// Available carbon-modelling strategies for the ComboBox in the results view. Static — the
    /// enum has exactly two values (IPCC Tier 2 and ICBM).
    /// </summary>
    public IReadOnlyList<CarbonModellingStrategies> AvailableCarbonStrategies { get; } =
        new[] { CarbonModellingStrategies.ICBM, CarbonModellingStrategies.IPCCTier2 };

    /// <summary>
    /// The strategy that will be used the next time analysis runs. Setting this property writes
    /// the value back to the active farm's Defaults and triggers a re-run, so the user sees the
    /// effect of switching strategies without having to click Recalculate.
    /// </summary>
    public CarbonModellingStrategies SelectedStrategy
    {
        get => _selectedStrategy;
        set
        {
            if (SetProperty(ref _selectedStrategy, value) && !_suppressStrategyReanalysis)
            {
                ApplyStrategyAndReanalyze(value);
            }
        }
    }

    #endregion

    #region Public Methods

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        this.InitializeViewModel();
        this.RunAnalysis();
    }

    public override void InitializeViewModel()
    {
        base.InitializeViewModel();
    }

    #endregion

    #region Private Methods

    private void RunAnalysis()
    {
        if (_farmAnalysisService is null)
        {
            // Reached only via the design-time / fallback parameterless constructor (see ctor
            // docs). Without an analysis service we have nothing to compute — bail rather than
            // NRE so the XAML previewer / a misconfigured container still renders the empty view.
            _logger.LogWarning(
                "GHGResultsViewModel.RunAnalysis called without an IFarmAnalysisService; " +
                "the view model was constructed via the parameterless ctor. Check that " +
                "IFarmAnalysisService and its transitive dependencies are registered in DI.");
            this.HasResults = false;
            this.HasShelterbeltResults = false;
            this.LastErrorMessage = null;
            return;
        }

        var farm = base.ActiveFarm;
        if (farm == null)
        {
            _logger.LogWarning("GHGResultsViewModel.RunAnalysis: no active farm; skipping.");
            this.HasResults = false;
            this.HasShelterbeltResults = false;
            this.LastErrorMessage = null;
            return;
        }

        // Sync the strategy ComboBox with the active farm. Suppressed so the resulting
        // property-changed notification doesn't recursively kick off another analysis run.
        _suppressStrategyReanalysis = true;
        try
        {
            SelectedStrategy = farm.Defaults.CarbonModellingStrategy;
        }
        finally
        {
            _suppressStrategyReanalysis = false;
        }

        IsProcessingData = true;
        try
        {
            var results = _farmAnalysisService.RunAnalysis(farm);

            this.FarmName = results.FarmName;
            this.CarbonModellingStrategy = results.CarbonModellingStrategy;
            this.YearResults = new ObservableCollection<FieldAnalysisYearResult>(results.YearResults);
            this.ShelterbeltYearResults = new ObservableCollection<ShelterbeltYearResult>(results.ShelterbeltYearResults);
            this.HasResults = results.YearResults.Count > 0;
            this.HasShelterbeltResults = results.ShelterbeltYearResults.Count > 0;
            this.LastErrorMessage = null;

            BuildSoilCarbonTrendChart(results.YearResults);

            _logger.LogInformation(
                "GHG analysis complete for {FarmName}: {FieldRows} field-year results + {ShelterbeltRows} shelterbelt-year results ({Strategy}).",
                results.FarmName, results.YearResults.Count, results.ShelterbeltYearResults.Count, results.CarbonModellingStrategy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GHG analysis failed for active farm.");
            this.LastErrorMessage = ex.Message;
            this.HasResults = false;
            this.HasShelterbeltResults = false;
        }
        finally
        {
            IsProcessingData = false;
        }
    }

    /// <summary>
    /// Build one line series per field over the union of all years present in the results. The
    /// chart is shown when there are at least two distinct years (a single-year run would just
    /// be a single point per field, which is what the DataGrid already conveys).
    /// </summary>
    private void BuildSoilCarbonTrendChart(IReadOnlyList<FieldAnalysisYearResult> yearResults)
    {
        if (yearResults.Count == 0)
        {
            this.SoilCarbonTrendSeries = Array.Empty<ISeries>();
            this.SoilCarbonTrendXAxes = Array.Empty<Axis>();
            return;
        }

        var sortedYears = yearResults.Select(r => r.Year).Distinct().OrderBy(y => y).ToList();

        var seriesByField = yearResults
            .GroupBy(r => r.FieldName)
            .OrderBy(g => g.Key)
            .Select(group =>
            {
                var byYear = group.ToDictionary(r => r.Year, r => r.SoilCarbon);
                var values = sortedYears
                    .Select(year => byYear.TryGetValue(year, out var v) ? (double?)v : null)
                    .ToArray();

                return (ISeries)new LineSeries<double?>
                {
                    Name = string.IsNullOrEmpty(group.Key) ? "(unnamed field)" : group.Key,
                    Values = values,
                    GeometrySize = 8,
                };
            })
            .ToArray();

        this.SoilCarbonTrendSeries = seriesByField;
        this.SoilCarbonTrendXAxes = new[]
        {
            new Axis
            {
                Name = "Year",
                Labels = sortedYears.Select(y => y.ToString()).ToArray(),
                LabelsRotation = 0,
            },
        };
    }

    private void ExportFieldResultsToCsv()
    {
        if (this.YearResults.Count == 0)
        {
            return;
        }

        try
        {
            var path = WriteFieldResultsCsv(this.YearResults, this.FarmName);
            _logger.LogInformation("Exported {Count} field-year rows to {Path}.", this.YearResults.Count, path);
            _notificationManager?.ShowToast(
                title: "Results exported",
                message: $"Saved to {path}",
                type: NotificationType.Success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to export field results to CSV.");
            this.LastErrorMessage = $"Export failed: {ex.Message}";
            _notificationManager?.ShowToast(
                title: "Export failed",
                message: ex.Message,
                type: NotificationType.Error);
        }
    }

    /// <summary>
    /// Writes the field-year rows to a timestamped CSV under <c>{Documents}/Holos5/Exports/</c>
    /// and returns the path. Public+static so a future test or CLI export can reuse it.
    /// </summary>
    internal static string WriteFieldResultsCsv(
        IEnumerable<FieldAnalysisYearResult> rows,
        string farmName)
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var directory = Path.Combine(documents, "Holos5", "Exports");
        Directory.CreateDirectory(directory);

        var safeFarmName = string.IsNullOrWhiteSpace(farmName) ? "Farm" : SanitizeForFilename(farmName);
        var filename = $"{safeFarmName}_GHG_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var path = Path.Combine(directory, filename);

        var csv = new StringBuilder();
        csv.AppendLine(string.Join(",", new[]
        {
            "Year", "Field", "Crop", "Area_ha",
            "AboveGroundCarbonInput_kg_per_ha", "BelowGroundCarbonInput_kg_per_ha",
            "ManureCarbonInput_kg_per_ha", "DigestateCarbonInput_kg_per_ha",
            "TotalCarbonInputs_kg_per_ha",
            "SoilCarbon_Mg_per_ha", "ChangeInSoilCarbon_Mg_per_ha_per_yr",
            "NitrogenAppliedFromManure_kg", "DirectN2O_kg_per_ha",
            "IndirectN2O_kg_per_ha", "TotalN2O_kg_per_ha",
        }));

        var inv = CultureInfo.InvariantCulture;
        foreach (var row in rows)
        {
            csv.AppendLine(string.Join(",", new[]
            {
                row.Year.ToString(inv),
                EscapeCsv(row.FieldName),
                EscapeCsv(row.CropType),
                row.Area.ToString("G", inv),
                row.AboveGroundCarbonInput.ToString("G", inv),
                row.BelowGroundCarbonInput.ToString("G", inv),
                row.ManureCarbonInput.ToString("G", inv),
                row.DigestateCarbonInput.ToString("G", inv),
                row.TotalCarbonInputs.ToString("G", inv),
                row.SoilCarbon.ToString("G", inv),
                row.ChangeInSoilCarbon.ToString("G", inv),
                row.NitrogenAppliedFromManure.ToString("G", inv),
                row.DirectN2OPerHectare.ToString("G", inv),
                row.IndirectN2OPerHectare.ToString("G", inv),
                row.TotalN2OPerHectare.ToString("G", inv),
            }));
        }

        File.WriteAllText(path, csv.ToString(), Encoding.UTF8);
        return path;
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        // Quote when the value contains commas, quotes, or line breaks (RFC 4180). Doubling a
        // quote inside a quoted field is how CSV escapes embedded quotes.
        var needsQuoting = value.IndexOfAny(new[] { ',', '"', '\r', '\n' }) >= 0;
        if (!needsQuoting)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }

    private static string SanitizeForFilename(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            sb.Append(Array.IndexOf(invalid, ch) >= 0 ? '_' : ch);
        }
        return sb.ToString();
    }

    private void ApplyStrategyAndReanalyze(CarbonModellingStrategies newStrategy)
    {
        var farm = base.ActiveFarm;
        if (farm == null)
        {
            return;
        }

        if (farm.Defaults.CarbonModellingStrategy != newStrategy)
        {
            _logger.LogInformation(
                "Switching carbon-modelling strategy for {FarmName} from {OldStrategy} to {NewStrategy}.",
                farm.Name, farm.Defaults.CarbonModellingStrategy, newStrategy);

            farm.Defaults.CarbonModellingStrategy = newStrategy;
        }

        this.RunAnalysis();
    }

    #endregion
}
