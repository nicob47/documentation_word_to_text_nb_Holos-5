using Avalonia.Controls;
using H.Avalonia.Events;
using H.Avalonia.Views.ComponentViews;
using H.Avalonia.Views.FarmCreationViews;
using H.Avalonia.Views.SupportingViews;
using H.Core.Models;
using Prism.Events;
using Prism.Regions;
using System.Linq;


namespace H.Avalonia.ViewModels.OptionsViews
{
    public class OptionsViewModel : ViewModelBase
    {
        #region Fields

        private object? _selectedItem;

        #endregion

        #region Fields

        public OptionsViewModel()
        {

        }
        public OptionsViewModel(IRegionManager regionManager, IEventAggregator eventAggregator) : base(regionManager, eventAggregator)
        {
            AllowNavigation = true;
        }

        #endregion

        #region Properties

        public object? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        #endregion

        #region Public Methods 

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.PropertyChanged += OnSelectedOptionChanged;
            EventAggregator?.GetEvent<ValidationErrorOccurredEvent>().Subscribe(LockNavigation);
            EventAggregator?.GetEvent<ValidationPassOccurredEvent>().Subscribe(UnlockNavigation);
        }

        public void OnCancelExecute()
        {
            base.RegionManager?.RequestNavigate(UiRegions.SidebarRegion, nameof(MyComponentsView));
            var activeView = this.RegionManager?.Regions[UiRegions.ContentRegion].ActiveViews.SingleOrDefault();
            if (activeView != null )
            {
                this.PropertyChanged -= OnSelectedOptionChanged;
                EventAggregator?.GetEvent<ValidationErrorOccurredEvent>().Unsubscribe(LockNavigation);
                EventAggregator?.GetEvent<ValidationPassOccurredEvent>().Unsubscribe(UnlockNavigation);
                this.RegionManager?.Regions[UiRegions.ContentRegion].Deactivate(activeView);
                this.RegionManager?.Regions[UiRegions.ContentRegion].Remove(activeView);
                SelectedItem = null; // need to set this to null because the option in the combo box stays selected otherwise
            }
        }

        #endregion

        #region Private Methods

        private void OnSelectedOptionChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (SelectedItem is ListBoxItem item && item.Content != null)
            {
                string? selectedOption = item.Content.ToString();
                switch (selectedOption)
                {
                    // File Menu
                    case "New Farm":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.FarmCreationViews.FileNewFarmView));
                        break;
                    case "Open Farm":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.FarmCreationViews.FileOpenFarmView));
                        break;
                    case "Close Farm":
                        this.ClearActiveView();
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(FarmOptionsView));
                        break;
                    case "Farms":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.FileMenuViews.FarmManagementView));
                        break;
                    case "Save Options":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.FileMenuViews.FileSaveOptionsView));
                        break;
                    case "Export Farm(s)":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.FileMenuViews.FileExportFarmView));
                        break;
                    case "Import Farm":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.FileMenuViews.FileImportFarmView));
                        break;
                    case "Export Climate":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.FileMenuViews.FileExportClimateView));
                        break;
                    case "Export Manure":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.FileMenuViews.FileExportManureView));
                        break;
                    // Settings Menu
                    case "Diets":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(DietFormulatorView));
                        break;
                    case "Ingredients":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(FeedIngredientsView));
                        break;
                    case "Farm":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.OptionFarmView));
                        break;
                    case "Soil":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.OptionSoilView));
                        break;
                    case "Soil N2O Breakdown":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.OptionSoilN2OBreakdownView));
                        break;
                    case "Barn Temperatures":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.OptionBarnTemperatureView));
                        break;
                    case "Temperature":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.OptionTemperatureView));
                        break;
                    case "Precipitation":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.OptionPrecipitationView));
                        break;
                    case "Evapotranspiration":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.OptionEvapotranspirationView));
                        break;
                    case "Default Bedding Composition":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.DefaultBeddingCompositionView));
                        break;
                    case "Default Manure Composition":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.DefaultManureCompositionView));
                        break;
                    case "User Settings":
                        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.OptionUserSettingsView));
                        break;
                }
            }
        }

        private void ClearActiveView()
        {
            // Clear content region
            var contentView = this.RegionManager?.Regions[UiRegions.ContentRegion].ActiveViews.SingleOrDefault();
            if (contentView != null)
            {
                this.RegionManager?.Regions[UiRegions.ContentRegion].Deactivate(contentView);
                this.RegionManager?.Regions[UiRegions.ContentRegion].Remove(contentView);
            }

            // Clear sidebar region
            var sidebarView = this.RegionManager?.Regions[UiRegions.SidebarRegion].ActiveViews.SingleOrDefault();
            if (sidebarView != null)
            {
                this.RegionManager?.Regions[UiRegions.SidebarRegion].Deactivate(sidebarView);
                this.RegionManager?.Regions[UiRegions.SidebarRegion].Remove(sidebarView);
            }
        }

        #endregion

        #region Event Listeners

        private void LockNavigation(ErrorInformation errorInformation)
        {
            AllowNavigation = false;
        }

        private void UnlockNavigation(ErrorInformation errorInformation)
        {
            AllowNavigation = true;
        }

        #endregion

    }
}
