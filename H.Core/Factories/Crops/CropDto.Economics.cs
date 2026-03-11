namespace H.Core.Factories.Crops;

public partial class CropDto
{
    #region Fields

    private bool _cropEconomicDataApplied;

    #endregion

    #region Economics Properties

    public bool CropEconomicDataApplied
    {
        get => _cropEconomicDataApplied;
        set => SetProperty(ref _cropEconomicDataApplied, value);
    }

    #endregion
}
