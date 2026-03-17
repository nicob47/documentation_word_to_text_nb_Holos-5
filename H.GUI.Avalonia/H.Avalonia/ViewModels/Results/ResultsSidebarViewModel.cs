using Avalonia.Controls;
using H.Avalonia.Events;
using H.Avalonia.Views;
using H.Avalonia.Views.ComponentViews;
using H.Avalonia.Views.ResultViews;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using H.Core.Services;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace H.Avalonia.ViewModels.Results
{
    public class ResultsSidebarViewModel : ViewModelBase
    {
        #region Fields

        private ILogger _logger;
        private IEventAggregator _eventAggregator;

        private bool _isAdvancedMode = false;

        private ListBoxItem _selectedBasicItem;
        private ListBoxItem _selectedAdvancedItem;

        private ObservableCollection<ListBoxItem> _basicViewChapters = new();
        private ObservableCollection<ListBoxItem> _advancedViewTabs = new();

        private ResultsSidebarUIState _basicUIState = new();
        private ResultsSidebarUIState _advancedUIState = new();

        #endregion

        #region Constructors

        public ResultsSidebarViewModel(IRegionManager regionManager, ILogger logger, IEventAggregator eventAggregator) : base(regionManager)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
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
                _basicUIState.SelectedItem = _selectedBasicItem;
                _basicUIState.LastAccessed = DateTime.Now;
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
                _advancedUIState.SelectedItem = _selectedAdvancedItem;
                _advancedUIState.LastAccessed = DateTime.Now;
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


        /// <summary>
        /// Populate chapter titles for the basic results view, these should correspond to the sections in the ResultsSummaryView.axaml
        /// </summary>
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

        /// <summary>
        /// Populate tab titles for the advanced results view, these should correspond to the different views available for navigation in the advanced results section.
        /// </summary>
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

        /// <summary>
        /// This will load the requested chapter in the basic results view in to the content region.
        /// </summary>
        /// <param name="selectedOption">The requested chapter to be loaded in the content region.</param>
        private void NavigateToBasicChapter(string selectedOption)
        {
            selectedOption = "ResultsSummaryView";
            base.RegionManager.RequestNavigate(UiRegions.ContentRegion, selectedOption);
        }

        /// <summary>
        /// Navigates to the corresponding tab in the advanced results view based on the selected option in the sidebar. The mapping between sidebar options and views is determined by the content of the ListBoxItem and can be adjusted as needed. If no matching option is found, no navigation will occur.
        /// </summary>
        /// <param name="selectedOption">The requested tab to be loaded in the content region.</param>
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
                default:
                    _logger.LogError($"Attempted to navigate to advanced tab {selectedOption} that does not exist in {nameof(ResultsSidebarView)}.");
                    return;
            }
        }

        /// <summary>
        /// When the results mode is toggled to basic or advanced, this method will navigate to the last accessed chapter or tab in the corresponding view. If no chapter or tab has been accessed yet, it will navigate to the first item in the list by default.
        /// </summary>
        private void OnResultModeChanged()
        {
            if (!IsAdvancedMode)
            {
                var selectedItem = _basicUIState.SelectedItem ?? _basicViewChapters.FirstOrDefault();
                if (selectedItem != null)
                {
                    NavigateToBasicChapter(selectedItem.Content?.ToString());
                }
            }
            else
            {
                var selectedItem = _advancedUIState.SelectedItem ?? _advancedViewTabs.FirstOrDefault();
                if (selectedItem != null)
                {
                    NavigateToAdvancedTab(selectedItem.Content?.ToString());
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Requests navigation to the appropriate chapter or tab in the content region based off of the basic/advanced results state.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedOptionChanged(object sender, PropertyChangedEventArgs e)
        {
            // If basic chapter selected, publish event to update chapter in the results summary view
            if (e.PropertyName == nameof(SelectedBasicItem) && SelectedBasicItem != null)
            {
                _eventAggregator?.GetEvent<BasicChapterSelectedEvent>().Publish(SelectedBasicItem.Content?.ToString());
            }

            // If advanced tab selected, navigate to that view using the region manager
            if (e.PropertyName == nameof(SelectedAdvancedItem) && SelectedAdvancedItem != null)
            {
                NavigateToAdvancedTab(SelectedAdvancedItem.Content?.ToString());
            }
        }

        #endregion
    }
}
