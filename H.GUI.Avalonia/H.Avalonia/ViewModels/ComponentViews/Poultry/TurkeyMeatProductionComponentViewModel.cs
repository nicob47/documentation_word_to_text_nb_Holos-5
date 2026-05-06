using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Avalonia.ViewModels.ComponentViews.Animals;
using H.Avalonia.Services.DietFormulator;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.ComponentViews.Poultry;

public class TurkeyMeatProductionComponentViewModel : AnimalsViewModelBase
{
    public TurkeyMeatProductionComponentViewModel(ILogger logger, IAnimalComponentService componentService,
        IStorageService storageService, IManagementPeriodService managementPeriodService, IDietFormulatorWindowService dietFormulatorWindowService) : base(logger, componentService, storageService, managementPeriodService, dietFormulatorWindowService)
    {
        ViewName = "TurkeyMeatProductionComponentView";
        AnimalType = AnimalType.Turkeys;
        ValidAnimalTypes = new ObservableCollection<AnimalType>(new[]
        {
            AnimalType.NotSelected,
            AnimalType.Turkeys,
            AnimalType.YoungTurkeyHen,
            AnimalType.TurkeyHen
        });
    }

    public TurkeyMeatProductionComponentViewModel() { }
}
