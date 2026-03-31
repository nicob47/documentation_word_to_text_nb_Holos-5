using DynamicData;
using H.Avalonia.Services;
using H.Avalonia.Views.ComponentViews;
using H.Avalonia.Views.FarmCreationViews;
using H.Core.Models;
using H.Core.Services.StorageService;
using Prism.Commands;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Linq;

namespace H.Avalonia.ViewModels.FarmCreationViews
{
    public class FarmOpenExistingViewmodel : FarmOpenExistingViewModelBase
    {
        #region Fields

        #endregion

        #region Constructors

        public FarmOpenExistingViewmodel(IRegionManager regionManager, IStorageService storageService, INotificationManagerService notificationManager) : base(regionManager, storageService, notificationManager)
        {
            NavigateToPreviousPage = new DelegateCommand(OnNavigateToPreviousPage);
        }

        #endregion

        #region Properties

        public DelegateCommand NavigateToPreviousPage { get; } = null!;

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private void OnNavigateToPreviousPage()
        {
            RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(FarmOptionsView));
        }

        #endregion

        #region Event Handler

        #endregion
    }
}
