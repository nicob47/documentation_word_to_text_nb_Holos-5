using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using H.Core.Enumerations;
using H.Core.Models.Results;
using H.Core.Services.Analysis;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
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

    private ObservableCollection<FieldAnalysisYearResult> _yearResults = new();
    private string _farmName = string.Empty;
    private string _carbonModellingStrategy = string.Empty;
    private bool _hasResults;
    private string? _lastErrorMessage;
    private CarbonModellingStrategies _selectedStrategy;
    private bool _suppressStrategyReanalysis;

    #endregion

    #region Constructors

    public GHGResultsViewModel()
    {
        _logger = null!;
        _farmAnalysisService = null!;
        RecalculateCommand = new DelegateCommand(RunAnalysis);
    }

    public GHGResultsViewModel(ILogger logger, IStorageService storageService, IFarmAnalysisService farmAnalysisService)
        : base()
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(storageService);
        ArgumentNullException.ThrowIfNull(farmAnalysisService);

        _logger = logger;
        _farmAnalysisService = farmAnalysisService;
        StorageService = storageService;

        RecalculateCommand = new DelegateCommand(RunAnalysis);
    }

    #endregion

    #region Properties

    public ObservableCollection<FieldAnalysisYearResult> YearResults
    {
        get => _yearResults;
        set => SetProperty(ref _yearResults, value);
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
        var farm = base.ActiveFarm;
        if (farm == null)
        {
            _logger.LogWarning("GHGResultsViewModel.RunAnalysis: no active farm; skipping.");
            this.HasResults = false;
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
            this.HasResults = !results.IsEmpty;
            this.LastErrorMessage = null;

            _logger.LogInformation(
                "GHG analysis complete for {FarmName}: {RowCount} per-field year results ({Strategy}).",
                results.FarmName, results.YearResults.Count, results.CarbonModellingStrategy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GHG analysis failed for active farm.");
            this.LastErrorMessage = ex.Message;
            this.HasResults = false;
        }
        finally
        {
            IsProcessingData = false;
        }
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
