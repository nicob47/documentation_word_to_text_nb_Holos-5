using H.Avalonia.ViewModels.FarmCreationViews;
using H.Avalonia.Views.FarmCreationViews;
using H.Core.Services.StorageService;
using Prism.Regions;

namespace H.Avalonia.ViewModels.OptionsViews.FileMenuViews
{
    public class FileOpenFarmViewModel : FarmOpenExistingViewModelBase
    {
        #region Constructors

        public FileOpenFarmViewModel(IRegionManager regionManager, IStorageService storageService) : base(regionManager, storageService)
        {

        }

        #endregion

        #region Public Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
        }

        #endregion
    }
}
