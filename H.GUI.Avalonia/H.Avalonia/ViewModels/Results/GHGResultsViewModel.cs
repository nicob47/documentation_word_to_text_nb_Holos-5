using System;
using System.Collections.ObjectModel;
using H.Core.Models.Results;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Prism.Regions;

namespace H.Avalonia.ViewModels.Results;

public class GHGResultsViewModel : ResultsViewModelBase
{
    #region Fields

    private ObservableCollection<ResultDto> _results = null!;
    private ILogger _logger = null!;

    #endregion

    #region Constructors

    public GHGResultsViewModel(ILogger logger, IStorageService storageService)
    {
        if (logger != null)
        {
            _logger = logger;
        }
        else
        {
            throw new ArgumentNullException(nameof(logger));
        }
    }

    #endregion

    #region Properties

    public ObservableCollection<ResultDto> Results
    {
        get => _results;
        set => SetProperty(ref _results, value);
    }

    #endregion

    #region Public Methods

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);

        this.InitializeViewModel();
    }

    public override void InitializeViewModel()
    {
        base.InitializeViewModel();

        var farm = base.ActiveFarm;
    }

    #endregion
}