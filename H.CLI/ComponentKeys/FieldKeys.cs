using System.Collections.Generic;
using H.CLI.Interfaces;
using H.Core.Enumerations;
using H.Localization.Resources.Strings;

namespace H.CLI.ComponentKeys
{
    /// <summary>
    /// The dictionary below takes in a string - the header and a nullable ImperialUnitsOfMeasurement?. We do not include
    /// the MetricUnitsOfMeasurement because the calculations performed later demand that the values be in Metric.
    /// Therefore, in our parser, we need to convert all Imperial units to Metric units and to do that, we only need
    /// to know what the Imperial units are (because if its metric, we don't need to do anything because the data is already
    /// in Metric).
    /// </summary>
    public class FieldKeys : IComponentKeys
    {
        /// <summary>
        /// When you modify the key, remember to add a new property corresponding to the new key that you have added 
        /// below to the FieldTemporaryInput class in the format: "Example Format",
        /// In this case, please add a new property in the format: ExampleFormat to the concrete FieldTemporaryInput class.
        /// The order of the keys below is the order in which they will be written when creating the template files for a Field.
        /// </summary>

        public FieldKeys()
        {
            this.MissingHeaders.Add(AppStrings.Column_NitrogenFixation, false);
        }
        public Dictionary<string, ImperialUnitsOfMeasurement?> Keys { get; set; } = new Dictionary<string, ImperialUnitsOfMeasurement?>()
        {
            // Note the casing must match in the resource files (i.e. it must be Phase Number in the resource file not Phase number). This is because reflection is
            // being used

            // When adding a string here, ensure property is added to FieldTemporaryInput class as well

            // Ordering matters here, the ordering of keys here must match the ordering of the columns in the input files (i.e. in FieldProcessor class)
            {AppStrings.Column_PhaseNumber, null },
            {AppStrings.Column_Name, null },
            {AppStrings.Column_Area, ImperialUnitsOfMeasurement.Acres },
            {AppStrings.Column_CurrentYear, null },
            {AppStrings.Column_CropYear, null },
            {AppStrings.Column_CropType, null },
            {AppStrings.Column_TillageType, null },
            {AppStrings.Column_YearInPerennialStand, null },
            {AppStrings.Column_PerennialStandID, null },
            {AppStrings.Column_PerennialStandLength, null },
            {AppStrings.Column_BiomassCoefficientProduct, null },
            {AppStrings.Column_BiomassCoefficientStraw, null },
            {AppStrings.Column_BiomassCoefficientRoots, null },
            {AppStrings.Column_BiomassCoefficientExtraroot, null },
            {AppStrings.Column_NitrogenContentInProduct, ImperialUnitsOfMeasurement.PoundsPerPound },
            {AppStrings.Column_NitrogenContentInStraw, ImperialUnitsOfMeasurement.PoundsPerPound },
            {AppStrings.Column_NitrogenContentInRoots, ImperialUnitsOfMeasurement.PoundsPerPound },
            {AppStrings.Column_NitrogenContentInExtraroot, ImperialUnitsOfMeasurement.PoundsPerPound },
            {AppStrings.Column_NitrogenFixation, ImperialUnitsOfMeasurement.PoundsNitrogenPerAcrePerYear },
            {AppStrings.Column_NitrogenDeposit, ImperialUnitsOfMeasurement.PoundsNitrogenPerAcre },
            {AppStrings.Column_CarbonConcentration, ImperialUnitsOfMeasurement.PoundsPerPound },
            {AppStrings.Column_Yield, ImperialUnitsOfMeasurement.BushelsPerAcre },
            {AppStrings.Column_HarvestMethod, null },
            {AppStrings.Column_NitrogenFertilizerRate, ImperialUnitsOfMeasurement.PoundsNitrogenPerAcre },
            {AppStrings.Column_PhosphorousFertilizerRate, ImperialUnitsOfMeasurement.PoundsPhosphorousPerAcre },
            {AppStrings.Column_IsIrrigated, null },
            {AppStrings.Column_IrrigationType, null },
            {AppStrings.Column_AmountOfIrrigation, ImperialUnitsOfMeasurement.InchesToMm },
            {AppStrings.Column_MoistureContentOfCrop,  null},
            {AppStrings.Column_MoistureContentOfCropPercentage,  ImperialUnitsOfMeasurement.Percentage},
            {AppStrings.Column_PercentageOfStrawReturnedToSoil,  ImperialUnitsOfMeasurement.Percentage },
            {AppStrings.Column_PercentageOfRootsReturnedToSoil,  ImperialUnitsOfMeasurement.Percentage },
            {AppStrings.Column_PercentageOfProductYieldReturnedToSoil,  ImperialUnitsOfMeasurement.Percentage },
            {AppStrings.Column_IsPesticideUsed, null },
            {AppStrings.Column_NumberOfPesticidePasses, null },
            {AppStrings.Column_ManureApplied, null },
            {AppStrings.Column_AmountOfManureApplied, ImperialUnitsOfMeasurement.PoundsPerAcre },
            {AppStrings.Column_ManureApplicationType, null },
            {AppStrings.Column_ManureAnimalSourceType, null },
            {AppStrings.Column_ManureStateType, null },
            {AppStrings.Column_ManureLocationSourceType, null },
            {AppStrings.Column_UnderSownCropsUsed, null },
            {AppStrings.Column_CropIsGrazed, null },
            {AppStrings.Column_FieldSystemComponentGuid, null },
            {AppStrings.Column_TimePeriodCategoryString, null },
            {AppStrings.Column_ClimateParameter, null },
            {AppStrings.Column_TillageFactor, null },
            {AppStrings.Column_ManagementFactor, null },
            {AppStrings.Column_PlantCarbonInAgriculturalProduct, null },
            {AppStrings.Column_CarbonInputFromProduct, null },
            {AppStrings.Column_CarbonInputFromStraw, null },
            {AppStrings.Column_CarbonInputFromRoots, null },
            {AppStrings.Column_CarbonInputFromExtraroots, null },
            {AppStrings.Column_SizeOfFirstRotationForField, null },
            {AppStrings.Column_AboveGroundCarbonInput, null },
            {AppStrings.Column_BelowGroundCarbonInput, null },
            {AppStrings.Column_ManureCarbonInputsPerHectare, null },
            {AppStrings.Column_DigestateCarbonInputsPerHectare, null },
            {AppStrings.Column_TotalCarbonInputs, null },
            {AppStrings.Column_Sand, null },
            {AppStrings.Column_Lignin, null },
            {AppStrings.Column_WFac, null },
            {AppStrings.Column_TFac, null },
            {AppStrings.Column_TotalNitrogenInputsForIpccTier2, null },
            {AppStrings.Column_NitrogenContent, null },
            {AppStrings.Column_AboveGroundResidueDryMatter, null },
            {AppStrings.Column_BelowGroundResidueDryMatter, null },
            {AppStrings.Column_FuelEnergy, null },
            {AppStrings.Column_HerbicideEnergy, null },
            {AppStrings.Column_FertilizerBlend, null },
        };

        //  Currently only 2 optional headers in the field keys
        public bool IsHeaderOptional(string s)
        {
            if (s == AppStrings.Column_NitrogenFixation) return true;
            else if (s == AppStrings.Column_FertilizerBlend) return true;
            else if (s == AppStrings.Column_NitrogenDeposit) return true;
            else return false;
        }
        // Populate with all the keys that exist currently and tell if it is missing or not
        public Dictionary<string, bool> MissingHeaders { get; set; } = new Dictionary<string, bool>(){};
    }
}
