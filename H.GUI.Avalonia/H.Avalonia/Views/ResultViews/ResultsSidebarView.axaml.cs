using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using H.Avalonia.ViewModels.Results;

namespace H.Avalonia.Views.ResultViews;

public partial class ResultsSidebarView : UserControl
{
    public ResultsSidebarView()
    {
        InitializeComponent();
    }

    private ResultsSidebarViewModel _viewModel => DataContext as ResultsSidebarViewModel;
}