using H.Core.Models;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Services
{
    public interface IFieldComponentHelper
    {
        void InitializeComponent(FieldSystemComponent component, Farm farm);
        void Replicate(ComponentBase copyFrom, ComponentBase copyTo);
        FieldSystemComponent Replicate(FieldSystemComponent component);
        string GetUniqueFieldName(IEnumerable<FieldSystemComponent> components);
        void InitializeSoilAvailableSoilTypes(Farm farm, FieldSystemComponent component);

        /// <summary>
        /// Returns the main crop for the given year (the non-secondary view item, falling back
        /// to the first item for legacy farms where IsSecondaryCrop is not set).
        /// </summary>
        CropViewItem? GetMainCropForYear(IEnumerable<CropViewItem> viewItems, int year);

        /// <summary>
        /// Returns the previous-year, current-year, and next-year view items for a single stand,
        /// used by perennial carbon-input calculations that need adjoining years for yield imputation.
        /// </summary>
        AdjoiningYears GetAdjoiningYears(IEnumerable<CropViewItem> viewItems, int year);
    }
}