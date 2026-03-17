using H.Core.CustomAttributes;
using H.Core.Enumerations;

namespace H.Core.Factories.Crops;

public partial class CropDto
{
    #region Fields

    private bool _manureApplied;
    private double _amountOfManureApplied;
    private ManureLocationSourceType _manureLocationSourceType;
    private ManureAnimalSourceTypes _manureAnimalSourceType;
    private ManureApplicationTypes _manureApplicationType;
    private ManureStateType _manureStateType;

    #endregion

    #region Manure Properties

    public bool ManureApplied
    {
        get => _manureApplied;
        set => SetProperty(ref _manureApplied, value);
    }

    [Units(MetricUnitsOfMeasurement.KilogramsPerHectare)]
    public double AmountOfManureApplied
    {
        get => _amountOfManureApplied;
        set => SetProperty(ref _amountOfManureApplied, value);
    }

    public ManureLocationSourceType ManureLocationSourceType
    {
        get => _manureLocationSourceType;
        set => SetProperty(ref _manureLocationSourceType, value);
    }

    public ManureAnimalSourceTypes ManureAnimalSourceType
    {
        get => _manureAnimalSourceType;
        set => SetProperty(ref _manureAnimalSourceType, value);
    }

    public ManureApplicationTypes ManureApplicationType
    {
        get => _manureApplicationType;
        set => SetProperty(ref _manureApplicationType, value);
    }

    public ManureStateType ManureStateType
    {
        get => _manureStateType;
        set => SetProperty(ref _manureStateType, value);
    }

    #endregion
}
