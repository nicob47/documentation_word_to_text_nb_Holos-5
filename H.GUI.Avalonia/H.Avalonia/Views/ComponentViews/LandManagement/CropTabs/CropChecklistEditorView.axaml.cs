using Avalonia;
using Avalonia.Controls;

namespace H.Avalonia.Views.ComponentViews.LandManagement.CropTabs;

public partial class CropChecklistEditorView : UserControl
{
    public CropChecklistEditorView()
    {
        InitializeComponent();
    }

    // ── Styled properties so the parent view can bind VM toggle states ──

    public static readonly StyledProperty<bool> IsFertilizerActiveProperty =
        AvaloniaProperty.Register<CropChecklistEditorView, bool>(nameof(IsFertilizerActive), defaultValue: true);

    public bool IsFertilizerActive
    {
        get => GetValue(IsFertilizerActiveProperty);
        set => SetValue(IsFertilizerActiveProperty, value);
    }

    public static readonly StyledProperty<bool> IsManureActiveProperty =
        AvaloniaProperty.Register<CropChecklistEditorView, bool>(nameof(IsManureActive));

    public bool IsManureActive
    {
        get => GetValue(IsManureActiveProperty);
        set => SetValue(IsManureActiveProperty, value);
    }

    public static readonly StyledProperty<bool> IsGrazingActiveProperty =
        AvaloniaProperty.Register<CropChecklistEditorView, bool>(nameof(IsGrazingActive));

    public bool IsGrazingActive
    {
        get => GetValue(IsGrazingActiveProperty);
        set => SetValue(IsGrazingActiveProperty, value);
    }

    public static readonly StyledProperty<bool> IsSoilActiveProperty =
        AvaloniaProperty.Register<CropChecklistEditorView, bool>(nameof(IsSoilActive));

    public bool IsSoilActive
    {
        get => GetValue(IsSoilActiveProperty);
        set => SetValue(IsSoilActiveProperty, value);
    }

    public static readonly StyledProperty<bool> IsResidueActiveProperty =
        AvaloniaProperty.Register<CropChecklistEditorView, bool>(nameof(IsResidueActive));

    public bool IsResidueActive
    {
        get => GetValue(IsResidueActiveProperty);
        set => SetValue(IsResidueActiveProperty, value);
    }

    public static readonly StyledProperty<bool> IsEconomicsActiveProperty =
        AvaloniaProperty.Register<CropChecklistEditorView, bool>(nameof(IsEconomicsActive));

    public bool IsEconomicsActive
    {
        get => GetValue(IsEconomicsActiveProperty);
        set => SetValue(IsEconomicsActiveProperty, value);
    }
}
