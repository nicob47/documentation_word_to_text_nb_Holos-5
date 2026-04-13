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
using H.Avalonia.Views.OptionsViews;
using H.Core.Services;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace H.Avalonia.ViewModels.Results
{
    public class ResultsSidebarViewModel : ViewModelBase
    {
        #region Fields

        private ILogger _logger;

        private bool _isAdvancedMode = false;

        private ListBoxItem _selectedBasicItem = null!;
        private ListBoxItem _selectedAdvancedItem = null!;

        private ObservableCollection<ListBoxItem> _basicViewChapters = new();
        private ObservableCollection<ListBoxItem> _advancedViewTabs = new();

        private ResultsSidebarUIState _basicUIState = new();
        private ResultsSidebarUIState _advancedUIState = new();

        #endregion

        #region Constructors

        public ResultsSidebarViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, ILogger logger) : base(regionManager, eventAggregator)
        {
            if (logger != null)
                _logger = logger;
            else
                throw new ArgumentNullException(nameof(logger));

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
                if (value != null)
                {
                    SetProperty(ref _selectedBasicItem, value);
                    _basicUIState.SelectedItem = _selectedBasicItem;
                    _basicUIState.LastAccessed = DateTime.Now;
                }
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
                if (value != null)
                {
                    SetProperty(ref _selectedAdvancedItem, value);
                    _advancedUIState.SelectedItem = _selectedAdvancedItem;
                    _advancedUIState.LastAccessed = DateTime.Now;
                }
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
                    SelectedBasicItem = _basicViewChapters.FirstOrDefault()!;
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
                if (SelectedAdvancedItem == null) // Sets default selected advanced item to the first tab if no tab is selected, occurs when initially navigated to.
                    SelectedAdvancedItem = _advancedViewTabs.FirstOrDefault()!;
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

        /// <summary>
        /// Handles navigation to the view and performs initialization when the view is activated.
        /// </summary>
        /// <param name="navigationContext">The context information associated with the navigation event, including parameters and state relevant to the navigation.</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            // Open to basic mode when navigated to
            IsAdvancedMode = false;
        }

        /// <summary>
        /// When the options button is clicked, this method will navigate to the options view in the sidebar and the select option view in the content region.
        /// </summary>
        public void OnOptionsExecute()
        {
            base.RegionManager?.RequestNavigate(UiRegions.SidebarRegion, nameof(OptionsView));
            base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(SelectOptionView));
            _logger.LogInformation($"Options button selected in {nameof(ResultsSidebarViewModel)}. Sidebar region navigating to {nameof(OptionsViews)}.");
        }

        /// <summary>
        /// When the back button is clicked, this method will clear the content region and navigate back to the components view in the sidebar.
        /// </summary>
        public void OnGoBackExecuted()
        {
            // Clear content region
            var contentView = this.RegionManager?.Regions[UiRegions.ContentRegion].ActiveViews.SingleOrDefault();
            if (contentView != null)
            {
                this.RegionManager?.Regions[UiRegions.ContentRegion].Deactivate(contentView);
                this.RegionManager?.Regions[UiRegions.ContentRegion].Remove(contentView);
            }
            // Navigate back to components view in the sidebar
            base.RegionManager?.RequestNavigate(UiRegions.SidebarRegion, nameof(MyComponentsView));
            _logger.LogInformation($"Back button selected in {nameof(ResultsSidebarViewModel)}. Deactivating {contentView}, sidebar region navigating to {nameof(MyComponentsView)}.");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Populate chapter titles for the basic results view. ListBoxItem content must match chapter names used in <see cref="ResultsSummaryView"/> code-behind ScrollToChapter method for navigation to work correctly
        /// </summary>
        private void PopulateBasicChaptersTitles()
        {
            // Strings not added to resources as these chapter titles will likely be different for release
            _basicViewChapters.Add(new ListBoxItem() { Content = "1. Farm Profile" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "2. Annual Production Summary" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "3. Total GHG Emissions" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "4. Emissions Breakdown By Category" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "5. Carbon Sequestration Summary" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "6. Manure Management Overview" });
            _basicViewChapters.Add(new ListBoxItem() { Content = "7. Key Findings and Recommendations" });
        }

        /// <summary>
        /// Populate tab titles for the advanced results view, these should correspond to the different views available for navigation in the advanced results section.
        /// </summary>
        private void PopulateAdvancedTabs()
        {
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Localization.Resources.Strings.AppStrings.Label_MultiYearCarbonModelling });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Localization.Resources.Strings.AppStrings.Label_EstimatesOfProduction });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Localization.Resources.Strings.AppStrings.Label_FeedEstimateReport });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Localization.Resources.Strings.AppStrings.Label_ManureManagement });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Localization.Resources.Strings.AppStrings.Label_EmissionsPieChart });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Localization.Resources.Strings.AppStrings.Label_OverallEmissions });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Localization.Resources.Strings.AppStrings.Label_ComponentEmissions });
            _advancedViewTabs.Add(new ListBoxItem() { Content = H.Localization.Resources.Strings.AppStrings.Label_DetailedEmissionsReport });
        }

        /// <summary>
        /// Navigates to the corresponding tab in the advanced results view based on the selected option in the sidebar. The mapping between sidebar options and views is determined by the content of the ListBoxItem and can be adjusted as needed. If no matching option is found, no navigation will occur.
        /// </summary>
        /// <param name="selectedOption">The requested tab to be loaded in the content region.</param>
        private void NavigateToAdvancedTab(string selectedOption)
        {
            // Map each advanced tab label to its corresponding view
            var viewMapping = new Dictionary<string, string>
            {
                { H.Localization.Resources.Strings.AppStrings.Label_MultiYearCarbonModelling, nameof(MultiYearCarbonModellingView) },
                { H.Localization.Resources.Strings.AppStrings.Label_EstimatesOfProduction, nameof(EstimatesOfProductionView) },
                { H.Localization.Resources.Strings.AppStrings.Label_FeedEstimateReport, nameof(FeedEstimateReportView) },
                { H.Localization.Resources.Strings.AppStrings.Label_ManureManagement, nameof(ManureManagementResultsView) },
                { H.Localization.Resources.Strings.AppStrings.Label_EmissionsPieChart, nameof(EmissionPieChartView) },
                { H.Localization.Resources.Strings.AppStrings.Label_OverallEmissions, nameof(OverallEmissionsResultsView) },
                { H.Localization.Resources.Strings.AppStrings.Label_ComponentEmissions, nameof(ComponentEmissionsResultsView) },
                { H.Localization.Resources.Strings.AppStrings.Label_DetailedEmissionsReport, nameof(DetailedEmissionsReportResultsView) },
            };
            // Attempt to navigate to selected view, log error selection does not exist in mapping
            if (viewMapping.TryGetValue(selectedOption, out var viewName))
            {
                base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, viewName);
            }
            else
            {
                _logger.LogError($"Attempted to navigate to advanced tab {selectedOption} that does not exist in {nameof(ResultsSidebarView)}.");
            }
        }

        /// <summary>
        /// When the results mode is toggled to basic or advanced, this method will navigate to the last accessed chapter or tab in the corresponding view. If no chapter or tab has been accessed yet, it will navigate to the first item in the list by default.
        /// </summary>
        private void OnResultModeChanged()
        {
            if (!IsAdvancedMode)
            {
                _logger.LogInformation($"Basic results view selected in {nameof(ResultsSidebarViewModel)}. Content region navigating to {nameof(ResultsSummaryView)}.");
                base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(ResultsSummaryView));
            }
            else
            {
                var selectedItem = _advancedUIState.SelectedItem ?? _advancedViewTabs.FirstOrDefault();
                _logger.LogInformation($"Advanced results view selected in {nameof(ResultsSidebarViewModel)}. Content region navigating to {selectedItem?.Content?.ToString()}.");
                if (selectedItem != null)
                {
                    NavigateToAdvancedTab(selectedItem.Content?.ToString() ?? string.Empty);
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
        private void OnSelectedOptionChanged(object? sender, PropertyChangedEventArgs e)
        {
            // If basic chapter selected, publish event to update chapter in the results summary view
            if (e.PropertyName == nameof(SelectedBasicItem) && SelectedBasicItem != null)
            {
                EventAggregator?.GetEvent<BasicChapterSelectedEvent>().Publish(SelectedBasicItem.Content?.ToString() ?? string.Empty);
                SelectedBasicItem = null!;
            }
            // If advanced tab selected, navigate to that view using the region manager
            if (e.PropertyName == nameof(SelectedAdvancedItem) && SelectedAdvancedItem != null)
            {
                NavigateToAdvancedTab(SelectedAdvancedItem.Content?.ToString() ?? string.Empty);
            }
        }

        #endregion
    }
}
