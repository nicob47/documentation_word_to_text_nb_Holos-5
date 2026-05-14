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
        RecalculateCommand = new DelegateCommand(() => _ = RunAnalysisAsync());
        ExportFieldResultsCommand = new DelegateCommand(ExportFieldResultsToCsv, () => this.HasResults);
    }

    /// <summary>
    /// Production constructor used by the DI container. Wires up the logger, storage service
    /// (needed by <see cref="ResultsViewModelBase"/> to resolve the active farm), the analysis
    /// service that produces ICBM / IPCC Tier 2 / shelterbelt results, and an optional toast
    /// notification manager for export feedback. Also subscribes to <see cref="HasResults"/> so
    /// the Export button's <c>CanExecute</c> stays in sync with the data state.
    /// </summary>
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

        // Capture dependencies. StorageService lives on the base class because every results
        // view-model needs to resolve the active farm the same way.
        _logger = logger;
        _farmAnalysisService = farmAnalysisService;
        _notificationManager = notificationManager;
        StorageService = storageService;

        // DelegateCommand doesn't natively support async, so the actions wrap a fire-and-forget
        // call to the async pipeline. RunAnalysisAsync catches its own exceptions and surfaces
        // them through LastErrorMessage, so swallowing the returned Task here is safe.
        RecalculateCommand = new DelegateCommand(() => _ = RunAnalysisAsync());
        ExportFieldResultsCommand = new DelegateCommand(ExportFieldResultsToCsv, () => this.HasResults);

        // Keep the Export button's enabled state in sync with whether we actually have data.
        // Without this the button would stay stuck in its initial CanExecute state until the
        // user interacted with it.
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

    /// <summary>
    /// Flat list of per-field, per-year carbon / nitrogen results bound to the main DataGrid.
    /// Replaced wholesale (rather than mutated in place) on each analysis run so the grid sees
    /// a single change notification and avoids flicker.
    /// </summary>
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

    /// <summary>
    /// True when the last analysis produced shelterbelt rows. The view binds the visibility of
    /// the shelterbelt section to this so farms without shelterbelts don't show an empty grid.
    /// </summary>
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

    /// <summary>
    /// Single-axis array (LiveCharts expects an array even for one axis) whose labels are the
    /// sorted, distinct years across the analysis result set.
    /// </summary>
    public Axis[] SoilCarbonTrendXAxes
    {
        get => _soilCarbonTrendXAxes;
        set => SetProperty(ref _soilCarbonTrendXAxes, value);
    }

    /// <summary>Display name of the farm the last analysis ran against — shown in the page header.</summary>
    public string FarmName
    {
        get => _farmName;
        set => SetProperty(ref _farmName, value);
    }

    /// <summary>
    /// Friendly name of the strategy actually used by the last run (e.g. "ICBM"). Distinct from
    /// <see cref="SelectedStrategy"/> which is the enum value driving the next run.
    /// </summary>
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

    /// <summary>
    /// Re-runs the analysis pipeline against the active farm. Bound to the Recalculate button;
    /// safe to invoke repeatedly because <see cref="RunAnalysisAsync"/> drops re-entrant calls.
    /// </summary>
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

    /// <summary>
    /// Prism navigation hook invoked when the GHG results page becomes active. Triggers a fresh
    /// analysis run for the currently active farm. Sync by contract — the async work is
    /// fire-and-forget so the UI thread can paint the loading state immediately.
    /// </summary>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        // Let the base class do its standard navigation bookkeeping first (active farm wiring,
        // PropertyChanged plumbing, etc.) before we kick off any work that depends on it.
        base.OnNavigatedTo(navigationContext);

        // Run any view-model-specific init hooks. Currently a pass-through but kept explicit so
        // future subclass logic always runs at the right point in the navigation lifecycle.
        this.InitializeViewModel();

        // Fire-and-forget: OnNavigatedTo is a sync Prism callback so we don't await here. The
        // async method catches its own exceptions. Awaiting via Task.Run lets the UI thread
        // repaint (showing the "Running carbon analysis..." banner + spinner) before the heavy
        // work starts.
        _ = this.RunAnalysisAsync();
    }

    /// <summary>
    /// Hook for one-time view-model initialization. Currently defers entirely to the base —
    /// kept as an override so future setup specific to the GHG view can slot in here.
    /// </summary>
    public override void InitializeViewModel()
    {
        base.InitializeViewModel();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Runs the analysis pipeline off the UI thread so the view can keep painting the
    /// "Running carbon analysis..." banner while ICBM / IPCC Tier 2 / shelterbelt math is in
    /// flight. Re-entrant calls (e.g. user clicks Recalculate twice fast) are dropped — the
    /// IsProcessingData flag gates them. Exposed as public so tests can await it directly
    /// rather than racing the fire-and-forget pattern that the GUI command actions use.
    /// </summary>
    public async Task RunAnalysisAsync()
    {
        // ---------------------------------------------------------------------------------
        // 1. Guard clauses: bail out early when there's nothing meaningful to compute.
        // ---------------------------------------------------------------------------------
        if (_farmAnalysisService is null)
        {
            // Reached only via the design-time / fallback parameterless constructor (see ctor
            // docs). Without an analysis service we have nothing to compute — bail rather than
            // NRE so the XAML previewer / a misconfigured container still renders the empty view.
            _logger.LogWarning(
                "GHGResultsViewModel.RunAnalysisAsync called without an IFarmAnalysisService; " +
                "the view model was constructed via the parameterless ctor. Check that " +
                "IFarmAnalysisService and its transitive dependencies are registered in DI.");
            this.HasResults = false;
            this.HasShelterbeltResults = false;
            this.LastErrorMessage = null;
            return;
        }

        // No farm selected yet (e.g. fresh launch, before farm creation). Clear any stale state
        // so the empty placeholder shows in the view.
        var farm = base.ActiveFarm;
        if (farm == null)
        {
            _logger.LogWarning("GHGResultsViewModel.RunAnalysisAsync: no active farm; skipping.");
            this.HasResults = false;
            this.HasShelterbeltResults = false;
            this.LastErrorMessage = null;
            return;
        }

        if (IsProcessingData)
        {
            // Re-entry guard: a previous analysis is still running (Recalculate clicked twice,
            // or strategy changed mid-run). Drop the duplicate — once the in-flight call
            // finishes, the user can click Recalculate again.
            _logger.LogDebug("GHGResultsViewModel.RunAnalysisAsync: previous run still in progress; skipping.");
            return;
        }

        // ---------------------------------------------------------------------------------
        // 2. Sync the strategy ComboBox to the farm's currently-persisted strategy.
        //    Suppressed so the resulting property-changed notification doesn't recursively kick
        //    off another analysis run via ApplyStrategyAndReanalyze.
        // ---------------------------------------------------------------------------------
        _suppressStrategyReanalysis = true;
        try
        {
            SelectedStrategy = farm.Defaults.CarbonModellingStrategy;
        }
        finally
        {
            _suppressStrategyReanalysis = false;
        }

        // ---------------------------------------------------------------------------------
        // 3. Run the actual analysis pipeline off the UI thread and project the results onto
        //    bindable view-model state. IsProcessingData drives the spinner + button enabled
        //    state; the finally block guarantees we always clear it.
        // ---------------------------------------------------------------------------------
        IsProcessingData = true;
        try
        {
            // Offload the heavy carbon/N pipeline to a background thread. The await yields
            // control back to the UI thread, which gets to paint the processing banner and
            // disable the Recalculate / Strategy controls (both gated on !IsProcessingData)
            // before the pipeline starts churning. When Task.Run completes, the continuation
            // resumes on the captured sync context — Avalonia's UI thread — so the view-model
            // property updates and ObservableCollection construction below are safe.
            var results = await Task.Run(() => _farmAnalysisService.RunAnalysis(farm));

            // Project the immutable result snapshot onto the observable view-model surface.
            // Each assignment fires a PropertyChanged event that the view picks up.
            this.FarmName = results.FarmName;
            this.CarbonModellingStrategy = results.CarbonModellingStrategy;
            this.YearResults = new ObservableCollection<FieldAnalysisYearResult>(results.YearResults);
            this.ShelterbeltYearResults = new ObservableCollection<ShelterbeltYearResult>(results.ShelterbeltYearResults);
            this.HasResults = results.YearResults.Count > 0;
            this.HasShelterbeltResults = results.ShelterbeltYearResults.Count > 0;
            this.LastErrorMessage = null;

            // Build the soil-C trend chart from the same rows. Kept as a separate call so chart
            // failures don't poison the rest of the results UI.
            BuildSoilCarbonTrendChart(results.YearResults);

            _logger.LogInformation(
                "GHG analysis complete for {FarmName}: {FieldRows} field-year results + {ShelterbeltRows} shelterbelt-year results ({Strategy}).",
                results.FarmName, results.YearResults.Count, results.ShelterbeltYearResults.Count, results.CarbonModellingStrategy);
        }
        catch (Exception ex)
        {
            // Surface failures through LastErrorMessage so the view's error banner picks them up
            // rather than crashing the page. The carbon pipeline has many ways to throw on
            // misconfigured data and we never want to take down the whole results window.
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
    /// Wrapped in a try/catch — LiveChartsCore 2.0.0-rc1 has had IndexOutOfRange crashes in its
    /// renderer (notably SKDefaultLegend.Draw) for edge-case inputs; if we hit one we degrade to
    /// "no chart" rather than tearing down the whole window.
    /// </summary>
    private void BuildSoilCarbonTrendChart(IReadOnlyList<FieldAnalysisYearResult> yearResults)
    {
        try
        {
            BuildSoilCarbonTrendChartCore(yearResults);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Soil-C trend chart build failed; hiding chart for this run.");
            this.SoilCarbonTrendSeries = Array.Empty<ISeries>();
            this.SoilCarbonTrendXAxes = Array.Empty<Axis>();
        }
    }

    /// <summary>
    /// Core chart-building routine. Groups rows by field, averages duplicate (field, year)
    /// entries, and projects each field onto the common sorted-years axis with nulls for missing
    /// slots so LiveCharts renders gaps instead of fake zeros.
    /// </summary>
    private void BuildSoilCarbonTrendChartCore(IReadOnlyList<FieldAnalysisYearResult> yearResults)
    {
        // Nothing to chart — reset to empty so the view hides any previous chart.
        if (yearResults.Count == 0)
        {
            this.SoilCarbonTrendSeries = Array.Empty<ISeries>();
            this.SoilCarbonTrendXAxes = Array.Empty<Axis>();
            return;
        }

        // Build the shared x-axis: all distinct years across all fields, sorted ascending. Every
        // field's series is projected onto this same axis so the lines line up visually.
        var sortedYears = yearResults.Select(r => r.Year).Distinct().OrderBy(y => y).ToList();

        // One LineSeries per field, ordered alphabetically so the legend is stable across runs.
        var seriesByField = yearResults
            .GroupBy(r => r.FieldName)
            .OrderBy(g => g.Key)
            .Select(group =>
            {
                // Use a grouped lookup rather than ToDictionary so duplicate (field, year) rows
                // (e.g. a main crop + a cover crop in the same year that didn't get merged by
                // CombineCarbonInputs) don't blow up with ArgumentException — take the average
                // for the year instead. The DataGrid still shows the raw rows; this is only the
                // trend-chart smoothing.
                var byYear = group
                    .GroupBy(r => r.Year)
                    .ToDictionary(g => g.Key, g => g.Average(r => r.SoilCarbon));

                // Project this field onto the full year axis: null where the field has no data
                // for that year so LiveCharts draws a gap instead of dropping the line to 0.
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

        // Publish the assembled series + axis to the view. Single assignment of each so the
        // chart re-renders once rather than flickering through partial states.
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

    /// <summary>
    /// Command action behind <see cref="ExportFieldResultsCommand"/>. Delegates the actual file
    /// write to <see cref="WriteFieldResultsCsv"/> and reports success or failure via toast +
    /// <see cref="LastErrorMessage"/>. No-ops when there are no rows to export.
    /// </summary>
    private void ExportFieldResultsToCsv()
    {
        // Defensive no-op. CanExecute already prevents the button from firing when there are no
        // rows, but this also covers programmatic invocation from tests / future callers.
        if (this.YearResults.Count == 0)
        {
            return;
        }

        try
        {
            // Do the actual file write. Returns the full path so we can show it in the toast.
            var path = WriteFieldResultsCsv(this.YearResults, this.FarmName);
            _logger.LogInformation("Exported {Count} field-year rows to {Path}.", this.YearResults.Count, path);
            _notificationManager?.ShowToast(
                title: "Results exported",
                message: $"Saved to {path}",
                type: NotificationType.Success);
        }
        catch (Exception ex)
        {
            // Mirror the analysis pipeline's error-handling pattern: log, surface the message in
            // LastErrorMessage for the inline banner, and pop a toast for immediate visibility.
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
        // 1. Resolve and ensure the target export folder exists. We use MyDocuments so the file
        //    is easy for the user to find regardless of where the app was installed.
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var directory = Path.Combine(documents, "Holos5", "Exports");
        Directory.CreateDirectory(directory);

        // 2. Build a safe, timestamped filename so concurrent / repeated exports don't collide.
        var safeFarmName = string.IsNullOrWhiteSpace(farmName) ? "Farm" : SanitizeForFilename(farmName);
        var filename = $"{safeFarmName}_GHG_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var path = Path.Combine(directory, filename);

        // 3. Emit the header row. Column order here drives the order in the file.
        var csv = new StringBuilder();
        csv.AppendLine(string.Join(",", new[]
        {
            "Year", "Field", "Crop", "Area_ha",
            "AboveGroundCarbonInput_kg_per_ha", "BelowGroundCarbonInput_kg_per_ha",
            "ManureCarbonInput_kg_per_ha", "DigestateCarbonInput_kg_per_ha",
            "TotalCarbonInputs_kg_per_ha",
            "SoilCarbon_kg_per_ha", "ChangeInSoilCarbon_kg_per_ha_per_yr",
            "NitrogenAppliedFromManure_kg", "DirectN2O_kg_per_ha",
            "IndirectN2O_kg_per_ha", "TotalN2O_kg_per_ha",
        }));

        // 4. Format each data row. InvariantCulture keeps the decimal separator as '.' so the
        //    CSV is portable across French / English Windows locales (and re-importable into Excel
        //    / R / Python without locale-specific parsing tricks).
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

        // 5. Persist as UTF-8 (with no BOM) so Excel on Windows still opens it correctly and
        //    accents in farm / field names survive the round-trip.
        File.WriteAllText(path, csv.ToString(), Encoding.UTF8);
        return path;
    }

    /// <summary>
    /// RFC 4180 CSV field escaping: wraps the value in double quotes when it contains a comma,
    /// quote, or line break, and doubles any embedded quotes. Returns the input unchanged when
    /// no escaping is needed.
    /// </summary>
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

    /// <summary>
    /// Replaces any character that is illegal in a Windows filename with an underscore so the
    /// farm name can be safely embedded in the export filename.
    /// </summary>
    private static string SanitizeForFilename(string value)
    {
        // Path.GetInvalidFileNameChars() is platform-specific but covers anything the OS would
        // reject. Anything matching gets swapped for an underscore so the resulting name is safe.
        var invalid = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            sb.Append(Array.IndexOf(invalid, ch) >= 0 ? '_' : ch);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Persists the new <see cref="CarbonModellingStrategies"/> selection onto the active farm
    /// and kicks off a fresh analysis run so the grid reflects the change immediately. Invoked
    /// from the <see cref="SelectedStrategy"/> setter unless re-analysis has been suppressed
    /// (e.g. while we're syncing the ComboBox to the farm's current value).
    /// </summary>
    private void ApplyStrategyAndReanalyze(CarbonModellingStrategies newStrategy)
    {
        // No farm → nothing to persist or re-run. Mirrors the guard in RunAnalysisAsync.
        var farm = base.ActiveFarm;
        if (farm == null)
        {
            return;
        }

        // Only mutate + log when the strategy actually changed. Avoids spurious log noise and
        // keeps the farm's dirty state accurate.
        if (farm.Defaults.CarbonModellingStrategy != newStrategy)
        {
            _logger.LogInformation(
                "Switching carbon-modelling strategy for {FarmName} from {OldStrategy} to {NewStrategy}.",
                farm.Name, farm.Defaults.CarbonModellingStrategy, newStrategy);

            farm.Defaults.CarbonModellingStrategy = newStrategy;
        }

        // Fire-and-forget — same pattern as OnNavigatedTo. The ComboBox setter is sync; the
        // async analysis runs on a background thread and the UI updates when it finishes.
        _ = this.RunAnalysisAsync();
    }

    #endregion
}
