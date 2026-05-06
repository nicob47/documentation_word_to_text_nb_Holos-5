using Avalonia.Controls;
using H.Avalonia.ViewModels.SupportingViews.DietFormulator;

namespace H.Avalonia.Views.SupportingViews.DietFormulator;

public partial class DietFormulatorWindow : Window
{
    public DietFormulatorWindow()
    {
        InitializeComponent();
        // Subscribe to the VM's CloseRequested event so the VM stays UI-agnostic
        // and the window closes itself when CloseCommand is invoked.
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, System.EventArgs e)
    {
        if (DataContext is DietFormulatorWindowViewModel vm)
        {
            vm.CloseRequested -= OnCloseRequested;
            vm.CloseRequested += OnCloseRequested;
        }
    }

    private void OnCloseRequested(object? sender, System.EventArgs e)
    {
        this.Close();
    }
}
