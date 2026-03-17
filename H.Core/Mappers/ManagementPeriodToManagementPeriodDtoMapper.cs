using H.Core.Factories.Animals;
using H.Core.Models.Animals;

namespace H.Core.Mappers;

public class ManagementPeriodToManagementPeriodDtoMapper : IModelMapper<ManagementPeriod, ManagementPeriodDto>
{
    public ManagementPeriodDto Map(ManagementPeriod source)
    {
        var dest = PropertyMapper.Map<ManagementPeriod, ManagementPeriodDto>(source);

        // Housing-related: nested HousingDetails → flat DTO properties
        dest.BeddingMaterialType = source.HousingDetails.BeddingMaterialType;
        dest.UserDefinedBeddingRate = source.HousingDetails.UserDefinedBeddingRate;
        dest.ActivityCoefficientOfFeedingSituation = source.HousingDetails.ActivityCeofficientOfFeedingSituation;
        dest.BaselineMaintenanceCoefficient = source.HousingDetails.BaselineMaintenanceCoefficient;

        // Manure-related: nested ManureDetails → flat DTO properties
        dest.MethaneConversionFactor = source.ManureDetails.MethaneConversionFactor;
        dest.VolatilizationFraction = source.ManureDetails.VolatilizationFraction;
        dest.N2ODirectEmissionFactor = source.ManureDetails.N2ODirectEmissionFactor;
        dest.LeachingFraction = source.ManureDetails.LeachingFraction;
        dest.EmissionFactorVolatilization = source.ManureDetails.EmissionFactorVolatilization;
        dest.EmissionFactorLeaching = source.ManureDetails.EmissionFactorLeaching;
        dest.AshContentOfManure = source.ManureDetails.AshContentOfManure;
        dest.MethaneProducingCapacityOfManure = source.ManureDetails.MethaneProducingCapacityOfManure;
        dest.VolatileSolidExcretion = source.ManureDetails.VolatileSolidExcretion;
        dest.FractionOfNitrogenInManure = source.ManureDetails.FractionOfNitrogenInManure;
        dest.FractionOfCarbonInManure = source.ManureDetails.FractionOfCarbonInManure;
        dest.FractionOfPhosphorusInManure = source.ManureDetails.FractionOfPhosphorusInManure;

        // Diet-related: nested SelectedDiet and domain properties → flat DTO properties
        dest.DietAdditiveType = source.DietAdditive;
        dest.SelectedDietType = source.SelectedDiet.DietType;
        dest.CrudeProtein = source.SelectedDiet.CrudeProtein;
        dest.Forage = source.SelectedDiet.Forage;
        dest.TotalDigestibleNutrient = source.SelectedDiet.TotalDigestibleNutrient;
        dest.DailyDryMatterFeedIntakeOfFeed = source.SelectedDiet.DailyDryMatterFeedIntakeOfFeed;

        return dest;
    }
}
