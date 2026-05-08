using H.Core.Models.LandManagement.Fields;

namespace H.Core.Models;

/// <summary>
/// Carries the previous, current, and next year's <see cref="CropViewItem"/> for a single field.
/// Carbon input calculations frequently need adjoining years (e.g., to handle perennial yield
/// imputation in the final stand year). Grouping them in one object keeps method signatures tidy.
/// </summary>
public class AdjoiningYears
{
    public CropViewItem? PreviousYearViewItem { get; set; }
    public CropViewItem? CurrentYearViewItem { get; set; }
    public CropViewItem? NextYearViewItem { get; set; }
}
