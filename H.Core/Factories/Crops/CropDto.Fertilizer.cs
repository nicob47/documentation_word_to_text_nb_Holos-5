using H.Core.CustomAttributes;
using H.Core.Enumerations;

namespace H.Core.Factories.Crops;

public partial class CropDto
{
    #region Fields

    private double _nitrogenFertilizerRate;
    private double _phosphorusFertilizerRate;
    private double _potassiumFertilizerRate;
    private double _sulphurFertilizerRate;
    private NitrogenFertilizerType _nitrogenFertilizerType;
    private FertilizerBlends _fertilizerBlend;
    private SoilReductionFactors _soilReductionFactor;
    private double _customReductionFactor;

    #endregion

    #region Fertilizer Properties

    [Units(MetricUnitsOfMeasurement.KilogramsNitrogenPerHectare)]
    public double NitrogenFertilizerRate
    {
        get => _nitrogenFertilizerRate;
        set => SetProperty(ref _nitrogenFertilizerRate, value);
    }

    [Units(MetricUnitsOfMeasurement.KilogramsPhosphorousPerHectare)]
    public double PhosphorusFertilizerRate
    {
        get => _phosphorusFertilizerRate;
        set => SetProperty(ref _phosphorusFertilizerRate, value);
    }

    [Units(MetricUnitsOfMeasurement.KilogramsPotassiumPerHectare)]
    public double PotassiumFertilizerRate
    {
        get => _potassiumFertilizerRate;
        set => SetProperty(ref _potassiumFertilizerRate, value);
    }

    [Units(MetricUnitsOfMeasurement.KilogramsSulphurPerHectare)]
    public double SulphurFertilizerRate
    {
        get => _sulphurFertilizerRate;
        set => SetProperty(ref _sulphurFertilizerRate, value);
    }

    public NitrogenFertilizerType NitrogenFertilizerType
    {
        get => _nitrogenFertilizerType;
        set => SetProperty(ref _nitrogenFertilizerType, value);
    }

    public FertilizerBlends FertilizerBlend
    {
        get => _fertilizerBlend;
        set => SetProperty(ref _fertilizerBlend, value);
    }

    public SoilReductionFactors SoilReductionFactor
    {
        get => _soilReductionFactor;
        set => SetProperty(ref _soilReductionFactor, value);
    }

    public double CustomReductionFactor
    {
        get => _customReductionFactor;
        set => SetProperty(ref _customReductionFactor, value);
    }

    #endregion
}
