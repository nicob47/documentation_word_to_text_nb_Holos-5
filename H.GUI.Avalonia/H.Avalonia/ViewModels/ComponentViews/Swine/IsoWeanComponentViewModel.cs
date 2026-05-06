using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Avalonia.ViewModels.ComponentViews.Animals;
using H.Avalonia.Services.DietFormulator;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.ComponentViews.Swine;

public class IsoWeanComponentViewModel : AnimalsViewModelBase
{
    public IsoWeanComponentViewModel(ILogger logger, IAnimalComponentService componentService,
        IStorageService storageService, IManagementPeriodService managementPeriodService, IDietFormulatorWindowService dietFormulatorWindowService) : base(logger, componentService, storageService, managementPeriodService, dietFormulatorWindowService)
    {
        ViewName = "IsoWeanComponentView";
        AnimalType = AnimalType.SwineStarter;
        ValidAnimalTypes = new ObservableCollection<AnimalType>(new[]
        {
            AnimalType.NotSelected,
            AnimalType.SwineStarter,
        });
    }

    public IsoWeanComponentViewModel() { }
}
