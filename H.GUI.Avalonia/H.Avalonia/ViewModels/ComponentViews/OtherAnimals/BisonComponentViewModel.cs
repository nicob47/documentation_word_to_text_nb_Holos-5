using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using H.Avalonia.ViewModels.ComponentViews.Animals;

namespace H.Avalonia.ViewModels.ComponentViews.OtherAnimals
{
    public class BisonComponentViewModel : AnimalsViewModelBase
    {
        #region Constructors

        public BisonComponentViewModel()
        {
        }

        public BisonComponentViewModel(ILogger logger, IAnimalComponentService componentService, IStorageService storageService, IManagementPeriodService managementPeriodService) : base(logger, componentService, storageService, managementPeriodService)
        {
            ViewName = "Bison";
            AnimalType = AnimalType.Bison;
        }

        #endregion
    }
}
