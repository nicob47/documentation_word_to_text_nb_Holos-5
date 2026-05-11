using H.Core.Emissions.Results;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Services.Animals
{
    public interface IAnimalService
    {
        List<AnimalComponentEmissionsResults> GetAnimalResults(Farm farm);
        List<AnimalComponentEmissionsResults> GetAnimalResults(AnimalType animalType, Farm farm);
        AnimalGroupEmissionResults GetResultsForGroup(AnimalGroup animalGroup, Farm farm, AnimalComponentBase animalComponent);
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