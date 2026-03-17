using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Avalonia.ViewModels.ComponentViews.OtherAnimals;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.ComponentViews.Poultry;

public class ChickenMeatProductionComponentViewModel : OtherAnimalsViewModelBase
{
    public ChickenMeatProductionComponentViewModel(ILogger logger, IAnimalComponentService componentService,
        IStorageService storageService, IManagementPeriodService managementPeriodService)
        : base(logger, componentService, storageService, managementPeriodService)
    {
        ViewName = "ChickenMeatProductionComponentView";
        AnimalType = AnimalType.Chicken;
        ValidAnimalTypes = new ObservableCollection<AnimalType>(new[]
        {
            AnimalType.NotSelected,
            AnimalType.Chicken,
            AnimalType.Broilers
        });
    }

    public ChickenMeatProductionComponentViewModel() { }
}
