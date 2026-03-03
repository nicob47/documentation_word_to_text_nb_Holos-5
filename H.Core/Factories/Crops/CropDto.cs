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

    #endregion

    #region Constructors

    public CropDto()
    {
        // Initialize with diverse crop types representing all color categories
        this.ValidCropTypes = new ObservableCollection<CropType>() 
        { 
            CropType.NotSelected,
            
            // Cereals - Orange
            CropType.Wheat,
            CropType.Barley,
            CropType.Oats,
            CropType.Rye,
            CropType.Corn,
            CropType.GrainCorn,
            CropType.SilageCorn,
            CropType.Triticale,
            CropType.Durum,
            
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
        set => SetProperty(ref _cropTypes, value);
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