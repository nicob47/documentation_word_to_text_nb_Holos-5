namespace H.Core.Models.Results;

/// <summary>
/// One per-field, per-year row in <see cref="FarmAnalysisResults.YearResults"/>. All carbon
/// quantities are kg C ha⁻¹ unless otherwise noted.
/// </summary>
public class FieldAnalysisYearResult
{
    public int Year { get; init; }
    public string FieldName { get; init; } = string.Empty;
    public string CropType { get; init; } = string.Empty;

    /// <summary>(ha)</summary>
    public double Area { get; init; }

    /// <summary>(kg C ha⁻¹) — above-ground carbon input including main + cover crops.</summary>
    public double AboveGroundCarbonInput { get; init; }

    /// <summary>(kg C ha⁻¹) — below-ground carbon input including main + cover crops.</summary>
    public double BelowGroundCarbonInput { get; init; }

    /// <summary>(kg C ha⁻¹) — manure carbon input from applications + grazing.</summary>
    public double ManureCarbonInput { get; init; }

    /// <summary>(kg C ha⁻¹) — digestate carbon input from applications + remaining.</summary>
    public double DigestateCarbonInput { get; init; }

    /// <summary>(kg C ha⁻¹) — total carbon inputs feeding the soil-pool calculation.</summary>
    public double TotalCarbonInputs { get; init; }

    /// <summary>(Mg C ha⁻¹) — soil organic carbon stock at end of the interval.</summary>
    public double SoilCarbon { get; init; }

    /// <summary>(Mg C ha⁻¹ yr⁻¹) — change in soil organic carbon since the previous interval.</summary>
    public double ChangeInSoilCarbon { get; init; }
}
