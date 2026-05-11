namespace H.Core.Models.LandManagement.Fields;

public static class CropViewItemExtensions
{
    public static List<CropViewItem> GetByYear(this IEnumerable<CropViewItem> viewItems, int year)
    {
        return viewItems.Where(x => x.Year == year).ToList();
    }

    /// <summary>
    /// All unique years across the given items, ascending.
    /// </summary>
    public static List<int> GetDistinctYears(this IEnumerable<CropViewItem> viewItems)
    {
        return viewItems.Select(x => x.Year).Distinct().OrderBy(x => x).ToList();
    }

    /// <summary>
    /// All items grown in the given year.
    /// </summary>
    public static List<CropViewItem> GetItemsByYear(this IEnumerable<CropViewItem> viewItems, int year)
    {
        return viewItems.Where(x => x.Year == year).ToList();
    }

    public static List<CropViewItem> GetSecondaryCrops(this IEnumerable<CropViewItem> viewItems, CropViewItem mainCropViewItem)
    {
        return viewItems.GetSecondaryCrops().Except(new[] { mainCropViewItem }).ToList();
    }

    public static List<CropViewItem> GetSecondaryCrops(this IEnumerable<CropViewItem> viewItems)
    {
        return viewItems.OrderBy(x => x.Year).Where(x => x.IsSecondaryCrop).ToList();
    }

    public static List<CropViewItem> GetMainCrops(this IEnumerable<CropViewItem> viewItems)
    {
        return viewItems.OrderBy(x => x.Year).Where(x => !x.IsSecondaryCrop).ToList();
    }
}
