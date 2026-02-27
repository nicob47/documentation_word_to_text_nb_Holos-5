using H.Core.Enumerations;

namespace H.Core.Factories.Animals;

public interface IManagementPeriodDto : IDto
{
    int NumberOfDays { get; set; }
    DateTime Start { get; set; }
    DateTime End { get; set; }

    ManureStateType ManureStateType { get; set; }
    HousingType HousingType { get; set; }

    // Properties with Units attributes for conversion
    double EnergyRequiredForMilk { get; set; }
    double EnergyRequiredForWool { get; set; }
    double StartWeight { get; set; }
    double EndWeight { get; set; }
    double PeriodDailyGain { get; set; }
    double MilkProduction { get; set; }
    double WoolProduction { get; set; }
    double GainCoefficientA { get; set; }
    double GainCoefficientB { get; set; }
    double LiveWeightChangeOfPregnantAnimal { get; set; }
    double LiveWeightOfYoungAtWeaningAge { get; set; }
    double LiveWeightOfYoungAtBirth { get; set; }
    double MilkFatContent { get; set; }
    double MilkProteinContentAsPercentage { get; set; }

    // Housing-related properties (from HousingDetails)
    BeddingMaterialType BeddingMaterialType { get; set; }
    double UserDefinedBeddingRate { get; set; }
    double ActivityCoefficientOfFeedingSituation { get; set; }
    double BaselineMaintenanceCoefficient { get; set; }

    // Manure-related properties (from ManureDetails)
    double MethaneConversionFactor { get; set; }
    double VolatilizationFraction { get; set; }
    double N2ODirectEmissionFactor { get; set; }
    double LeachingFraction { get; set; }
    double EmissionFactorVolatilization { get; set; }
    double EmissionFactorLeaching { get; set; }
    double AshContentOfManure { get; set; }
    double MethaneProducingCapacityOfManure { get; set; }
    double VolatileSolidExcretion { get; set; }
    double FractionOfNitrogenInManure { get; set; }
    double FractionOfCarbonInManure { get; set; }
    double FractionOfPhosphorusInManure { get; set; }

    // Diet-related properties
    DietAdditiveType DietAdditiveType { get; set; }
    DietType SelectedDietType { get; set; }
    double CrudeProtein { get; set; }
    double Forage { get; set; }
    double TotalDigestibleNutrient { get; set; }
    double DailyDryMatterFeedIntakeOfFeed { get; set; }
}