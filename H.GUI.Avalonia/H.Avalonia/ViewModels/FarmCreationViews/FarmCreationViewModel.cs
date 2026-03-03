using System;
using System.Windows.Input;
using H.Avalonia.Views;
using H.Avalonia.Views.FarmCreationViews;
using H.Core.Services;
using H.Core.Services.StorageService;
using Prism.Commands;
using Prism.Regions;

namespace H.Avalonia.ViewModels.FarmCreationViews
{
    public class FarmCreationViewModel : ViewModelBase
    {
        
        #region Fields

        private string _farmName = string.Empty;
        private string _farmComments = string.Empty;
        private readonly IFarmHelper _farmHelper = null!;

        #endregion

        #region Constructors

        public FarmCreationViewModel()
        {
        }

        public FarmCreationViewModel(IRegionManager regionManager, IStorageService storageService, IFarmHelper farmHelper) : base(regionManager, storageService)
        {
            if (farmHelper != null)
            {
                _farmHelper = farmHelper;
            }
            else
            {
                throw new ArgumentNullException(nameof(farmHelper));
            }

            NavigateToPreviousPage = new DelegateCommand(OnNavigateToPreviousPage);
        }

        #endregion

        #region Properties

        public ICommand NavigateToPreviousPage { get; } = null!;

        public string FarmName
        {
            get => _farmName;
            set
            {
                if (SetProperty(ref _farmName, value))
                {    
                    ValidateFarmName();
                }
            }
        }

        public string FarmComments
        {
            get => _farmComments;
            set => SetProperty(ref  _farmComments, value);
        }

        #endregion

        #region Public Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
        }

        public void OnCreateNewFarmExecute()
        {
            // Validate FarmName before proceeding
            ValidateFarmName();

            if (HasErrors)
            {
                // Optionally notify the user that there are validation errors
                return;
            }

            var farm = _farmHelper.Create();
            farm.Name = FarmName;
            farm.Comments = FarmComments;

            base.StorageService?.SetActiveFarm(farm);
            base.StorageService?.Storage.ApplicationData.DisplayUnitStrings.SetStrings(farm.MeasurementSystemType);

            //base.RegionManager.RequestNavigate(UiRegions.SidebarRegion, nameof(MyComponentsView));
            //base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(ChooseComponentsView));

            // Navigate to next view
            base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(SoilDataView));
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Override this method to provide specific cleanup logic for FarmCreationViewModel resources
        /// </summary>
        protected override void CleanupResources()
        {
            // Always call base implementation first to clean up ViewModelBase resources
            base.CleanupResources();

            // Clean up DelegateCommand if it implements IDisposable
            if (NavigateToPreviousPage is IDisposable disposableCommand)
            {
                disposableCommand.Dispose();
            }

            // Clear validation errors for FarmName property if any exist
            RemoveError(nameof(FarmName));
        }

        #endregion

        #region Private Methods

        private void OnNavigateToPreviousPage()
        {
           base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(FarmOptionsView));
        }
        private void ValidateFarmName()
        {
            const string propertyName = nameof(FarmName);

            if (string.IsNullOrWhiteSpace(_farmName))
            {
                AddError(propertyName, "Farm name cannot be empty.");
            }
            else
            {
                RemoveError(propertyName);
            }
        }
        #endregion
    }
}
