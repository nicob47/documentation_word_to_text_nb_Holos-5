using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Avalonia.ViewModels.ComponentViews.OtherAnimals;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.ComponentViews.Beef;

public class FinishingComponentViewModel : OtherAnimalsViewModelBase
{
    public FinishingComponentViewModel(ILogger logger, IAnimalComponentService componentService,
        IStorageService storageService, IManagementPeriodService managementPeriodService)
        : base(logger, componentService, storageService, managementPeriodService)
    {
        ViewName = "FinishingComponentView";
        AnimalType = AnimalType.BeefFinisher;
        ValidAnimalTypes = new ObservableCollection<AnimalType>(new[]
        {
            AnimalType.NotSelected,
            AnimalType.BeefFinishingSteer,
            AnimalType.BeefFinishingHeifer,
        });
    }

    public FinishingComponentViewModel() { }
}
