namespace H.Core.Models.Results;

/// <summary>
/// One per-shelterbelt, per-year row in <see cref="FarmAnalysisResults.ShelterbeltYearResults"/>.
/// Carbon quantities are in Mg C km⁻¹; year-over-year changes are in Mg C km⁻¹ yr⁻¹.
/// </summary>
public class ShelterbeltYearResult
{
    public int Year { get; init; }

    public string ShelterbeltName { get; init; } = string.Empty;

    /// <summary>(Mg C km⁻¹) — accumulated living biomass carbon.</summary>
    public double TotalLivingBiomassCarbon { get; init; }

    /// <summary>(Mg C km⁻¹ yr⁻¹) — change in living biomass C since the previous year.</summary>
    public double TotalLivingBiomassCarbonChange { get; init; }

    /// <summary>(Mg C km⁻¹) — accumulated dead-organic-matter carbon.</summary>
    public double TotalDeadOrganicMatterCarbon { get; init; }

    /// <summary>(Mg C km⁻¹ yr⁻¹) — change in DOM C since the previous year.</summary>
    public double TotalDeadOrganicMatterChange { get; init; }

    /// <summary>(Mg C km⁻¹) — accumulated total ecosystem carbon.</summary>
    public double TotalEcosystemCarbon { get; init; }

    /// <summary>(Mg C km⁻¹ yr⁻¹) — change in total ecosystem C since the previous year.</summary>
    public double TotalEcosystemCarbonChange { get; init; }
}
