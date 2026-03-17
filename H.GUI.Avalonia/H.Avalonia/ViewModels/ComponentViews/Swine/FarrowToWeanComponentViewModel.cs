using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Avalonia.ViewModels.ComponentViews.OtherAnimals;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.ComponentViews.Swine;

public class FarrowToWeanComponentViewModel : OtherAnimalsViewModelBase
{
    public FarrowToWeanComponentViewModel(ILogger logger, IAnimalComponentService componentService,
        IStorageService storageService, IManagementPeriodService managementPeriodService)
        : base(logger, componentService, storageService, managementPeriodService)
    {
        ViewName = "FarrowToWeanComponentView";
        AnimalType = AnimalType.Swine;
        ValidAnimalTypes = new ObservableCollection<AnimalType>(new[]
        {
            AnimalType.NotSelected,
            AnimalType.SwineSows,
            AnimalType.SwineBoar,
            AnimalType.SwineGilts,
            AnimalType.SwineLactatingSow,
            AnimalType.SwineDrySow,
            AnimalType.SwineStarter,
        });
    }

    public FarrowToWeanComponentViewModel() { }
}
