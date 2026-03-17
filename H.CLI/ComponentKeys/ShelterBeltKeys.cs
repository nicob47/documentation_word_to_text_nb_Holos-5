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
    /// in Metric). The Imperial Units will be used in our ConvertToMetricFromImperial method using a switch statement based
    /// on the ImperialUnitsOfMeasurement here.
    /// </summary>
    public class ShelterBeltKeys : IComponentKeys
    {
        /// <summary>
        /// When you modify the key, remember to add a new property corresponding to the new key that you have added 
        /// below to the ShelterBeltTemporaryInput class in the format: "Example Format",
        /// In this case, please add a new property in the format: ExampleFormat to the concrete ShelterbeltTemporaryInput class.
        /// The order of the keys below is the order in which they will be written when creating the template files for a Shelterbelt
        /// </summary>

        public Dictionary<string, ImperialUnitsOfMeasurement?> Keys { get; set; } = new Dictionary<string, ImperialUnitsOfMeasurement?>
        {
            {AppStrings.Column_HardinessZone, null},
            {AppStrings.Column_EcodistrictId, null },
            {AppStrings.Column_YearOfObservation, null},
            {AppStrings.Column_Name, null},
            {AppStrings.Column_RowName, null},
            {AppStrings.Column_RowId, null},
            {AppStrings.Column_RowLength, ImperialUnitsOfMeasurement.Yards},
            {AppStrings.Column_PlantYear, null },
            {AppStrings.Column_CutYear, null },
            {AppStrings.Column_Species, null },
            {AppStrings.Column_PlantedTreeCount, null },
            {AppStrings.Column_LiveTreeCount, null },
            {AppStrings.Column_PlantedTreeSpacing, ImperialUnitsOfMeasurement.Yards },
            {AppStrings.Column_AverageCircumference, ImperialUnitsOfMeasurement.InchesToCm },
        };

        public bool IsHeaderOptional(string s)
        {
            return false;
        }
        public Dictionary<string, bool> MissingHeaders { get; set; } = new Dictionary<string, bool>();
    }

}
