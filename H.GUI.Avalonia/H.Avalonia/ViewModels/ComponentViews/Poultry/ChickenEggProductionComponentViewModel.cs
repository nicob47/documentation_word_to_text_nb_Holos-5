using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Avalonia.ViewModels.ComponentViews.OtherAnimals;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.ComponentViews.Poultry;

public class ChickenEggProductionComponentViewModel : OtherAnimalsViewModelBase
{
    public ChickenEggProductionComponentViewModel(ILogger logger, IAnimalComponentService componentService,
        IStorageService storageService, IManagementPeriodService managementPeriodService)
        : base(logger, componentService, storageService, managementPeriodService)
    {
        ViewName = "ChickenEggProductionComponentView";
        AnimalType = AnimalType.ChickenHens;
        ValidAnimalTypes = new ObservableCollection<AnimalType>(new[]
        {
            AnimalType.NotSelected,
            AnimalType.ChickenHens,
            AnimalType.ChickenCockerels,
            AnimalType.ChickenRoosters
        });
    }

    public ChickenEggProductionComponentViewModel() { }
}
