using System;
using Avalonia.Controls;
using H.Avalonia.ViewModels.SupportingViews.DietFormulator;

namespace H.Avalonia.Views.SupportingViews.DietFormulator;

public partial class DietFormulatorWindow : Window
{
    // Preferred size on a large monitor; the actual size is clamped to the screen's
    // working area at open time so the window always fits (laptops, small desktops, etc.).
    private const double PreferredWidth = 1400;
    private const double PreferredHeight = 820;

    // Floor so we never shrink below something readable. Matches Window.MinWidth/MinHeight.
    private const double MinClampedWidth = 950;
    private const double MinClampedHeight = 550;

    // Fraction of the screen's working area we'll use when clamping (leaves headroom
    // for the taskbar, window chrome, and a small visible-strip-of-parent feel).
    private const double ScreenFraction = 0.9;

    public DietFormulatorWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Opened += OnOpened;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is DietFormulatorWindowViewModel vm)
        {
            vm.CloseRequested -= OnCloseRequested;
            vm.CloseRequested += OnCloseRequested;
        }
    }

    private void OnCloseRequested(object? sender, EventArgs e) => this.Close();

    /// <summary>
    /// Clamp the modal to the available screen area so it doesn't render off-screen
    /// on smaller displays (1366x768 laptops, etc.). On larger monitors the preferred
    /// size is used directly. Users can still resize manually within the configured
    /// MinWidth/MinHeight bounds.
    /// </summary>
    private void OnOpened(object? sender, EventArgs e)
    {
        var screen = Screens.ScreenFromVisual(this) ?? Screens.Primary;
        if (screen == null) return;

        var area = screen.WorkingArea;
        // Avalonia Screen.WorkingArea is in physical pixels; convert to DIPs using DesktopScaling.
        var scale = screen.Scaling > 0 ? screen.Scaling : 1.0;
        var areaWidthDip = area.Width / scale;
        var areaHeightDip = area.Height / scale;

        var targetWidth = Math.Max(MinClampedWidth, Math.Min(PreferredWidth, areaWidthDip * ScreenFraction));
        var targetHeight = Math.Max(MinClampedHeight, Math.Min(PreferredHeight, areaHeightDip * ScreenFraction));

        Width = targetWidth;
        Height = targetHeight;
    }
}
