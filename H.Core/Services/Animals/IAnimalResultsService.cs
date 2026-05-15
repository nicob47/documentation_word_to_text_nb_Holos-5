using H.Core.Emissions.Results;
using H.Core.Models;
using H.Core.Models.Animals;

namespace H.Core.Services.Animals
{
    /// <summary>
    /// Per-species results-service contract. Implemented once for each animal category — beef,
    /// dairy, swine, sheep, poultry, other-livestock — by classes that derive from
    /// <see cref="AnimalResultsServiceBase"/>. The species-agnostic
    /// <see cref="AnimalResultsService"/> orchestrator dispatches to whichever implementation
    /// matches the components on the farm.
    ///
    /// <para><b>Two-level rollup:</b></para>
    /// <list type="bullet">
    ///   <item><see cref="CalculateResultsForComponent"/> — one animal component (e.g. one beef herd) → per-group emission rows.</item>
    ///   <item><see cref="CalculateResultsForAnimalComponents"/> — sequence of components → per-component wrapped results, ready for downstream consumers (manure / digestate services, N₂O factor calculator, the GUI's per-component drill-down).</item>
    /// </list>
    /// </summary>
    public interface IAnimalResultsService
    {
        /// <summary>
        /// Compute per-group emission rows for a single animal component. Group means
        /// <see cref="H.Core.Models.Animals.AnimalGroup"/> — typically a cohort with shared
        /// management (e.g. "lactating cows", "growing finisher pigs"). Each group's rows are
        /// summed across the component's management periods.
        /// </summary>
        IList<AnimalGroupEmissionResults> CalculateResultsForComponent(AnimalComponentBase animalComponent, Farm farm);

        /// <summary>
        /// Component-level wrapper around <see cref="CalculateResultsForComponent"/>. Wraps each
        /// component's per-group results into an <see cref="AnimalComponentEmissionsResults"/>
        /// so callers (manure service, N₂O calculator, GUI) get a list of self-contained
        /// per-component rollups. Hits the base-class cache when the component's
        /// <c>ResultsCalculated</c> flag is still true.
        /// </summary>
        List<AnimalComponentEmissionsResults> CalculateResultsForAnimalComponents(IEnumerable<AnimalComponentBase> components, Farm farm);
    }
}