using H.Core.Factories.Animals;
using H.Core.Models.Animals;

namespace H.Core.Mappers;

public class ManagementPeriodDtoToManagementPeriodMapper : IModelMapper<ManagementPeriodDto, ManagementPeriod>
{
    public ManagementPeriod Map(ManagementPeriodDto source)
    {
        var dest = PropertyMapper.Map<ManagementPeriodDto, ManagementPeriod>(source);

        // Map DietAdditiveType → DietAdditive (name mismatch)
        dest.DietAdditive = source.DietAdditiveType;

        // Housing-related: flat DTO properties → nested HousingDetails
        dest.HousingDetails.BeddingMaterialType = source.BeddingMaterialType;
        dest.HousingDetails.UserDefinedBeddingRate = source.UserDefinedBeddingRate;
        dest.HousingDetails.ActivityCeofficientOfFeedingSituation = source.ActivityCoefficientOfFeedingSituation;
        dest.HousingDetails.BaselineMaintenanceCoefficient = source.BaselineMaintenanceCoefficient;

        // Manure-related: flat DTO properties → nested ManureDetails
        dest.ManureDetails.MethaneConversionFactor = source.MethaneConversionFactor;
        dest.ManureDetails.VolatilizationFraction = source.VolatilizationFraction;
        dest.ManureDetails.N2ODirectEmissionFactor = source.N2ODirectEmissionFactor;
        dest.ManureDetails.LeachingFraction = source.LeachingFraction;
        dest.ManureDetails.EmissionFactorVolatilization = source.EmissionFactorVolatilization;
        dest.ManureDetails.EmissionFactorLeaching = source.EmissionFactorLeaching;
        dest.ManureDetails.AshContentOfManure = source.AshContentOfManure;
        dest.ManureDetails.MethaneProducingCapacityOfManure = source.MethaneProducingCapacityOfManure;
        dest.ManureDetails.VolatileSolidExcretion = source.VolatileSolidExcretion;
        dest.ManureDetails.FractionOfNitrogenInManure = source.FractionOfNitrogenInManure;
        dest.ManureDetails.FractionOfCarbonInManure = source.FractionOfCarbonInManure;
        dest.ManureDetails.FractionOfPhosphorusInManure = source.FractionOfPhosphorusInManure;

        // Diet-related: flat DTO properties → nested SelectedDiet
        dest.SelectedDiet.DietType = source.SelectedDietType;

        return dest;
    }
}
