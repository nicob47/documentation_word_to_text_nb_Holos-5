using H.Core.CustomAttributes;
using H.Core.Enumerations;

namespace H.Core.Factories.Crops;

public partial class CropDto
{
    #region Fields

    private double _dryYield;
    private double _moistureContentOfCropPercentage;
    private TillageType _tillageType;
    private HarvestMethods _harvestMethod;
    private int _numberOfPesticidePasses;

    #endregion

    #region General Properties

    [Units(MetricUnitsOfMeasurement.KilogramsPerHectare)]
    public double DryYield
    {
        get => _dryYield;
        set => SetProperty(ref _dryYield, value);
    }

    [Units(MetricUnitsOfMeasurement.Percentage)]
    public double MoistureContentOfCropPercentage
    {
        get => _moistureContentOfCropPercentage;
        set => SetProperty(ref _moistureContentOfCropPercentage, value);
    }

    public TillageType TillageType
    {
        get => _tillageType;
        set => SetProperty(ref _tillageType, value);
    }

    public HarvestMethods HarvestMethod
    {
        get => _harvestMethod;
        set => SetProperty(ref _harvestMethod, value);
    }

    public int NumberOfPesticidePasses
    {
        get => _numberOfPesticidePasses;
        set => SetProperty(ref _numberOfPesticidePasses, value);
    }

    #endregion
}
