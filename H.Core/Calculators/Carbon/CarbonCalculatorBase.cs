using H.Core.Emissions.Results;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services.Animals;

namespace H.Core.Calculators.Carbon
{
    /// <summary>
    /// Shared base for the two soil-carbon calculators (<see cref="ICBMSoilCarbonCalculator"/> +
    /// <see cref="IPCCTier2SoilCarbonCalculator"/>). Holds the cross-cutting state both models
    /// read (animal emissions, climate provider, N₂O factor calculator, digestate service) and
    /// the nitrogen-pool flow machinery they share — see partial
    /// <c>CarbonCalculatorBase.Nitrogen.cs</c>.
    ///
    /// <para><b>Why this base exists:</b></para>
    /// ICBM and Tier 2 use different pool topologies for carbon (4 ICBM pools vs 3 Tier 2 pools)
    /// but the nitrogen side is much more shared — both compute residue N pools, manure /
    /// digestate N, mineralization, leaching, etc. against the same N₂O emission factors. Pulling
    /// that into a base class avoided duplicating ~1000 lines of N math between the two
    /// calculators.
    ///
    /// <para><b>How subclasses customize it:</b></para>
    /// The N pass calls into <c>SetOrganicNitrogenPoolStartState()</c> (and a few siblings) which
    /// each subclass overrides with its own start-of-year seed. After the seed runs the rest of
    /// the N flow lives here on the base.
    /// </summary>
    public abstract partial class CarbonCalculatorBase
    {
        #region Fields

        /// <summary>
        /// Service that computes digestate C / N application contributions from any
        /// AnaerobicDigestionComponent on the farm. Shared default-constructed instance — the
        /// service is stateless aside from a per-farm initialize step.
        /// </summary>
        protected DigestateService _digestateService = new DigestateService();

        #endregion

        #region Constructors

        /// <summary>
        /// Initialises an empty <see cref="AnimalComponentEmissionsResults"/> list so subclasses
        /// can dereference it before the orchestrator (<c>FieldResultsService.CalculateFinalResultsForField</c>)
        /// primes it with the real animal results from <see cref="H.Core.Services.Animals.IAnimalService"/>.
        /// </summary>
        protected CarbonCalculatorBase()
        {
            this.AnimalComponentEmissionsResults = new List<AnimalComponentEmissionsResults>();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Per-animal-component emission results, used by the manure / grazing / digestate N
        /// flows to fold animal-driven N into the field-level totals. Must be assigned <i>after</i>
        /// the animal pipeline runs and <i>before</i> the final carbon / N pass — the orchestrator
        /// in <c>FieldResultsService.CalculateFinalResultsForField</c> handles the ordering.
        /// </summary>
        public List<AnimalComponentEmissionsResults> AnimalComponentEmissionsResults { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Equation 2.2.2-26
        /// 
        /// Calculate amount of carbon input from all manure applications in a year.
        /// </summary>
        /// <returns>The amount of carbon input during the year (kg C ha^-1)</returns>
        public double CalculateManureCarbonInputPerHectare(
            CropViewItem viewItem)
        {
            return viewItem.GetTotalCarbonFromAppliedManure() / viewItem.Area;
        }

        /// <summary>
        /// Equation 4.9.7-1
        /// Equation 4.9.7-2
        /// Equation 4.9.7-5
        /// 
        /// Calculate amount of carbon input from all digestate applications in a year.
        /// </summary>
        /// <returns>The amount of carbon input during the year (kg C ha^-1)</returns>
        public double CalculateDigestateCarbonInputPerHectare(
            CropViewItem viewItem,
            Farm farm)
        {
            var result = 0d;

            foreach (var digestateApplicationViewItem in viewItem.DigestateApplicationViewItems)
            {
                result += digestateApplicationViewItem.AmountOfCarbonAppliedPerHectare;
            }

            return result;
        }

        /// <summary>
        /// Equation 2.1.2-34
        /// Equation 2.1.2-2
        ///
        /// (kg C ha^-1)
        /// </summary>
        public double CalculateInputsFromSupplementalHayFedToGrazingAnimals(
            CropViewItem? previousYearViewItem,
            CropViewItem currentYearViewItem,
            CropViewItem? nextYearViewItems,
            Farm farm)
        {
            var result = 0.0;

            // Get total amount of supplemental hay added
            var hayImportViewItems = currentYearViewItem.HayImportViewItems;
            foreach (var hayImportViewItem in hayImportViewItems)
            {
                // Total dry matter weight
                var totalDryMatterWeight = hayImportViewItem.GetTotalDryMatterWeightOfAllBales();

                // Amount lost during feeding
                var loss = farm.Defaults.DefaultSupplementalFeedingLossPercentage / 100;

                // Total additional carbon that must be added to above ground inputs for the field - NOTE: moisture content is already considered in the above method call and so it
                // is not included here as it is in the equation from the algorithm document
                var totalCarbon = (totalDryMatterWeight * (loss)) * currentYearViewItem.CarbonConcentration;

                result += totalCarbon;
            }

            return (result / currentYearViewItem.Area);
        }

        #endregion
    }
}