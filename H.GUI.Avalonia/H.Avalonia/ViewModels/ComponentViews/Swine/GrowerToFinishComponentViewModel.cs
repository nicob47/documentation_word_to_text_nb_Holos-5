using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Avalonia.ViewModels.ComponentViews.OtherAnimals;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.ComponentViews.Swine;

public class GrowerToFinishComponentViewModel : OtherAnimalsViewModelBase
{
    public GrowerToFinishComponentViewModel(ILogger logger, IAnimalComponentService componentService,
        IStorageService storageService, IManagementPeriodService managementPeriodService)
        : base(logger, componentService, storageService, managementPeriodService)
    {
        ViewName = "GrowerToFinishComponentView";
        AnimalType = AnimalType.SwineGrower;
        ValidAnimalTypes = new ObservableCollection<AnimalType>(new[]
        {
            AnimalType.NotSelected,
            AnimalType.SwineGrower,
            AnimalType.SwineFinisher,
        });
    }

    public GrowerToFinishComponentViewModel() { }
}
