using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Prism.Regions;

namespace H.Avalonia.Views.ResultViews;

public partial class ResultsSummaryView : UserControl, INavigationAware
{
    public ResultsSummaryView()
    {
        InitializeComponent();
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
            case "FarmProfileSection":
                FarmProfileSection?.BringIntoView();
                break;
            case "AnnualProductionSection":
                AnnualProductionSection?.BringIntoView();
                break;
            case "TotalGHGEmissionsSection":
                TotalGHGEmissionsSection?.BringIntoView();
                break;
            case "EmissionsBreakdownSection":
                EmissionsBreakdownSection?.BringIntoView();
                break;
            case "CarbonSequestrationSection":
                CarbonSequestrationSection?.BringIntoView();
                break;
            case "ManureManagementSection":
                ManureManagementSection?.BringIntoView();
                break;
            case "KeyFindingsSection":
                KeyFindingsSection?.BringIntoView();
                break;
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}