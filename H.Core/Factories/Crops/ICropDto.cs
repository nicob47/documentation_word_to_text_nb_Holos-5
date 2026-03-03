using System.Collections.Generic;
using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Factories.Crops;

/// <summary>
/// A data transfer object used to validate and collection information about a <see cref="CropViewItem"/>
/// </summary>
public interface ICropDto : IDto
{
    /// <summary>
    /// The crop type being grown
    /// </summary>
    CropType CropType { get; set; }

    /// <summary>
    /// The list of valid crop types available for selection
    /// </summary>
    ObservableCollection<CropType> ValidCropTypes { get; set; }

    /// <summary>
    /// A flat, grouped list of items for the crop selection ComboBox.
    /// Alternates between plain-string category headers (non-selectable) and
    /// <see cref="CropType"/> values (selectable).  Used by the UI layer together
    /// with <see cref="SelectedCropTypeItem"/> to produce a categorised dropdown.
    /// </summary>
    IReadOnlyList<object> GroupedCropItems { get; }

    /// <summary>
    /// The currently selected item in the grouped ComboBox.
    /// Wraps <see cref="CropType"/> as an <c>object?</c> so the ComboBox
    /// <c>SelectedItem</c> binding can coexist with the mixed-type
    /// <see cref="GroupedCropItems"/> list.  Setting this to a string (category
    /// header) is silently ignored so that clicking a header does not change the
    /// selected crop.
    /// </summary>
    object? SelectedCropTypeItem { get; set; }

    /// <summary>
    /// The year in which the crop was grown
    /// </summary>
    int Year { get; set; }

    /// <summary>
    /// The total amount of annual irrigation
    ///
    /// (mm)
    /// </summary>
    double AmountOfIrrigation { get; set; }

    /// <summary>
    /// Total wet weight yield of the crop
    ///
    /// (kg ha^-1)
    /// </summary>
    double WetYield { get; set; }

    /// <summary>
    /// Indicates whether this crop is currently selected in the UI
    /// </summary>
    bool IsSelected { get; set; }

    /// <summary>
    /// Indicates whether herbicide was used for this crop
    /// </summary>
    bool HerbicideUsed { get; set; }

    /// <summary>
    /// Indicates whether changes to this crop should be copied to other crops
    /// of the same type in the same field (row)
    /// </summary>
    bool CopyToSimilarCrops { get; set; }
}
