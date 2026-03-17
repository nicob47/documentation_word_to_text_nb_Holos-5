using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Avalonia.ViewModels.ComponentViews.OtherAnimals;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.ComponentViews.Beef;

public class CowCalfComponentViewModel : OtherAnimalsViewModelBase
{
    public CowCalfComponentViewModel(ILogger logger, IAnimalComponentService componentService,
        IStorageService storageService, IManagementPeriodService managementPeriodService)
        : base(logger, componentService, storageService, managementPeriodService)
    {
        ViewName = "CowCalfComponentView";
        AnimalType = AnimalType.BeefCow;
        ValidAnimalTypes = new ObservableCollection<AnimalType>(new[]
        {
            AnimalType.NotSelected,
            AnimalType.BeefCowLactating,
            AnimalType.BeefCowDry,
            AnimalType.BeefCalf,
            AnimalType.BeefBulls,
            AnimalType.BeefReplacementHeifers,
        });
    }

    public CowCalfComponentViewModel() { }
}
