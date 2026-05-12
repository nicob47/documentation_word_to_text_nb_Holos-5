namespace H.Core.Models.Results;

/// <summary>
/// Flattened result of running the carbon model on a farm. Produced by
/// <see cref="H.Core.Services.Analysis.IFarmAnalysisService"/> from the per-year
/// <c>CropViewItem</c> list that <c>FieldResultsService.CalculateFinalResults</c> returns.
///
/// The DTO is what the GUI binds to — keeping the view models decoupled from the
/// heavyweight CropViewItem aggregate.
/// </summary>
public class FarmAnalysisResults
{
    public string FarmName { get; init; } = string.Empty;
    public string Province { get; init; } = string.Empty;

    /// <summary>
    /// Which carbon model the analysis ran under — ICBM or IPCC Tier 2. Helpful when displaying
    /// results so the user knows which strategy produced the numbers.
    /// </summary>
    public string CarbonModellingStrategy { get; init; } = string.Empty;

    /// <summary>
    /// Per-field, per-year carbon results. One row for every (field, year) pair that the analysis
    /// produced — flattened so it binds straight into a DataGrid.
    /// </summary>
    public IReadOnlyList<FieldAnalysisYearResult> YearResults { get; init; } =
        new List<FieldAnalysisYearResult>();

    /// <summary>
    /// Per-shelterbelt, per-year carbon results. Empty when the farm has no shelterbelt
    /// components. Computed independently of <see cref="YearResults"/> via
    /// <c>ShelterbeltCalculator</c> — shelterbelts have their own allometric model and don't
    /// feed into the field-level ICBM / Tier 2 calculations.
    /// </summary>
    public IReadOnlyList<ShelterbeltYearResult> ShelterbeltYearResults { get; init; } =
        new List<ShelterbeltYearResult>();

    /// <summary>
    /// True when neither fields nor shelterbelts produced any year results.
    /// </summary>
    public bool IsEmpty => YearResults.Count == 0 && ShelterbeltYearResults.Count == 0;
}
