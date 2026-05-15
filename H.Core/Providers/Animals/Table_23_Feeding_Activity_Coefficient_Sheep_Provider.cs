using H.Core.Enumerations;
using System.Diagnostics;
using NLog;

namespace H.Core.Providers.Animals
{
    /// <summary>
    /// Table 23. Feeding activity coefficients for sheep.
    /// </summary>
    public class Table_23_Feeding_Activity_Coefficient_Sheep_Provider : IFeedingActivityCoefficientProvider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public Table_23_Feeding_Activity_Coefficient_Sheep_Provider()
        {
        }
        public IFeedingActivityCoeffientData GetByHousing(HousingType housingType)
        {
            switch (housingType)
            {
                
                case HousingType.HousedEwes: // Footnote 1
                    {
                        return new Table_17_Cattle_Feeding_Activity_Coefficient_Data()
                        {
                            FeedingActivityCoefficient = 0.0096,
                        };
                    }
                
                case HousingType.Confined: // Footnote 2
                    {
                    return new Table_17_Cattle_Feeding_Activity_Coefficient_Data()
                    {
                        FeedingActivityCoefficient = 0.0067,
                    };
                }

                case HousingType.Pasture:
                case HousingType.FlatPasture:
                {
                    return new Table_17_Cattle_Feeding_Activity_Coefficient_Data()
                    {
                        FeedingActivityCoefficient = 0.0107,
                    };
                }

                case HousingType.HillyPastureOrOpenRange:
                {
                    return new Table_17_Cattle_Feeding_Activity_Coefficient_Data()
                    {
                        FeedingActivityCoefficient = 0.024
                    };
                }

                default:
                {
                    var defaultValue = new Table_17_Cattle_Feeding_Activity_Coefficient_Data()
                    {
                        FeedingActivityCoefficient = 0
                    };

                    _log.Error($"{nameof(Table_23_Feeding_Activity_Coefficient_Sheep_Provider.GetByHousing)}" +
                    $" unable to get data for housing type: {housingType}." +
                    $" Returning default value of {defaultValue}.");
                    return defaultValue;
                }
            }
        }

        #region Footnotes

        // Footnote 1: Animals are confined due to pregnancy in final trimester (50 days) (IPCC, 2019)
        // Footnote 2: Animals housed for fattening
        #endregion
    }
}
