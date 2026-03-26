using Avalonia;
using Avalonia.Controls;
using H.Avalonia.Infrastructure;

namespace H.Avalonia.Views.ComponentViews.LandManagement.Field;

public partial class FieldComponentView : UserControl
{
    public static readonly StyledProperty<bool> ShowAdvancedOptionsProperty =
        AvaloniaProperty.Register<FieldComponentView, bool>(nameof(ShowAdvancedOptions), defaultValue: false);

    public bool ShowAdvancedOptions
    {
        get => GetValue(ShowAdvancedOptionsProperty);
        set => SetValue(ShowAdvancedOptionsProperty, value);
    }

    public FieldComponentView()
    {
        InitializeComponent();

        // Set default value for design time
        if (Design.IsDesignMode)
        {
            ShowAdvancedOptions = true;
        }

        // Sync with the shared AppViewSettings singleton
        AppViewSettings.Instance.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AppViewSettings.ShowAdvancedOptions))
                ShowAdvancedOptions = AppViewSettings.Instance.ShowAdvancedOptions;
        };
        ShowAdvancedOptions = AppViewSettings.Instance.ShowAdvancedOptions;
    }
}