using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using H.Avalonia.Events;
using Prism.Events;
using Prism.Regions;

namespace H.Avalonia.Views.ResultViews;

public partial class ResultsSummaryView : UserControl, INavigationAware
{
    private IEventAggregator _eventAggregator;
    private ScrollViewer _scrollViewer;

    public ResultsSummaryView(IEventAggregator eventAggregator)
    {
        InitializeComponent();
        _eventAggregator = eventAggregator;
        _scrollViewer = this.FindControl<ScrollViewer>("ContentScrollViewer");

        _eventAggregator?.GetEvent<BasicChapterSelectedEvent>().Subscribe(ScrollToChapter);
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext?.Parameters != null && navigationContext.Parameters.ContainsKey("chapter"))
        {
            var chapter = navigationContext.Parameters.GetValue<string>("chapter");
            if (!string.IsNullOrEmpty(chapter))
            {
                // Post to UI thread so layout is complete before BringIntoView
                Dispatcher.UIThread.Post(() => ScrollToChapter(chapter), DispatcherPriority.Background);
            }
        }
    }

    private void ScrollToChapter(string chapter)
    {
        // Named Borders in the XAML become fields: FarmProfileSection, AnnualProductionSection, etc.
        switch (chapter)
        {
            case "Farm Profile":
                FarmProfileSection?.BringIntoView();
                break;
            case "Annual Production Summary":
                AnnualProductionSection?.BringIntoView();
                break;
            case "Total GHG Emissions":
                TotalGHGEmissionsSection?.BringIntoView();
                break;
            case "Emissions Breakdown By Category":
                EmissionsBreakdownSection?.BringIntoView();
                break;
            case "Carbon Sequestration Summary":
                CarbonSequestrationSection?.BringIntoView();
                break;
            case "Manure Management Overview":
                ManureManagementSection?.BringIntoView();
                break;
            case "Key Findings and Recommendations":
                KeyFindingsSection?.BringIntoView();
                break;
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}