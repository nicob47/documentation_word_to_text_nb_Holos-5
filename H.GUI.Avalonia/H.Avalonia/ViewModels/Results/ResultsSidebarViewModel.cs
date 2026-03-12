using Avalonia.Controls;
using H.Avalonia.Events;
using H.Avalonia.Views;
using H.Avalonia.Views.ComponentViews;
using H.Avalonia.Views.ResultViews;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H.Avalonia.ViewModels.Results
{
    public class ResultsSidebarViewModel : ViewModelBase
    {
        #region Fields

        private bool _isAdvancedMode = false;

        private ListBoxItem _selectedBasicItem;
        private ListBoxItem _selectedAdvancedItem;
        private ListBoxItem _lastSelectedBasicItem;
        private ListBoxItem _lastSelectedAdvancedItem;

        private ObservableCollection<ListBoxItem> _basicViewChapters = new();
        private ObservableCollection<ListBoxItem> _advancedViewTabs = new();

        #endregion

        #region Constructors

        public ResultsSidebarViewModel(IRegionManager regionManager) : base(regionManager)
        {
            PopulateBasicChaptersTitles();
            PopulateAdvancedTabs();
            this.PropertyChanged += OnSelectedOptionChanged;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The currently selected chapter in the basic view. Changing this will navigate to the corresponding chapter in the summary view.
        /// </summary>
        public ListBoxItem SelectedBasicItem
        {
            get => _selectedBasicItem;
            set
            {
                SetProperty(ref _selectedBasicItem, value);
            }
        }

        /// <summary>
        /// The currently selected tab in the advanced view. Changing this will navigate to the corresponding tab in the advanced results view.
        /// </summary>
        public ListBoxItem SelectedAdvancedItem
        {
            get => _selectedAdvancedItem;
            set
            {
                SetProperty(ref _selectedAdvancedItem, value);
            }

        }

        /// <summary>
        /// An observable collection of chapters to display in the sidebar when in basic mode. The first item will be selected by default if no chapter is currently selected.
        /// </summary>
        public ObservableCollection<ListBoxItem> BasicViewChapters
        {
            get
            {
                if (SelectedBasicItem == null)
                    SelectedBasicItem = _basicViewChapters.FirstOrDefault();
                return _basicViewChapters;
            }
            set => SetProperty(ref _basicViewChapters, value);
        }

        /// <summary>
        /// An observable collection of tabs to display in the sidebar when in advanced mode. The first item will be selected by default if no tab is currently selected.
        /// </summary>
        public ObservableCollection<ListBoxItem> AdvancedViewTabs
        {
            get
            {
                if (SelectedAdvancedItem == null)
                    SelectedAdvancedItem = _advancedViewTabs.FirstOrDefault();
                return _advancedViewTabs;
            }
            set => SetProperty(ref _advancedViewTabs, value);
        }

        /// <summary>
        /// Boolean that indicates whether the sidebar is in basic or advanced mode. 
        /// </summary>
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
            if (SelectedAdvancedItem is ListBoxItem item && item.Content != null)
            {
                string? selectedOption = item.Content.ToString();
                if (!IsAdvancedMode)
                {
                    NavigateToBasicChapter(selectedOption);
                }
                else
                    NavigateToAdvancedTab(selectedOption);
            }
        }

        private void PopulateBasicChaptersTitles()
        {
            _basicViewChapters.Add(new ListBoxItem() { Content = "Farm Profile" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "Annual Production Summary" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "Total GHG Emissions" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "Emissions Breakdown By Category" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "Carbon Sequestration Summary" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "Manure Management Overview" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "Key Findings and Recommendations" });
        }

        private void PopulateAdvancedTabs()
        {
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Core.Properties.Resources.TitleMultiYearCarbonModelling });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Core.Properties.Resources.TitleEstimatesOfProduction });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Core.Properties.Resources.TitleFeedEstimateReport });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Core.Properties.Resources.LabelManureManagement });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Core.Properties.Resources.TitleEmissionsPieChart });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Core.Properties.Resources.TitleOverallEmissions });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Core.Properties.Resources.TitleComponentEmissions });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Core.Properties.Resources.TitleDetailedEmissionsReport });
        }

        private void NavigateToBasicChapter(string selectedOption)
        {
            selectedOption = "ResultsSummaryView";
            base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(ResultsSummaryView));
        }
        private void NavigateToAdvancedTab(string selectedOption)
        {
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

        #endregion

        #region Event Handlers

        private void OnResultModeChanged()
        {
            _lastSelectedBasicItem = _selectedBasicItem;
            _lastSelectedAdvancedItem = _selectedAdvancedItem;
            // Store current advanced tab, navigate to summary when switching to basic
            if (!IsAdvancedMode)
            {
                if (_lastSelectedBasicItem == null)
                {
                    base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(ResultsSummaryView));
                    return;
                }
                SelectedBasicItem = _lastSelectedBasicItem;
            }
            else
            {
                if (_lastSelectedAdvancedItem == null)
                {
                    base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(MultiYearCarbonModellingView));
                    return;
                }

                base.RegionManager.RequestNavigate(UiRegions.ContentRegion, nameof(_lastSelectedAdvancedItem));
            }
        }

        #endregion
    }
}
