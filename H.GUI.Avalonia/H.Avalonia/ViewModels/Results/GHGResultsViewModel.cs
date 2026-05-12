using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

    #endregion
}
