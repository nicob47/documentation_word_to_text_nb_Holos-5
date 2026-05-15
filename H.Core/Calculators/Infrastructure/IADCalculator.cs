using H.Core.Emissions.Results;
using H.Core.Models;

namespace H.Core.Calculators.Infrastructure
{
    /// <summary>
    /// Anaerobic-digestion (AD) calculator contract. Given a farm that has an
    /// <see cref="H.Core.Models.AnaerobicDigestionComponent"/> plus the precomputed per-animal-
    /// component results (which provide the manure substrate inputs), produces per-day digester
    /// output rows (<see cref="DigestorDailyOutput"/>) describing biogas / methane production,
    /// digestate composition (raw / solid / liquid), and downstream emissions during storage.
    ///
    /// <para>
    /// The output feeds <see cref="H.Core.Services.Animals.IDigestateService"/>, which then
    /// surfaces per-field N + C contributions to the N₂O calculator and the soil-C pool
    /// dynamics — same shape as the manure path.
    /// </para>
    /// </summary>
    public interface IADCalculator
    {
        /// <summary>
        /// Run the AD calculation for every AD component on the farm. Returns an empty list
        /// when the farm has no AD components — the digestate service short-circuits before
        /// calling this in that case, so the empty result is just defensive.
        /// </summary>
        /// <param name="farm">Farm context. AD components are read from <c>farm.AnaerobicDigestionComponents</c>.</param>
        /// <param name="animalComponentEmissionsResults">Per-animal-component results that supply fresh-manure substrate inputs.</param>
        /// <returns>One <see cref="DigestorDailyOutput"/> per day per AD component over the analysis period.</returns>
        List<DigestorDailyOutput> CalculateResults(Farm farm, List<AnimalComponentEmissionsResults> animalComponentEmissionsResults);
    }
}