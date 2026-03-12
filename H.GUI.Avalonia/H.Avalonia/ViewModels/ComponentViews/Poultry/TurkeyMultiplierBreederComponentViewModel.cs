using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Avalonia.ViewModels.ComponentViews.OtherAnimals;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.ComponentViews.Poultry;

public class TurkeyMultiplierBreederComponentViewModel : OtherAnimalsViewModelBase
{
    public TurkeyMultiplierBreederComponentViewModel(ILogger logger, IAnimalComponentService componentService,
        IStorageService storageService, IManagementPeriodService managementPeriodService)
        : base(logger, componentService, storageService, managementPeriodService)
    {
        ViewName = "TurkeyMultiplierBreederComponentView";
        AnimalType = AnimalType.Turkeys;
        ValidAnimalTypes = new ObservableCollection<AnimalType>(new[]
        {
            AnimalType.NotSelected,
            AnimalType.Turkeys,
            AnimalType.YoungTurkeyHen,
            AnimalType.TurkeyHen
        });
    }

    public TurkeyMultiplierBreederComponentViewModel() { }
}
