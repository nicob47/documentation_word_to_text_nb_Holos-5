using H.Core.CustomAttributes;
using H.Core.Enumerations;

namespace H.Core.Factories.Crops;

public partial class CropDto
{
    #region Fields

    private bool _cropIsGrazed;
    private double _forageUtilizationRate;

    #endregion

    #region Grazing Properties

    public bool CropIsGrazed
    {
        get => _cropIsGrazed;
        set => SetProperty(ref _cropIsGrazed, value);
    }

    [Units(MetricUnitsOfMeasurement.Percentage)]
    public double ForageUtilizationRate
    {
        get => _forageUtilizationRate;
        set => SetProperty(ref _forageUtilizationRate, value);
    }

    #endregion
}
