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
    /// True when no fields produced any year results (empty farm or empty stage state).
    /// </summary>
    public bool IsEmpty => YearResults.Count == 0;
}
