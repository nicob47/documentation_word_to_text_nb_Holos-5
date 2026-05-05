using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Avalonia.ViewModels.ComponentViews.Animals;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.ComponentViews.Beef;

public class BackgroundingComponentViewModel : AnimalsViewModelBase
{
    public BackgroundingComponentViewModel(ILogger logger, IAnimalComponentService componentService,
        IStorageService storageService, IManagementPeriodService managementPeriodService)
        : base(logger, componentService, storageService, managementPeriodService)
    {
        ViewName = "BackgroundingComponentView";
        AnimalType = AnimalType.BeefBackgrounder;
        ValidAnimalTypes = new ObservableCollection<AnimalType>(new[]
        {
            AnimalType.NotSelected,
            AnimalType.BeefBackgrounderSteer,
            AnimalType.BeefBackgrounderHeifer,
            AnimalType.StockerSteers,
            AnimalType.StockerHeifers,
        });
    }

    public BackgroundingComponentViewModel() { }
}
