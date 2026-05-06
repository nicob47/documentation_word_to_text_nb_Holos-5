using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using H.Avalonia.ViewModels.ComponentViews.Animals;
using H.Avalonia.Services.DietFormulator;

namespace H.Avalonia.ViewModels.ComponentViews.OtherAnimals
{
    public class BisonComponentViewModel : AnimalsViewModelBase
    {
        #region Constructors

        public BisonComponentViewModel()
        {
        }

        public BisonComponentViewModel(ILogger logger, IAnimalComponentService componentService, IStorageService storageService, IManagementPeriodService managementPeriodService, IDietFormulatorWindowService dietFormulatorWindowService) : base(logger, componentService, storageService, managementPeriodService, dietFormulatorWindowService)
        {
            ViewName = "Bison";
            AnimalType = AnimalType.Bison;
        }

        #endregion
    }
}
