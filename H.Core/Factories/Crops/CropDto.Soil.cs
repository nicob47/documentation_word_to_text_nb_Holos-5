namespace H.Core.Factories.Crops;

public partial class CropDto
{
    #region Fields

    private double _climateParameter;
    private double _tillageFactor;
    private double _managementFactor;

    #endregion

    #region Soil Properties

    public double ClimateParameter
    {
        get => _climateParameter;
        set => SetProperty(ref _climateParameter, value);
    }

    public double TillageFactor
    {
        get => _tillageFactor;
        set => SetProperty(ref _tillageFactor, value);
    }

    public double ManagementFactor
    {
        get => _managementFactor;
        set => SetProperty(ref _managementFactor, value);
    }

    #endregion
}
