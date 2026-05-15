using H.Core.Emissions.Results;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Calculators.Nitrogen
{
    /// <summary>
    /// Partial: N₂O emissions from manure deposited directly on pasture by grazing animals
    /// (separate from manure-application math, which lives in <c>.Manure.cs</c>). Pulls grazing
    /// management periods from the animal-pipeline results that
    /// <c>FieldResultsService.CalculateFinalResultsForField</c> primes onto the calculator.
    /// </summary>
    public partial class N2OEmissionFactorCalculator
    {
        /// <summary>
        /// Direct N₂O-N from grazing animals deposited as manure on this field in the year.
        /// Walks the matching grazing slots from the field's CropViewItem and sums each
        /// management period's per-month contribution. Returns 0 if the field has no grazing
        /// view items or the field component can't be found on the farm.
        /// </summary>
        /// <param name="farm">Farm context (needed to resolve the field component by GUID).</param>
        /// <param name="currentYearResults">Field-year row being processed; carries the grazing slot list.</param>
        /// <param name="animalComponentEmissionsResultsList">Pre-computed per-animal-component results; the source of per-group, per-month grazing emissions.</param>
        /// <returns>Direct N₂O-N (kg N₂O-N) summed across all grazing slots that landed on this field this year.</returns>
        public double GetDirectN2ONFromGrazingAnimals(Farm farm, CropViewItem currentYearResults,
            List<AnimalComponentEmissionsResults> animalComponentEmissionsResultsList)
        {
            var result = 0d;
            var field = farm.GetFieldSystemComponent(currentYearResults.FieldSystemComponentGuid);
            if (field == null) return 0.0;

            foreach (var emissionsResults in animalComponentEmissionsResultsList)
            {
                foreach (var animalGroupEmissionResults in emissionsResults.EmissionResultsForAllAnimalGroupsInComponent)
                {
                    foreach (var groupEmissionsByMonth in animalGroupEmissionResults.GroupEmissionsByMonths)
                    {
                        var managementPeriod = groupEmissionsByMonth.MonthsAndDaysData.ManagementPeriod;
                        if (field.IsGrazingManagementPeriodFromPasture(managementPeriod) && managementPeriod.Start.Year == currentYearResults.Year)
                        {
                            var emissions = groupEmissionsByMonth.MonthlyManureDirectN2ONEmission;
                            result += emissions;
                        }

                    }
                }
            }

            return result;
        }

        public double GetLeachingN2ONFromGrazingAnimals(Farm farm, CropViewItem currentYearResults,
            List<AnimalComponentEmissionsResults> animalComponentEmissionsResultsList)
        {
            var result = 0d;
            var field = farm.GetFieldSystemComponent(currentYearResults.FieldSystemComponentGuid);
            if (field == null) return 0.0;

            foreach (var emissionsResults in animalComponentEmissionsResultsList)
            {
                foreach (var animalGroupEmissionResults in emissionsResults.EmissionResultsForAllAnimalGroupsInComponent)
                {
                    foreach (var groupEmissionsByMonth in animalGroupEmissionResults.GroupEmissionsByMonths)
                    {
                        var managementPeriod = groupEmissionsByMonth.MonthsAndDaysData.ManagementPeriod;
                        if (field.IsGrazingManagementPeriodFromPasture(managementPeriod) && managementPeriod.Start.Year == currentYearResults.Year)
                        {
                            var emissions = groupEmissionsByMonth.MonthlyManureLeachingN2ONEmission;
                            result += emissions;
                        }

                    }
                }
            }

            return result;
        }

        public double GetActualLeachingN2ONFromGrazingAnimals(Farm farm, CropViewItem currentYearResults,
            List<AnimalComponentEmissionsResults> animalComponentEmissionsResultsList)
        {
            var result = 0d;
            var field = farm.GetFieldSystemComponent(currentYearResults.FieldSystemComponentGuid);
            if (field == null) return 0.0;

            foreach (var emissionsResults in animalComponentEmissionsResultsList)
            {
                foreach (var animalGroupEmissionResults in emissionsResults.EmissionResultsForAllAnimalGroupsInComponent)
                {
                    foreach (var groupEmissionsByMonth in animalGroupEmissionResults.GroupEmissionsByMonths)
                    {
                        var managementPeriod = groupEmissionsByMonth.MonthsAndDaysData.ManagementPeriod;
                        if (field.IsGrazingManagementPeriodFromPasture(managementPeriod) && managementPeriod.Start.Year == currentYearResults.Year)
                        {
                            var emissions = groupEmissionsByMonth.MonthlyManureNitrateLeachingN2ONEmission;
                            result += emissions;
                        }

                    }
                }
            }

            return result;
        }

        public double GetVolatilizationN2ONFromGrazingAnimals(Farm farm, CropViewItem currentYearResults,
            List<AnimalComponentEmissionsResults> animalComponentEmissionsResultsList)
        {
            var result = 0d;
            var field = farm.GetFieldSystemComponent(currentYearResults.FieldSystemComponentGuid);
            if (field == null) return 0.0;

            foreach (var emissionsResults in animalComponentEmissionsResultsList)
            {
                foreach (var animalGroupEmissionResults in emissionsResults.EmissionResultsForAllAnimalGroupsInComponent)
                {
                    foreach (var groupEmissionsByMonth in animalGroupEmissionResults.GroupEmissionsByMonths)
                    {
                        var managementPeriod = groupEmissionsByMonth.MonthsAndDaysData.ManagementPeriod;
                        if (field.IsGrazingManagementPeriodFromPasture(managementPeriod) && managementPeriod.Start.Year == currentYearResults.Year)
                        {
                            var emissions = groupEmissionsByMonth.MonthlyManureVolatilizationN2ONEmission;
                            result += emissions;
                        }

                    }
                }
            }

            return result;
        }

        public double GetVolatilizationNH3FromGrazingAnimals(Farm farm, CropViewItem currentYearResults,
            List<AnimalComponentEmissionsResults> animalComponentEmissionsResultsList)
        {
            var result = 0d;
            var field = farm.GetFieldSystemComponent(currentYearResults.FieldSystemComponentGuid);
            if (field == null) return 0.0;

            foreach (var emissionsResults in animalComponentEmissionsResultsList)
            {
                foreach (var animalGroupEmissionResults in emissionsResults.EmissionResultsForAllAnimalGroupsInComponent)
                {
                    foreach (var groupEmissionsByMonth in animalGroupEmissionResults.GroupEmissionsByMonths)
                    {
                        var managementPeriod = groupEmissionsByMonth.MonthsAndDaysData.ManagementPeriod;
                        if (field.IsGrazingManagementPeriodFromPasture(managementPeriod) && managementPeriod.Start.Year == currentYearResults.Year)
                        {
                            var emissions = groupEmissionsByMonth.MonthlyNH3FromGrazingAnimals;
                            result += emissions;
                        }

                    }
                }
            }

            return result;
        }
    }
}