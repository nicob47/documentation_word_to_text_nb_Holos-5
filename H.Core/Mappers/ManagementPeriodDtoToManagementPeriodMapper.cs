using AutoMapper;
using H.Core.Factories;
using H.Core.Factories.Animals;
using H.Core.Models.Animals;

namespace H.Core.Mappers;

public class ManagementPeriodDtoToManagementPeriodMapper : Profile
{
    public ManagementPeriodDtoToManagementPeriodMapper()
    {
        CreateMap<ManagementPeriodDto, ManagementPeriod>()

            // Housing-related: flat DTO properties → nested HousingDetails
            .ForMember(dest => dest.HousingDetails, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                dest.HousingDetails.BeddingMaterialType = src.BeddingMaterialType;
                dest.HousingDetails.UserDefinedBeddingRate = src.UserDefinedBeddingRate;
                dest.HousingDetails.ActivityCeofficientOfFeedingSituation = src.ActivityCoefficientOfFeedingSituation;
                dest.HousingDetails.BaselineMaintenanceCoefficient = src.BaselineMaintenanceCoefficient;
            })

            // Manure-related: flat DTO properties → nested ManureDetails
            .ForMember(dest => dest.ManureDetails, opt => opt.Ignore())
            .AfterMap((src, dest) =>
            {
                dest.ManureDetails.MethaneConversionFactor = src.MethaneConversionFactor;
                dest.ManureDetails.VolatilizationFraction = src.VolatilizationFraction;
                dest.ManureDetails.N2ODirectEmissionFactor = src.N2ODirectEmissionFactor;
                dest.ManureDetails.LeachingFraction = src.LeachingFraction;
                dest.ManureDetails.EmissionFactorVolatilization = src.EmissionFactorVolatilization;
                dest.ManureDetails.EmissionFactorLeaching = src.EmissionFactorLeaching;
                dest.ManureDetails.AshContentOfManure = src.AshContentOfManure;
                dest.ManureDetails.MethaneProducingCapacityOfManure = src.MethaneProducingCapacityOfManure;
                dest.ManureDetails.VolatileSolidExcretion = src.VolatileSolidExcretion;
                dest.ManureDetails.FractionOfNitrogenInManure = src.FractionOfNitrogenInManure;
                dest.ManureDetails.FractionOfCarbonInManure = src.FractionOfCarbonInManure;
                dest.ManureDetails.FractionOfPhosphorusInManure = src.FractionOfPhosphorusInManure;
            })

            // Diet-related: flat DTO properties → nested SelectedDiet and domain properties
            .ForMember(dest => dest.SelectedDiet, opt => opt.Ignore())
            .ForMember(dest => dest.DietAdditive, opt => opt.MapFrom(src => src.DietAdditiveType))
            .AfterMap((src, dest) =>
            {
                dest.SelectedDiet.DietType = src.SelectedDietType;
            })

            // Ignore read-only nutritional summary fields (they don't map back to domain)
            .ForSourceMember(src => src.CrudeProtein, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.Forage, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.TotalDigestibleNutrient, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.DailyDryMatterFeedIntakeOfFeed, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.ActivityCoefficientOfFeedingSituation, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.BaselineMaintenanceCoefficient, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.UserDefinedBeddingRate, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.BeddingMaterialType, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.MethaneConversionFactor, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.VolatilizationFraction, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.N2ODirectEmissionFactor, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.LeachingFraction, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.EmissionFactorVolatilization, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.EmissionFactorLeaching, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.AshContentOfManure, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.MethaneProducingCapacityOfManure, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.VolatileSolidExcretion, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.FractionOfNitrogenInManure, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.FractionOfCarbonInManure, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.FractionOfPhosphorusInManure, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.SelectedDietType, opt => opt.DoNotValidate());
    }
}
