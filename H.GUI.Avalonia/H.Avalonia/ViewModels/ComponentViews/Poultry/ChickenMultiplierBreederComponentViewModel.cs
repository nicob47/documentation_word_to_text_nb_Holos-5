using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Avalonia.ViewModels.ComponentViews.Animals;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.ComponentViews.Poultry;

public class ChickenMultiplierBreederComponentViewModel : AnimalsViewModelBase
{
    public ChickenMultiplierBreederComponentViewModel(ILogger logger, IAnimalComponentService componentService,
        IStorageService storageService, IManagementPeriodService managementPeriodService)
        : base(logger, componentService, storageService, managementPeriodService)
    {
        ViewName = "ChickenMultiplierBreederComponentView";
        AnimalType = AnimalType.Chicken;
        ValidAnimalTypes = new ObservableCollection<AnimalType>(new[]
        {
            AnimalType.NotSelected,
            AnimalType.Chicken
        });
    }

    public ChickenMultiplierBreederComponentViewModel() { }
}
