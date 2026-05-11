namespace H.Core.Enumerations;

/// <summary>
/// Lets CLI users override Holos's default carbon-input calculation strategy on a per-run basis.
/// </summary>
public enum ResidueInputCalculationMethod
{
    Default,
    ICBM,
    IPCCTier2,
}
