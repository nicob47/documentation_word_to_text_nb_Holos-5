using Avalonia.Controls;
using H.Avalonia.Events;
using H.Avalonia.Views;
using H.Avalonia.Views.ComponentViews;
using H.Avalonia.Views.ResultViews;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Avalonia.ViewModels.Results
{
    public class ResultsSidebarViewModel : ViewModelBase
    {
        #region Fields

        private object _selectedItem;
        private bool _isAdvancedMode = false;
        private object _lastSelectedAdvancedItem;

        #endregion

        #region Constructors

        public ResultsSidebarViewModel(IRegionManager regionManager) : base(regionManager)
        {

        }

        #endregion

        #region Properties

        public object SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public object LastSelectedAdvancedItem
        {
            get => _lastSelectedAdvancedItem;
            set
            {
                if (!IsAdvancedMode)
                {
                    SetProperty(ref _lastSelectedAdvancedItem, value);
                }
            }
        }

        public bool IsAdvancedMode
        {
            get => _isAdvancedMode;
            set
            {
                if (SetProperty(ref _isAdvancedMode, value))
                {
                    OnResultModeChanged();
                }
            }
        }

        #endregion

        #region Public Methods

        public void OnGoBackExecuted()
        {
            this.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(BlankView));
            this.RegionManager.RequestNavigate(UiRegions.SidebarRegion, nameof(MyComponentsView));
        }

        public void OnOptionsExecute()
        {
            base.RegionManager.RequestNavigate(UiRegions.SidebarRegion, nameof(Views.OptionsViews.OptionsView));
            base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.SelectOptionView));
        }

        #endregion

        #region Private Methods

        private void OnSelectedOptionChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (SelectedItem is ListBoxItem item && item.Content != null)
            {
                string? selectedOption = item.Content.ToString();
                switch (selectedOption)
                {
                    case var _ when selectedOption == H.Core.Properties.Resources.TitleMultiYearCarbonModelling:
                        base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(MultiYearCarbonModellingView));

                        break;
                    case var _ when selectedOption == H.Core.Properties.Resources.TitleEstimatesOfProduction:
                        base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(EstimatesOfProductionView));
                        break;
                    case var _ when selectedOption == H.Core.Properties.Resources.TitleFeedEstimateReport:
                        base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(FeedEstimateReportView));
                        break;
                    case var _ when selectedOption == H.Core.Properties.Resources.LabelManureManagement:
                        base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(ManureManagementResultsView));
                        break;
                    case var _ when selectedOption == H.Core.Properties.Resources.TitleEmissionsPieChart:
                        base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(EmissionPieChartView));
                        break;
                    case var _ when selectedOption == H.Core.Properties.Resources.TitleOverallEmissions:
                        base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(OverallEmissionsResultsView));
                        break;
                    case var _ when selectedOption == H.Core.Properties.Resources.TitleComponentEmissions:
                        base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(ComponentEmissionsResultsView));
                        break;
                    case var _ when selectedOption == H.Core.Properties.Resources.TitleDetailedEmissionsReport:
                        base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(DetailedEmissionsReportResultsView));
                        break;
                }
            }
        }

        #endregion

        #region Event Handlers

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.PropertyChanged += OnSelectedOptionChanged;
            SelectedItem = new ListBoxItem { Content = H.Core.Properties.Resources.TitleMultiYearCarbonModelling };
        }

        private void OnResultModeChanged()
        {
            // Store current advanced tab, navigate to summary when switching to basic
            if (!IsAdvancedMode)
            {
                //_lastSelectedAdvancedItem = _selectedItem;
                base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(ResultsSummaryView));
            }
            // Restore last advanced tab if switching back to advanced
            else
            {
                //SelectedItem = _lastSelectedAdvancedItem;
                base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(MultiYearCarbonModellingView));
            }
        }

        #endregion
    }
}
