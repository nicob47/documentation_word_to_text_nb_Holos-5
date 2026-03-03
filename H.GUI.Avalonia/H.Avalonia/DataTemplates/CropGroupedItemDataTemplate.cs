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
/// Design goal: keep the list flat and easy to scan.
///   • Headers  — small ALL-CAPS label with a coloured left border strip; not interactive.
///   • Crop items — plain text with a coloured 10×10 square dot on the left; no nested pill border.
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
    // Renders as:  ▌ CEREALS
    // IsHitTestVisible=false prevents it from being selected.
    private static Control BuildHeader(string categoryName)
    {
        var colorHex = GetCategoryColorHex(categoryName);

        // 3px solid left border strip
        var strip = new Border
        {
            Width = 3,
            Background = Brush.Parse(colorHex == "#FAFAFA" ? "#BBBBBB" : colorHex),
            Margin = new Thickness(0, 0, 8, 0),
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        var label = new TextBlock
        {
            Text = categoryName.ToUpperInvariant(),
            FontSize = 10,
            FontWeight = FontWeight.Bold,
            Foreground = Brush.Parse("#888888"),
            VerticalAlignment = VerticalAlignment.Center,
            LetterSpacing = 0.5,
        };

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(2, 8, 0, 2),
        };
        row.Children.Add(strip);
        row.Children.Add(label);

        return new Border
        {
            IsHitTestVisible = false,
            Child = row,
        };
    }

    // ─── Crop item ────────────────────────────────────────────────────────────
    // Renders as:  ■ Canola       (coloured 10×10 square + plain text, no pill border)
    private static Control BuildCropItem(CropType cropType)
    {
        var colorHex = _colorService.GetCropColorHex(cropType);
        var displayName = _colorService.GetCropDisplayName(cropType);

        // Small colour square — same palette as Step 3 grid
        var dot = new Border
        {
            Width = 10,
            Height = 10,
            CornerRadius = new CornerRadius(2),
            Background = Brush.Parse(colorHex),
            BorderBrush = Brush.Parse("#CCCCCC"),
            BorderThickness = new Thickness(0.5),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 8, 0),
        };

        var label = new TextBlock
        {
            Text = displayName,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
        };

        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
        };
        row.Children.Add(dot);
        row.Children.Add(label);

        return row;
    }

    // ─── Category → hex colour ────────────────────────────────────────────────
    private static string GetCategoryColorHex(string categoryName) => categoryName switch
    {
        "Cereals"    => "#FFF3E0",
        "Oilseeds"   => "#E8F5E9",
        "Pulses"     => "#E3F2FD",
        "Forages"    => "#F3E5F5",
        "Fallow"     => "#FAFAFA",
        "Root Crops" => "#EFEBE9",
        "Silage"     => "#FFFDE7",
        _            => "#F5F5F5",
    };
}
