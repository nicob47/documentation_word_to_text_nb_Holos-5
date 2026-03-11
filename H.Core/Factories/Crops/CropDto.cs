using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using H.Core.CustomAttributes;
using H.Core.Enumerations;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Factories.Crops;

/// <summary>
/// A class used to validate input as it relates to a <see cref="CropViewItem"/>. This class is used to valid input before any input
/// is transferred to the <see cref="CropViewItem"/>
/// </summary>
public partial class CropDto : DtoBase, ICropDto
{
    #region Fields

    private double _amountOfIrrigation;
    private int _year;
    private CropType _cropType;
    private ObservableCollection<CropType> _cropTypes = new();
    private double _wetYield;
    private bool _isSelected;
    private bool _herbicideUsed;
    private bool _copyToSimilarCrops;

    // Lazily-built grouped list (string headers + CropType values)
    private IReadOnlyList<object>? _groupedCropItems;

    #endregion

    #region Constructors

    public CropDto()
    {
        // Initialize with diverse crop types representing all color categories
        this.ValidCropTypes = new ObservableCollection<CropType>()
        {
            CropType.NotSelected,

            // Cereals - Orange (IsSmallGrains + explicit fallbacks for Rye/Corn/Durum)
            CropType.Wheat,
            CropType.Barley,
            CropType.Oats,
            CropType.Rye,
            CropType.Corn,
            CropType.GrainCorn,
            CropType.Triticale,
            CropType.Durum,

            // Silage - Yellow (IsSilageCrop)
            CropType.SilageCorn,

            // Oilseeds - Green
            CropType.Canola,
            CropType.Flax,
            CropType.FlaxSeed,
            CropType.Sunflower,
            CropType.SunflowerSeed,
            CropType.Soybeans,
            CropType.Mustard,
            CropType.MustardSeed,

            // Pulses - Blue
            CropType.Peas,
            CropType.DryPeas,
            CropType.FieldPeas,
            CropType.Lentils,
            CropType.Chickpeas,
            CropType.FabaBeans,
            CropType.Beans,
            CropType.DryBean,

            // Forages - Purple
            CropType.AlfalfaMedicagoSativaL,
            CropType.AlfalfaHay,
            CropType.TameGrass,
            CropType.TameLegume,
            CropType.TameMixed,
            CropType.GrassHay,
            CropType.PerennialForages,
            CropType.Forage,

            // Fallow - Gray
            CropType.Fallow,
            CropType.SummerFallow
        };

        this.CropType = this.ValidCropTypes.ElementAt(0);
        this.Year = DateTime.Now.Year;
        this.AmountOfIrrigation = 0;
        this.WetYield = 0;
        this.HerbicideUsed = false;
        this.CopyToSimilarCrops = false;

        // General defaults
        this.TillageType = TillageType.Reduced;
        this.HarvestMethod = HarvestMethods.CashCrop;
        this.MoistureContentOfCropPercentage = 12;

        // Fertilizer defaults
        this.CustomReductionFactor = 1.0;

        // Residue defaults
        this.CarbonConcentration = 0.45;

        this.PropertyChanged += OnPropertyChanged;
    }

    #endregion

    #region Properties

    public CropType CropType
    {
        get => _cropType;
        set => SetProperty(ref _cropType, value);
    }

    public ObservableCollection<CropType> ValidCropTypes
    {
        get => _cropTypes;
        set
        {
            if (SetProperty(ref _cropTypes, value))
            {
                // Invalidate grouped list whenever the source list changes
                _groupedCropItems = null;
                RaisePropertyChanged(nameof(GroupedCropItems));
            }
        }
    }

    /// <inheritdoc/>
    public IReadOnlyList<object> GroupedCropItems => _groupedCropItems ??= BuildGroupedCropItems();

    /// <inheritdoc/>
    public object? SelectedCropTypeItem
    {
        get => this.CropType;   // expose current crop type as the selected item
        set
        {
            // Ignore header strings — clicking a category header must not change the crop
            if (value is CropType cropType)
            {
                this.CropType = cropType;
                RaisePropertyChanged(nameof(SelectedCropTypeItem));
            }
        }
    }

    public int Year
    {
        get => _year;
        set => SetProperty(ref _year, value);
    }

    [Units(MetricUnitsOfMeasurement.Millimeters)]
    public double AmountOfIrrigation
    {
        get => _amountOfIrrigation;
        set => SetProperty(ref _amountOfIrrigation, value);
    }

    [Units(MetricUnitsOfMeasurement.KilogramsPerHectare)]
    public double WetYield
    {
        get => _wetYield;
        set => SetProperty(ref _wetYield, value);
    }

    /// <summary>
    /// Indicates whether this crop is currently selected in the UI
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Indicates whether herbicide was used for this crop
    /// </summary>
    public bool HerbicideUsed
    {
        get => _herbicideUsed;
        set => SetProperty(ref _herbicideUsed, value);
    }

    /// <summary>
    /// Indicates whether changes to this crop should be copied to other crops
    /// of the same type in the same field (row)
    /// </summary>
    public bool CopyToSimilarCrops
    {
        get => _copyToSimilarCrops;
        set => SetProperty(ref _copyToSimilarCrops, value);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Builds a flat list that interleaves category-header strings with
    /// <see cref="CropType"/> values from <see cref="ValidCropTypes"/>.
    /// The resulting list is used as the <c>ItemsSource</c> for the crop
    /// ComboBox so that headers appear as non-selectable section dividers.
    /// </summary>
    private IReadOnlyList<object> BuildGroupedCropItems()
    {
        var result = new List<object>();
        string? currentCategory = null;

        foreach (var cropType in ValidCropTypes)
        {
            var category = GetCategoryName(cropType);

            if (category != currentCategory)
            {
                currentCategory = category;
                // Insert a plain-string header; the UI layer detects strings and
                // renders them as non-selectable labels
                if (category is not null)
                {
                    result.Add(category);
                }
            }

            result.Add(cropType);
        }

        return result.AsReadOnly();
    }

    /// <summary>
    /// Returns the display name of the category a crop belongs to,
    /// or <c>null</c> for <see cref="CropType.NotSelected"/>.
    /// Mirrors the colour buckets defined in <c>CropColorService</c>.
    /// </summary>
    private static string? GetCategoryName(CropType cropType)
    {
        if (cropType == CropType.NotSelected)
            return null;

        // Check explicit extension methods first
        if (cropType.IsFallow())
            return "Fallow";

        if (cropType.IsSmallGrains())
            return "Cereals";

        if (cropType.IsOilSeed())
            return "Oilseeds";

        if (cropType.IsPulseCrop())
            return "Pulses";

        if (cropType.IsPerennial())
            return "Forages";

        if (cropType.IsRootCrop())
            return "Root Crops";

        if (cropType.IsSilageCrop())
            return "Silage";

        // Explicit fallbacks for crop types that don't match the extension methods
        // but are present in ValidCropTypes — keeps them out of the "Other" bucket.
        return cropType switch
        {
            // Cereals not covered by IsSmallGrains()
            CropType.Rye or CropType.Corn or CropType.Durum
                => "Cereals",

            // Oilseeds not covered by IsOilSeed()
            CropType.FlaxSeed or CropType.Sunflower or CropType.SunflowerSeed
            or CropType.MustardSeed
                => "Oilseeds",

            // Pulses not covered by IsPulseCrop()
            CropType.Peas or CropType.Beans or CropType.DryBean or CropType.FabaBeans
                => "Pulses",

            // Forages / perennials not covered by IsPerennial()
            CropType.AlfalfaMedicagoSativaL or CropType.AlfalfaHay or CropType.GrassHay
                => "Forages",

            _ => "Other",
        };
    }

    #endregion

    #region Event Handlers

    private void ValidateWetYield()
    {
        var key = nameof(WetYield);
        if (this.WetYield < 0)
        {
            AddError(key, "Wet yield cannot be negative");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// The user must specify a crop type before proceeding
    /// </summary>
    private void ValidateCropType()
    {
        var key = nameof(CropType);
        if (this.CropType == CropType.NotSelected)
        {
            AddError(key, "A crop type must be selected");
        }
        else
        {
            RemoveError(key);
        }
    }

    private void ValidateAmountOfIrrigation()
    {
        var key = nameof(AmountOfIrrigation);
        if (this.AmountOfIrrigation < 0)
        {
            AddError(key, "Amount of irrigation cannot be negative");
        }
        else
        {
            RemoveError(key);
        }
    }

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != null)
        {
            if (e.PropertyName.Equals(nameof(CropType)))
            {
                // Ensure the crop type is valid
                this.ValidateCropType();
                // Keep SelectedCropTypeItem in sync
                RaisePropertyChanged(nameof(SelectedCropTypeItem));
            }
            else if (e.PropertyName.Equals(nameof(AmountOfIrrigation)))
            {
                this.ValidateAmountOfIrrigation();
            }
            else if (e.PropertyName.Equals(nameof(WetYield)))
            {
                this.ValidateWetYield();
            }
        }
    }

    #endregion
}
