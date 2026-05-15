using H.Core.Models.LandManagement.Fields;

namespace H.Core.Calculators.Infrastructure
{
    /// <summary>
    /// Partial: digestate-to-field-application math. Takes the per-day digester output
    /// (substrate flows + N + C in each fraction) and converts it into per-(field,
    /// application) N + C inputs that the soil-carbon pipeline reads through
    /// <see cref="H.Core.Services.Animals.DigestateService"/>.
    ///
    /// <para><b>Status note:</b></para>
    /// <see cref="GetAmountOfNitrogenInFieldApplication"/> is currently a stub that throws
    /// <see cref="NotImplementedException"/> — kept as the documented entry point for the
    /// per-application drawdown math that still needs to be ported from v4. Until it's wired
    /// up, digestate field-application N is computed via <c>DigestateService</c>'s simpler
    /// "fraction of total available" path.
    /// </summary>
    public partial class ADCalculator
    {
        /// <summary>
        /// <b>Not yet implemented.</b> Will return the kg N applied to a field by a single
        /// <see cref="DigestateApplicationViewItem"/>, computed as
        /// <c>fraction_of_digestate_used × total_N_in_that_fraction</c>. The fraction depends
        /// on whether the application is raw, solid, or liquid (see Table 47 coefficients).
        /// </summary>
        // Get fraction of digestate used and use that to get fraction of N or C used
        public double GetAmountOfNitrogenInFieldApplication(CropViewItem cropViewItem, DigestateApplicationViewItem viewItem, List<DigestorDailyOutput> digestorDailyOutputs)
        {
            var amountOfDigestate = viewItem.AmountAppliedPerHectare * cropViewItem.Area;
            var digestateType = viewItem.DigestateState;


            var flowRateOfAllSubstrates = digestorDailyOutputs.Sum(x => x.FlowRateOfAllSubstratesInDigestate);
            var flowRateOfLiquidFraction = digestorDailyOutputs.Sum(x => x.FlowRateLiquidFraction);
            var flowRateOfSolidFraction = digestorDailyOutputs.Sum(x => x.FlowRateSolidFraction);

            throw new NotImplementedException();
        }
    }
}