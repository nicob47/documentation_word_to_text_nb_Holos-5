using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media;
using H.Core.Enumerations;
using H.Core.Services.CropColorService;

namespace H.Avalonia.DataTemplates;

/// <summary>
/// A data template selector for the crop type ComboBox that renders category headers
/// (string items) differently from selectable crop type items (CropType enum values).
///
/// Design:
///   • Popup has a white background (set on the ComboBox via ItemContainerTheme)
///     so category colours read clearly.
///   • Crop items — full-width coloured left border (4 px) + crop name on white.
///   • Category headers — ALL-CAPS label, light grey background, not interactive.
/// </summary>
public sealed class CropGroupedItemDataTemplate : IDataTemplate
{
    private static readonly ICropColorService _colorService = new CropColorService();

    /// <inheritdoc/>
    public bool Match(object? data) => data is string or CropType;

    /// <inheritdoc/>
    public Control? Build(object? param)
    {
        if (param is string header)
            return BuildHeader(header);

        if (param is CropType cropType)
            return BuildCropItem(cropType);

        return null;
    }

    // ─── Category header ──────────────────────────────────────────────────────
    // Light grey divider row with ALL-CAPS category name.
    // IsHitTestVisible=false keeps it non-selectable.
    private static Control BuildHeader(string categoryName)
    {
        var label = new TextBlock
        {
            Text = categoryName.ToUpperInvariant(),
            FontSize = 10,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#777777"),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(8, 0, 0, 0),
        };

        return new Border
        {
            Background = Brush.Parse("#EEEEEE"),
            Padding = new Thickness(0, 5, 0, 5),
            IsHitTestVisible = false,
            Child = label,
        };
    }

    // ─── Crop item ────────────────────────────────────────────────────────────
    // White background + 4 px coloured left border so colour is always visible
    // regardless of the ComboBox item container's hover/selected highlight.
    private static Control BuildCropItem(CropType cropType)
    {
        var colorHex = _colorService.GetCropColorHex(cropType);
        var displayName = _colorService.GetCropDisplayName(cropType);

        // Saturate the pastel slightly so the accent strip reads clearly
        var accentHex = Saturate(colorHex);

        var accent = new Border
        {
            Width = 4,
            Background = Brush.Parse(accentHex),
            VerticalAlignment = VerticalAlignment.Stretch,
            Margin = new Thickness(0, 0, 8, 0),
        };

        var label = new TextBlock
        {
            Text = displayName,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var row = new DockPanel { LastChildFill = true };
        DockPanel.SetDock(accent, Dock.Left);
        row.Children.Add(accent);
        row.Children.Add(label);

        return row;
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a more saturated/darker version of the pastel hex so the 4 px
    /// accent strip is clearly visible against both white and the hover highlight.
    /// </summary>
    private static string Saturate(string hex) => hex switch
    {
        "#FFF3E0" => "#F59E0B",   // Cereals  pastel orange  → amber
        "#E8F5E9" => "#22C55E",   // Oilseeds pastel green   → green
        "#E3F2FD" => "#3B82F6",   // Pulses   pastel blue    → blue
        "#F3E5F5" => "#A855F7",   // Forages  pastel purple  → purple
        "#FAFAFA" => "#9CA3AF",   // Fallow   near-white     → grey
        "#EFEBE9" => "#92400E",   // Root     pastel brown   → brown
        "#FFFDE7" => "#EAB308",   // Silage   pastel yellow  → yellow
        _         => "#9CA3AF",
    };
}
