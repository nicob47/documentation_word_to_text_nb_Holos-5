using H.Core.CustomAttributes;
using H.Core.Enumerations;

namespace H.Core.Factories.Crops;

public partial class CropDto
{
    #region Fields

    private double _biomassCoefficientProduct;
    private double _biomassCoefficientStraw;
    private double _biomassCoefficientRoots;
    private double _biomassCoefficientExtraroot;
    private double _percentageOfProductYieldReturnedToSoil;
    private double _percentageOfStrawReturnedToSoil;
    private double _percentageOfRootsReturnedToSoil;
    private double _nitrogenContentInProduct;
    private double _nitrogenContentInStraw;
    private double _nitrogenContentInRoots;
    private double _nitrogenContentInExtraroot;
    private double _carbonConcentration;
    private double _ligninContent;

    #endregion

    #region Residue Properties

    /// <summary>
    /// R_p (unitless)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.None)]
    public double BiomassCoefficientProduct
    {
        get => _biomassCoefficientProduct;
        set => SetProperty(ref _biomassCoefficientProduct, value);
    }

    /// <summary>
    /// R_s (unitless)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.None)]
    public double BiomassCoefficientStraw
    {
        get => _biomassCoefficientStraw;
        set => SetProperty(ref _biomassCoefficientStraw, value);
    }

    /// <summary>
    /// R_r (unitless)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.None)]
    public double BiomassCoefficientRoots
    {
        get => _biomassCoefficientRoots;
        set => SetProperty(ref _biomassCoefficientRoots, value);
    }

    /// <summary>
    /// R_e (unitless)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.None)]
    public double BiomassCoefficientExtraroot
    {
        get => _biomassCoefficientExtraroot;
        set => SetProperty(ref _biomassCoefficientExtraroot, value);
    }

    [Units(MetricUnitsOfMeasurement.Percentage)]
    public double PercentageOfProductYieldReturnedToSoil
    {
        get => _percentageOfProductYieldReturnedToSoil;
        set => SetProperty(ref _percentageOfProductYieldReturnedToSoil, value);
    }

    [Units(MetricUnitsOfMeasurement.Percentage)]
    public double PercentageOfStrawReturnedToSoil
    {
        get => _percentageOfStrawReturnedToSoil;
        set => SetProperty(ref _percentageOfStrawReturnedToSoil, value);
    }

    [Units(MetricUnitsOfMeasurement.Percentage)]
    public double PercentageOfRootsReturnedToSoil
    {
        get => _percentageOfRootsReturnedToSoil;
        set => SetProperty(ref _percentageOfRootsReturnedToSoil, value);
    }

    public double NitrogenContentInProduct
    {
        get => _nitrogenContentInProduct;
        set => SetProperty(ref _nitrogenContentInProduct, value);
    }

    public double NitrogenContentInStraw
    {
        get => _nitrogenContentInStraw;
        set => SetProperty(ref _nitrogenContentInStraw, value);
    }

    public double NitrogenContentInRoots
    {
        get => _nitrogenContentInRoots;
        set => SetProperty(ref _nitrogenContentInRoots, value);
    }

    public double NitrogenContentInExtraroot
    {
        get => _nitrogenContentInExtraroot;
        set => SetProperty(ref _nitrogenContentInExtraroot, value);
    }

    public double CarbonConcentration
    {
        get => _carbonConcentration;
        set => SetProperty(ref _carbonConcentration, value);
    }

    public double LigninContent
    {
        get => _ligninContent;
        set => SetProperty(ref _ligninContent, value);
    }

    #endregion
}
