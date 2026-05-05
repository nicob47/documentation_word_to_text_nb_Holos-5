using H.Core.Enumerations;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using H.Avalonia.ViewModels.ComponentViews.Animals;

namespace H.Avalonia.ViewModels.ComponentViews.OtherAnimals
{
    public class MulesComponentViewModel : AnimalsViewModelBase
    {
        #region Constructors

        public MulesComponentViewModel(ILogger logger, IAnimalComponentService componentService, IStorageService storageService, IManagementPeriodService managementPeriodService) : base(logger, componentService, storageService, managementPeriodService)
        {
            ViewName = "Mules";
            AnimalType = AnimalType.Mules;
        }

        public MulesComponentViewModel() 
        { 
        
        }

        #endregion
    }
}