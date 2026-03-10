using H.CLI.Interfaces;
using H.Core.Enumerations;
using H.Localization.Resources.Strings;

namespace H.CLI.ComponentKeys
{

    public class SwineKeys : AnimalKeyBase, IComponentKeys
    {
        #region Constructors

        public SwineKeys()
        {
            base.Keys.Add(AppStrings.Column_Name, null);
            base.Keys.Add(H.Core.Properties.Resources.ComponentType, null);
            base.Keys.Add(AppStrings.Column_GroupName, null);
            base.Keys.Add(AppStrings.Column_GroupType, null);

            base.Keys.Add(AppStrings.Column_ManagementPeriodName, null);
            base.Keys.Add(AppStrings.Column_ManagementPeriodStartDate, null);
            base.Keys.Add(AppStrings.Column_ManagementPeriodDays, null);
            base.Keys.Add(AppStrings.Column_NumberOfAnimals, null);
            base.Keys.Add(AppStrings.Column_ProductionStage, null);
            base.Keys.Add(AppStrings.Column_StartWeight, ImperialUnitsOfMeasurement.Pounds);
            base.Keys.Add(AppStrings.Column_EndWeight, ImperialUnitsOfMeasurement.Pounds);

            base.Keys.Add(AppStrings.Column_DietAdditiveType, null);
            base.Keys.Add(AppStrings.Column_FeedIntake, ImperialUnitsOfMeasurement.PoundPerHeadPerDay);
            base.Keys.Add(AppStrings.Column_CrudeProtein, ImperialUnitsOfMeasurement.PoundsPerPound);
            base.Keys.Add(AppStrings.Column_AshContentOfDiet, ImperialUnitsOfMeasurement.PoundsPerPound);
            base.Keys.Add(AppStrings.Column_Forage, ImperialUnitsOfMeasurement.PercentageDryMatter);
            base.Keys.Add(AppStrings.Column_TDN, ImperialUnitsOfMeasurement.PercentageDryMatter);
            base.Keys.Add(AppStrings.Column_Starch, ImperialUnitsOfMeasurement.PercentageDryMatter);
            base.Keys.Add(AppStrings.Column_Fat, ImperialUnitsOfMeasurement.PercentageDryMatter);
            base.Keys.Add(AppStrings.Column_ME, ImperialUnitsOfMeasurement.BritishThermalUnitPerPound);
            base.Keys.Add(AppStrings.Column_NDF, ImperialUnitsOfMeasurement.PercentageDryMatter);
            base.Keys.Add(AppStrings.Column_VolatileSolidAdjusted, ImperialUnitsOfMeasurement.PoundsPerPound);
            base.Keys.Add(AppStrings.Column_NitrogenExcretionAdjusted, ImperialUnitsOfMeasurement.PoundsPerPound);

            base.Keys.Add(AppStrings.Column_HousingType, null);
            base.Keys.Add(AppStrings.Column_ActivityCoefficientOfFeedingSituation, ImperialUnitsOfMeasurement.BritishThermalUnitPerDayPerPound);
            base.Keys.Add(AppStrings.Column_CFTemp, ImperialUnitsOfMeasurement.BritishThermalUnitPerDayPerPound);
            base.Keys.Add(AppStrings.Column_UserDefinedBeddingRate, null);
            base.Keys.Add(AppStrings.Column_TotalCarbonKilogramsDryMatterForBedding, null);
            base.Keys.Add(AppStrings.Column_TotalNitrogenKilogramsDryMatterForBedding, null);
            base.Keys.Add(AppStrings.Column_MoistureContentOfBeddingMaterial, null);

            base.Keys.Add(AppStrings.Column_MethaneConversionFactorOfManure, ImperialUnitsOfMeasurement.PoundsMethanePerPoundMethane);
            base.Keys.Add(AppStrings.Column_MethaneProducingCapacityOfManure, null);
            base.Keys.Add(AppStrings.Column_N2ODirectEmissionFactor, ImperialUnitsOfMeasurement.PoundsN2ONPerPoundN);
            base.Keys.Add(AppStrings.Column_EmissionFactorVolatilization, null);
            base.Keys.Add(AppStrings.Column_VolatilizationFraction, null);
            base.Keys.Add(AppStrings.Column_EmissionFactorLeaching, null);
            base.Keys.Add(AppStrings.Column_FractionLeaching, null);
            base.Keys.Add(AppStrings.Column_AshContent, null);
            base.Keys.Add(AppStrings.Column_VolatileSolidsExcretion, ImperialUnitsOfMeasurement.PoundsPerPound);
            base.Keys.Add(AppStrings.Column_YearlyEntericMethaneRate, ImperialUnitsOfMeasurement.PoundPerHeadPerYear);
            base.Keys.Add(AppStrings.Column_ManureStateType, null);
        }

        #endregion
    }
}
