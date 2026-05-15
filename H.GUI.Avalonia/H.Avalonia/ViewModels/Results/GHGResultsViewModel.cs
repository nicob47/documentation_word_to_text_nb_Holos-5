using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Notifications;
using ClosedXML.Excel;
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
/// ViewModel for the GHG &amp; Carbon results page (<c>GHGResultsView</c>). Sits between the
/// user's "Run Analysis" / "Recalculate" / strategy-toggle clicks and the
/// <see cref="IFarmAnalysisService"/> calculator stack.
///
/// <para><b>Responsibilities:</b></para>
/// <list type="bullet">
///   <item>Resolve the active farm and call <c>IFarmAnalysisService.RunAnalysis</c> on a background thread (<see cref="RunAnalysisAsync"/>).</item>
///   <item>Project <see cref="FarmAnalysisResults"/> onto bindable <see cref="ObservableCollection{T}"/> surfaces (<see cref="YearResults"/>, <see cref="ShelterbeltYearResults"/>) for the DataGrids.</item>
///   <item>Build the soil-C trend chart (<see cref="SoilCarbonTrendSeries"/>) — one LineSeries per field across the union of years.</item>
///   <item>Expose the strategy ComboBox (<see cref="SelectedStrategy"/>): flipping ICBM ↔ Tier 2 writes back to <c>farm.Defaults.CarbonModellingStrategy</c> and re-runs the analysis.</item>
///   <item>Surface errors from the calculator stack via <see cref="LastErrorMessage"/>, which the view's error banner shows.</item>
///   <item>Export field results to CSV (static helper) or Excel via <see cref="ExportFieldResultsToExcel"/>.</item>
/// </list>
///
/// <para><b>Threading note:</b></para>
/// The heavy carbon / N pipeline runs inside <c>Task.Run</c> so the UI thread stays free to paint
/// the "Running carbon analysis..." banner. The continuation back on the UI thread is what writes
/// the ObservableCollections — anything that needs the UI dispatcher (chart series, collection
/// changes) is on that side of the <c>await</c>.
///
/// <para>
/// See <c>Carbon_Model_Flow.md</c> for the full pipeline diagram and how this VM fits in.
/// </para>
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
        RecalculateCommand = new DelegateCommand(() => { InvalidateFieldStageState(); _ = RunAnalysisAsync(); });
        ExportFieldResultsCommand = new DelegateCommand(ExportFieldResultsToExcel, () => this.HasResults);
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
        // Recalculate explicitly invalidates the field stage state so any edits the user made in
        // the field component editor are picked up; strategy switches don't, because the strategy
        // doesn't affect detail-item inputs and rebuilding the stage state on every combo-box
        // tick is the dominant cost on large farms.
        RecalculateCommand = new DelegateCommand(() => { InvalidateFieldStageState(); _ = RunAnalysisAsync(); });
        ExportFieldResultsCommand = new DelegateCommand(ExportFieldResultsToExcel, () => this.HasResults);

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
        // Invalidate the field stage state on nav-in so any edits the user just made in the
        // field component editor are reflected on this fresh visit to the results page.
        InvalidateFieldStageState();
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
    /// Marks the active farm's field stage state as out-of-date so the next analysis run rebuilds
    /// its detail view items from the current field components. Cheap no-op when there is no
    /// active farm or no stage state yet. Called only on explicit user intents (Recalculate, fresh
    /// navigation into the results page) — strategy combo-box changes deliberately skip this so
    /// they don't trigger an expensive stage-state rebuild that wouldn't change the inputs anyway.
    /// </summary>
    private void InvalidateFieldStageState()
    {
        var farm = base.ActiveFarm;
        var stageState = farm?.StageStates
            .OfType<H.Core.Models.LandManagement.Fields.FieldSystemDetailsStageState>()
            .SingleOrDefault();
        if (stageState != null)
        {
            stageState.IsInitialized = false;
        }
    }

    /// <summary>
    /// Command action behind <see cref="ExportFieldResultsCommand"/>. Delegates the actual file
    /// write to <see cref="WriteFieldResultsXlsx"/> and reports success or failure via toast +
    /// <see cref="LastErrorMessage"/>. No-ops when there are no rows to export.
    /// </summary>
    private void ExportFieldResultsToExcel()
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
            var path = WriteFieldResultsXlsx(this.YearResults, this.FarmName);
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
            _logger.LogError(ex, "Failed to export field results to Excel.");
            this.LastErrorMessage = $"Export failed: {ex.Message}";
            _notificationManager?.ShowToast(
                title: "Export failed",
                message: ex.Message,
                type: NotificationType.Error);
        }
    }

    /// <summary>
    /// Writes the field-year rows to a styled, timestamped <c>.xlsx</c> file under
    /// <c>{Documents}/Holos5/Exports/</c> and returns the path. Uses ClosedXML so we get a real
    /// Excel workbook with a coloured header row, banded data rows, frozen header, number
    /// formatting, and auto-sized columns \u2014 i.e. something the user can hand to a stakeholder
    /// without further massage. Internal+static so tests can exercise the writer directly.
    /// </summary>
    internal static string WriteFieldResultsXlsx(
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
        var filename = $"{safeFarmName}_GHG_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var path = Path.Combine(directory, filename);

        // 3. Define the column schema in one place so header text, value extraction, and number
        //    format stay aligned. Each tuple: (header, value selector, Excel number format).
        //    "0.###" gives up to 3 decimals without trailing zeros; "0" is whole numbers for Year.
        var columns = new (string Header, Func<FieldAnalysisYearResult, object?> Value, string Format)[]
        {
            ("Year",                              r => r.Year,                       "0"),
            ("Field",                             r => r.FieldName,                  string.Empty),
            ("Crop",                              r => r.CropType,                   string.Empty),
            ("Area (ha)",                         r => r.Area,                       "0.##"),
            ("Above-ground C input (kg C/ha)",    r => r.AboveGroundCarbonInput,     "0.###"),
            ("Below-ground C input (kg C/ha)",    r => r.BelowGroundCarbonInput,     "0.###"),
            ("Manure C input (kg C/ha)",          r => r.ManureCarbonInput,          "0.###"),
            ("Digestate C input (kg C/ha)",       r => r.DigestateCarbonInput,       "0.###"),
            ("Total C inputs (kg C/ha)",          r => r.TotalCarbonInputs,          "0.###"),
            ("Soil C (kg C/ha)",                  r => r.SoilCarbon,                 "0.###"),
            ("\u0394 Soil C (kg C/ha/yr)",        r => r.ChangeInSoilCarbon,         "0.###"),
            ("N from manure (kg N)",              r => r.NitrogenAppliedFromManure,  "0.###"),
            ("Direct N\u2082O (kg N\u2082O/ha)",  r => r.DirectN2OPerHectare,        "0.###"),
            ("Indirect N\u2082O (kg N\u2082O/ha)", r => r.IndirectN2OPerHectare,     "0.###"),
            ("Total N\u2082O (kg N\u2082O/ha)",   r => r.TotalN2OPerHectare,         "0.###"),
        };

        // 4. Build the workbook in-memory, then save once at the end. ClosedXML's `using` disposes
        //    the underlying OpenXML package, which is what actually flushes bytes to disk.
        using (var workbook = new XLWorkbook())
        {
            var sheet = workbook.Worksheets.Add("GHG Results");

            // 4a. Title row: farm name + export timestamp, merged across all columns. Gives the
            //     reader instant context if the file is renamed or shared without its filename.
            var titleRange = sheet.Range(1, 1, 1, columns.Length).Merge();
            titleRange.Value = string.IsNullOrWhiteSpace(farmName)
                ? $"GHG Results \u2014 exported {DateTime.Now:yyyy-MM-dd HH:mm}"
                : $"{farmName} \u2014 GHG Results \u2014 exported {DateTime.Now:yyyy-MM-dd HH:mm}";
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 14;
            titleRange.Style.Font.FontColor = XLColor.White;
            titleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#0E5C2F"); // deep AAFC-ish green
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            sheet.Row(1).Height = 22;

            // 4b. Header row (row 2): white text on a lighter green band, bold and centred.
            for (var c = 0; c < columns.Length; c++)
            {
                var cell = sheet.Cell(2, c + 1);
                cell.Value = columns[c].Header;
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#2E7D32");
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Alignment.WrapText = true;
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                cell.Style.Border.BottomBorderColor = XLColor.FromHtml("#0E5C2F");
            }
            sheet.Row(2).Height = 32;

            // 4c. Data rows. Banded fill alternates light green / white so long tables stay
            //     readable. Numeric cells get a format string; text cells stay default.
            var rowIndex = 3;
            foreach (var row in rows)
            {
                for (var c = 0; c < columns.Length; c++)
                {
                    var cell = sheet.Cell(rowIndex, c + 1);
                    var value = columns[c].Value(row);

                    // ClosedXML's `Value` setter is type-aware: doubles become numeric cells,
                    // strings become text cells. Null becomes blank. NaN / +Inf / -Inf are not
                    // representable in the OOXML number-cell schema — ClosedXML throws
                    // ArgumentException("Value can't be NaN or infinity") if we hand it one.
                    // Coerce those to a blank cell so the export still succeeds when the analysis
                    // produced non-finite values (e.g. divide-by-zero in N₂O-per-hectare for a
                    // zero-area row).
                    if (value is null) { cell.Clear(); }
                    else if (value is double d)
                    {
                        if (double.IsNaN(d) || double.IsInfinity(d)) { cell.Clear(); }
                        else { cell.Value = d; }
                    }
                    else if (value is int i) { cell.Value = i; }
                    else { cell.Value = value.ToString(); }

                    if (!string.IsNullOrEmpty(columns[c].Format))
                    {
                        cell.Style.NumberFormat.Format = columns[c].Format;
                    }
                }

                // Zebra striping on every other data row.
                if ((rowIndex & 1) == 1)
                {
                    sheet.Range(rowIndex, 1, rowIndex, columns.Length)
                        .Style.Fill.BackgroundColor = XLColor.FromHtml("#F1F8F4");
                }

                rowIndex++;
            }

            // 4d. Final layout polish: freeze the title + header so scrolling keeps them in view,
            //     auto-size columns to fit content, add a thin outer border, and turn on filters.
            sheet.SheetView.FreezeRows(2);
            sheet.Columns().AdjustToContents();
            // Cap super-wide auto-fit columns so a single long crop name doesn't blow out the layout.
            foreach (var col in sheet.ColumnsUsed())
            {
                if (col.Width > 32) { col.Width = 32; }
            }

            var dataRange = sheet.Range(2, 1, rowIndex - 1, columns.Length);
            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            dataRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#0E5C2F");
            dataRange.SetAutoFilter();

            // 5. Persist the workbook. ClosedXML writes a real OOXML .xlsx \u2014 Excel, LibreOffice,
            //    Google Sheets, and pandas can all open it without conversion.
            workbook.SaveAs(path);
        }

        return path;
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
