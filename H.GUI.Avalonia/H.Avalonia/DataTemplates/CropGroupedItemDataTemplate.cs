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
/// Headers appear as bold, full-width, non-interactive labels with a category colour strip.
/// Crop items appear as coloured pill badges matching the Step 3 preview grid colours.
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
        {
            return BuildHeader(header);
        }

        if (param is CropType cropType)
        {
            return BuildCropItem(cropType);
        }

        return null;
    }

    // -------------------------------------------------------------------------
    // Header: bold category label with a left colour accent strip
    // -------------------------------------------------------------------------
    private static Control BuildHeader(string categoryName)
    {
        // Determine colour from category name
        var colorHex = GetCategoryColorHex(categoryName);

        // Left accent strip
        var accent = new Border
        {
            Width = 4,
            CornerRadius = new CornerRadius(2),
            Background = Brush.Parse(colorHex),
            Margin = new Thickness(0, 2, 8, 2),
            VerticalAlignment = VerticalAlignment.Stretch,
        };

        var label = new TextBlock
        {
            Text = categoryName,
            FontWeight = FontWeight.Bold,
            FontSize = 11,
            Foreground = Brush.Parse("#555555"),
            VerticalAlignment = VerticalAlignment.Center,
        };

        var row = new DockPanel { Margin = new Thickness(4, 6, 4, 2) };
        DockPanel.SetDock(accent, Dock.Left);
        row.Children.Add(accent);
        row.Children.Add(label);

        // Wrap in a non-interactive container (IsHitTestVisible=false makes it non-selectable)
        return new Border
        {
            Background = Brush.Parse("#F8F8F8"),
            IsHitTestVisible = false,
            Child = row,
        };
    }

    // -------------------------------------------------------------------------
    // Crop item: coloured pill matching Step 3 preview grid colours
    // -------------------------------------------------------------------------
    private static Control BuildCropItem(CropType cropType)
    {
        var colorHex = _colorService.GetCropColorHex(cropType);
        var displayName = _colorService.GetCropDisplayName(cropType);

        var label = new TextBlock
        {
            Text = displayName,
            FontSize = 13,
            VerticalAlignment = VerticalAlignment.Center,
        };

        return new Border
        {
            Background = Brush.Parse(colorHex),
            Padding = new Thickness(6, 3),
            CornerRadius = new CornerRadius(3),
            Margin = new Thickness(-4, 1, -4, 1),
            Child = label,
        };
    }

    // -------------------------------------------------------------------------
    // Map category name → hex (mirrors CropColorService logic)
    // -------------------------------------------------------------------------
    private static string GetCategoryColorHex(string categoryName) => categoryName switch
    {
        "Cereals"  => "#FFF3E0",
        "Oilseeds" => "#E8F5E9",
        "Pulses"   => "#E3F2FD",
        "Forages"  => "#F3E5F5",
        "Fallow"   => "#FAFAFA",
        "Root Crops" => "#EFEBE9",
        "Silage"   => "#FFFDE7",
        _          => "#F5F5F5",
    };
}
