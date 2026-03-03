namespace H.Avalonia.Models;

/// <summary>
/// Sentinel object used as a non-selectable category header in the crop type ComboBox.
/// Mixed into a flat list alongside CropType values to produce grouped dropdowns.
/// </summary>
public sealed record CropCategoryHeader(string Name, string ColorHex);
