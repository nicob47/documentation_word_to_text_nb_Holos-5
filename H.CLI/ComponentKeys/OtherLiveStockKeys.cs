using H.Core.Enumerations;
using H.CLI.Interfaces;
using H.Localization.Resources.Strings;

namespace H.CLI.ComponentKeys
{
    public class OtherLivestockKeys : AnimalKeyBase, IComponentKeys
    {
        #region Constructors

        public OtherLivestockKeys() : base()
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

            base.Keys.Add(AppStrings.Column_YearlyManureMethaneRate, ImperialUnitsOfMeasurement.PoundPerHeadPerYear);
            base.Keys.Add(AppStrings.Column_YearlyNitrogenExcretionRate, ImperialUnitsOfMeasurement.PoundPerHeadPerYear);
            base.Keys.Add(AppStrings.Column_YearlyEntericMethaneRate, ImperialUnitsOfMeasurement.PoundPerHeadPerYear);
            base.Keys.Add(AppStrings.Column_N2ODirectEmissionFactor, ImperialUnitsOfMeasurement.PoundsN2ONPerPoundN);
            base.Keys.Add(AppStrings.Column_VolatilizationFraction, null);

            base.Keys.Add(AppStrings.Column_DailyManureMethaneEmissionRate, null);
            base.Keys.Add(AppStrings.Column_MethaneProducingCapacityOfManure, null);
            base.Keys.Add(AppStrings.Column_MethaneConversionFactorOfManure, null);
            base.Keys.Add(AppStrings.Column_VolatileSolids, null);
            base.Keys.Add(AppStrings.Column_EmissionFactorVolatilization, null);
            base.Keys.Add(AppStrings.Column_EmissionFactorLeaching, null);
            base.Keys.Add(AppStrings.Column_FractionLeaching, null);
        }

        #endregion
    }
}
