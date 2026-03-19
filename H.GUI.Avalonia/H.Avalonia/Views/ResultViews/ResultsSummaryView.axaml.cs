using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using H.Avalonia.Events;
using Prism.Events;
using Prism.Regions;

namespace H.Avalonia.Views.ResultViews;

public partial class ResultsSummaryView : UserControl
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

    /// <summary>
    /// Scrolls to the selected chapter in the results summary when a chapter is selected from the sidebar. The chapter names must match the named borders in the XAML for this to work.
    /// </summary>
    /// <param name="chapter"></param>
    private void ScrollToChapter(string chapter)
    {
        // Named Borders in the XAML for navigation
        switch (chapter)
        {
            case "1. Farm Profile":
                FarmProfileSection?.BringIntoView();
                break;
            case "2. Annual Production Summary":
                AnnualProductionSection?.BringIntoView();
                break;
            case "3. Total GHG Emissions":
                TotalGHGEmissionsSection?.BringIntoView();
                break;
            case "4. Emissions Breakdown By Category":
                EmissionsBreakdownSection?.BringIntoView();
                break;
            case "5. Carbon Sequestration Summary":
                CarbonSequestrationSection?.BringIntoView();
                break;
            case "6. Manure Management Overview":
                ManureManagementSection?.BringIntoView();
                break;
            case "7. Key Findings and Recommendations":
                KeyFindingsSection?.BringIntoView();
                break;
        }
    }
}