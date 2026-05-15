using H.Core.Calculators.Infrastructure;
using H.Core.Emissions.Results;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.Infrastructure;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Services.Animals
{
    /// <summary>
    /// Source of truth for digestate C and N flows on farms that have an
    /// <see cref="AnaerobicDigestionComponent"/>. Digestate is the nitrogen-rich liquid /
    /// solid output produced when manure and other organic substrates run through an
    /// anaerobic digester. Once produced it behaves like manure — gets stored, applied to
    /// fields, exported — but with different emission-factor curves (more alkaline, higher
    /// NH₃ loss).
    ///
    /// <para><b>Parallel to <see cref="IManureService"/>:</b></para>
    /// Both services follow the same lifecycle pattern (<see cref="Initialize"/> with farm
    /// context + precomputed animal results, then per-application / per-field lookups). The
    /// soil-carbon and nitrogen calculators call both during the final pass so digestate
    /// contributions show up alongside manure contributions in the year-end totals.
    ///
    /// <para>
    /// Most farms have no AD component, in which case every lookup short-circuits to 0 and
    /// this service is effectively a no-op.
    /// </para>
    /// </summary>
    public interface IDigestateService
    {
        /// <summary>
        /// Prime the service with the farm and precomputed animal results. The AD calculator
        /// reads animal manure production as the substrate input for its daily-output run.
        /// </summary>
        void Initialize(Farm farm, List<AnimalComponentEmissionsResults> animalComponentEmissionsResults);

        DateTime GetDateOfMaximumAvailableDigestate(Farm farm, DigestateState state, int year,
            List<DigestorDailyOutput> digestorDailyOutputs, bool subtractFieldAppliedAmounts);
        DigestateTank GetTank(Farm farm, DateTime targetDate, List<DigestorDailyOutput> dailyOutputs);
        List<DigestorDailyOutput> GetDailyResults(Farm farm);

        List<ManureLocationSourceType> GetValidDigestateLocationSourceTypes();

        /// <summary>
        /// Returns the total amount of N applied (to the entire field) from a digestate field application.
        /// 
        /// (kg N)
        /// </summary>
        double CalculateTotalNitrogenFromDigestateApplication(
            CropViewItem cropViewItem,
            DigestateApplicationViewItem digestateApplicationViewItem,
            DigestateTank tank);

        /// <summary>
        /// (kg C)
        /// </summary>
        double CalculateTotalCarbonFromDigestateApplication(
            CropViewItem cropViewItem,
            DigestateApplicationViewItem digestateApplicationViewItem,
            DigestateTank tank);

        /// <summary>
        /// Equation 4.6.1-4
        /// 
        /// Stored nitrogen available for application to land minus digestate applied to fields or exported
        ///
        /// (kg N)
        /// </summary>
        double GetTotalNitrogenRemainingAtEndOfYear(int year, Farm farm);

        double GetTotalNitrogenExported(int year, Farm farm);

        /// <summary>
        /// Equation 4.9.7-3
        /// </summary>
        double GetTotalCarbonRemainingAtEndOfYear(int year, Farm farm, AnaerobicDigestionComponent component);
        double GetTotalAmountOfDigestateAppliedOnDay(DateTime dateTime, Farm farm, DigestateState state,
            ManureLocationSourceType sourceLocation);

        /// <summary>
        /// Equation 4.9.7-5
        /// </summary>
        double GetTotalCarbonForField(CropViewItem cropViewItem, int year, Farm farm, AnaerobicDigestionComponent component);

        /// <summary>
        /// Equations 4.9.7-1, 4.9.7-2, 4.9.7-5
        ///
        /// Per-hectare digestate carbon for a field, including local-application
        /// inputs plus a share of the year-end remaining digestate carbon.
        ///
        /// (kg C ha^-1)
        /// </summary>
        double GetTotalDigestateCarbonInputsForField(Farm farm, int year, CropViewItem viewItem);

        List<AnimalComponentEmissionsResults> AnimalResults { get; set; }
    }
}