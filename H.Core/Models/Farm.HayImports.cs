using H.Core.Models.LandManagement.Fields;

namespace H.Core.Models;

public partial class Farm
{
    #region Public Methods

    /// <summary>
    /// Returns all <see cref="HayImportViewItem"/> across the farm that draw hay from the given source field.
    /// </summary>
    public List<HayImportViewItem> GetHayImportsUsingImportedHayFromSourceField(FieldSystemComponent fieldUsedAsSource)
    {
        var result = new List<HayImportViewItem>();
        foreach (var fieldSystemComponent in this.FieldSystemComponents)
        {
            foreach (var cropViewItem in fieldSystemComponent.CropViewItems)
            {
                result.AddRange(cropViewItem.HayImportViewItems.Where(x => x.FieldSourceGuid.Equals(fieldUsedAsSource.Guid)));
            }
        }

        return result;
    }

    public List<HayImportViewItem> GetHayImportsUsingImportedHayFromSourceField(Guid fieldSystemGuid)
    {
        var field = this.GetFieldSystemComponent(fieldSystemGuid);
        return field != null
            ? this.GetHayImportsUsingImportedHayFromSourceField(field)
            : new List<HayImportViewItem>();
    }

    /// <summary>
    /// Returns all <see cref="HayImportViewItem"/>, in a given year, that draw hay from the field identified by <paramref name="fieldSystemGuid"/>.
    /// </summary>
    public List<HayImportViewItem> GetHayImportsUsingImportedHayFromSourceFieldByYear(Guid fieldSystemGuid, int year)
    {
        var field = this.GetFieldSystemComponent(fieldSystemGuid);
        if (field == null)
        {
            return new List<HayImportViewItem>();
        }

        return this.GetHayImportsUsingImportedHayFromSourceField(field)
            .Where(x => x.Start.Year.Equals(year))
            .ToList();
    }

    #endregion
}
