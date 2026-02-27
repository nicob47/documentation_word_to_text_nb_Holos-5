using AutoMapper;
using H.Core.Factories;
using H.Core.Factories.Animals;
using H.Core.Models.Animals;

namespace H.Core.Mappers;

public class ManagementPeriodToManagementPeriodDtoMapper : Profile
{
    public ManagementPeriodToManagementPeriodDtoMapper()
    {
        CreateMap<ManagementPeriod, ManagementPeriodDto>()

            // Housing-related: nested HousingDetails → flat DTO properties
            .ForMember(dest => dest.BeddingMaterialType, opt => opt.MapFrom(src => src.HousingDetails.BeddingMaterialType))
            .ForMember(dest => dest.UserDefinedBeddingRate, opt => opt.MapFrom(src => src.HousingDetails.UserDefinedBeddingRate))
            .ForMember(dest => dest.ActivityCoefficientOfFeedingSituation, opt => opt.MapFrom(src => src.HousingDetails.ActivityCeofficientOfFeedingSituation))
            .ForMember(dest => dest.BaselineMaintenanceCoefficient, opt => opt.MapFrom(src => src.HousingDetails.BaselineMaintenanceCoefficient))

            // Manure-related: nested ManureDetails → flat DTO properties
            .ForMember(dest => dest.MethaneConversionFactor, opt => opt.MapFrom(src => src.ManureDetails.MethaneConversionFactor))
            .ForMember(dest => dest.VolatilizationFraction, opt => opt.MapFrom(src => src.ManureDetails.VolatilizationFraction))
            .ForMember(dest => dest.N2ODirectEmissionFactor, opt => opt.MapFrom(src => src.ManureDetails.N2ODirectEmissionFactor))
            .ForMember(dest => dest.LeachingFraction, opt => opt.MapFrom(src => src.ManureDetails.LeachingFraction))
            .ForMember(dest => dest.EmissionFactorVolatilization, opt => opt.MapFrom(src => src.ManureDetails.EmissionFactorVolatilization))
            .ForMember(dest => dest.EmissionFactorLeaching, opt => opt.MapFrom(src => src.ManureDetails.EmissionFactorLeaching))
            .ForMember(dest => dest.AshContentOfManure, opt => opt.MapFrom(src => src.ManureDetails.AshContentOfManure))
            .ForMember(dest => dest.MethaneProducingCapacityOfManure, opt => opt.MapFrom(src => src.ManureDetails.MethaneProducingCapacityOfManure))
            .ForMember(dest => dest.VolatileSolidExcretion, opt => opt.MapFrom(src => src.ManureDetails.VolatileSolidExcretion))
            .ForMember(dest => dest.FractionOfNitrogenInManure, opt => opt.MapFrom(src => src.ManureDetails.FractionOfNitrogenInManure))
            .ForMember(dest => dest.FractionOfCarbonInManure, opt => opt.MapFrom(src => src.ManureDetails.FractionOfCarbonInManure))
            .ForMember(dest => dest.FractionOfPhosphorusInManure, opt => opt.MapFrom(src => src.ManureDetails.FractionOfPhosphorusInManure))

            // Diet-related: nested SelectedDiet and domain properties → flat DTO properties
            .ForMember(dest => dest.DietAdditiveType, opt => opt.MapFrom(src => src.DietAdditive))
            .ForMember(dest => dest.SelectedDietType, opt => opt.MapFrom(src => src.SelectedDiet.DietType))
            .ForMember(dest => dest.CrudeProtein, opt => opt.MapFrom(src => src.SelectedDiet.CrudeProtein))
            .ForMember(dest => dest.Forage, opt => opt.MapFrom(src => src.SelectedDiet.Forage))
            .ForMember(dest => dest.TotalDigestibleNutrient, opt => opt.MapFrom(src => src.SelectedDiet.TotalDigestibleNutrient))
            .ForMember(dest => dest.DailyDryMatterFeedIntakeOfFeed, opt => opt.MapFrom(src => src.SelectedDiet.DailyDryMatterFeedIntakeOfFeed));
    }
}
