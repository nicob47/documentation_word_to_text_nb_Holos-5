using H.Core.Emissions.Results;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Services.Animals
{
    /// <summary>
    /// Façade over the per-animal-type emission calculators (beef, dairy, swine, sheep, poultry,
    /// other-livestock). Produces the <see cref="AnimalComponentEmissionsResults"/> rows that
    /// downstream consumers — most notably the soil-carbon / nitrogen pipeline in
    /// <see cref="H.Core.Services.LandManagement.FieldResultsService"/> — read to fold animal-driven
    /// carbon and nitrogen flows (manure applications, grazing deposits, supplemental hay) into
    /// the field-level totals.
    ///
    /// <para><b>Ordering invariant:</b></para>
    /// <see cref="H.Core.Services.Analysis.FarmAnalysisService"/> calls
    /// <see cref="GetAnimalResults(Farm)"/> before <c>FieldResultsService.CalculateFinalResults</c>,
    /// then assigns the result to <c>FieldResultsService.AnimalResults</c>. If you skip that
    /// priming step, the field-level N₂O numbers come out short by the manure / grazing
    /// contributions.
    /// </para>
    /// </summary>
    public interface IAnimalService
    {
        /// <summary>
        /// Runs every animal component on the farm and returns one
        /// <see cref="AnimalComponentEmissionsResults"/> per component. Used during the analysis
        /// pass to feed the field-level nitrogen calculation.
        /// </summary>
        List<AnimalComponentEmissionsResults> GetAnimalResults(Farm farm);

        /// <summary>
        /// Filtered variant — only returns results for components matching <paramref name="animalType"/>
        /// (e.g. only beef cattle). Used by the GUI's animal-results tabs to render one species at a time.
        /// </summary>
        List<AnimalComponentEmissionsResults> GetAnimalResults(AnimalType animalType, Farm farm);

        /// <summary>
        /// Computes emissions for a single <see cref="AnimalGroup"/> aggregated over all of its
        /// management periods. Used by the per-group result drill-down in the GUI.
        /// </summary>
        AnimalGroupEmissionResults GetResultsForGroup(AnimalGroup animalGroup, Farm farm, AnimalComponentBase animalComponent);

        /// <summary>
        /// Computes emissions for a single <see cref="AnimalGroup"/> restricted to one
        /// <see cref="ManagementPeriod"/>. Used by validation paths and unit tests where the
        /// full per-group rollup isn't needed.
        /// </summary>
        AnimalGroupEmissionResults GetResultsForManagementPeriod(AnimalGroup animalGroup, Farm farm, AnimalComponentBase animalComponent, ManagementPeriod managementPeriod);

        /// <summary>
        /// Returns the per-month emissions for the animal group placed on the given grazing slot,
        /// filtered to management periods whose housing is pasture and whose date range falls within
        /// the grazing slot's start/end.
        /// </summary>
        List<GroupEmissionsByMonth> GetGroupEmissionsFromGrazingAnimals(
            List<AnimalComponentEmissionsResults> results,
            GrazingViewItem grazingViewItem);

        /// <summary>
        /// Selects the management periods for the given animal group whose housing is pasture
        /// (per Chapter 11 / Appendix methodology).
        /// </summary>
        List<ManagementPeriod> GetGrazingManagementPeriods(
            AnimalGroup animalGroup,
            FieldSystemComponent fieldSystemComponent);
    }
}