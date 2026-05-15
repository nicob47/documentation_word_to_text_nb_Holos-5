#region Imports

using H.Core.Enumerations;
using NLog;

#endregion

namespace H.Core.Providers.Animals
{
    /// <summary>
    /// Table 17. Feeding activity coefficients (Ca) for beef cattle
    /// Source: IPCC (2019, Table 10.5).
    /// </summary>
    public class Table_17_Beef_Dairy_Cattle_Feeding_Activity_Coefficient_Provider : IFeedingActivityCoefficientProvider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public Table_17_Beef_Dairy_Cattle_Feeding_Activity_Coefficient_Provider()
        {
        }
        public IFeedingActivityCoeffientData GetByHousing(HousingType housingType)
        {
            switch (housingType)
            {
                case HousingType.HousedInBarn:
                case HousingType.Confined:
                case HousingType.ConfinedNoBarn:
                {
                    return new Table_17_Cattle_Feeding_Activity_Coefficient_Data()
                    {
                        FeedingActivityCoefficient = 0,
                    };
                }

                case HousingType.Pasture:
                case HousingType.FlatPasture:
                case HousingType.EnclosedPasture:
                {
                    return new Table_17_Cattle_Feeding_Activity_Coefficient_Data()
                    {
                        FeedingActivityCoefficient = 0.17,
                    };
                }

                case HousingType.OpenRangeOrHills:
                {
                    return new Table_17_Cattle_Feeding_Activity_Coefficient_Data()
                    {
                        FeedingActivityCoefficient = 0.36,
                    };
                }

                default:
                {
                    var defaultValue = new Table_17_Cattle_Feeding_Activity_Coefficient_Data()
                    {
                        FeedingActivityCoefficient = 0,
                    };
                    _log.Error($"{nameof(Table_17_Beef_Dairy_Cattle_Feeding_Activity_Coefficient_Provider)}.{nameof(Table_17_Beef_Dairy_Cattle_Feeding_Activity_Coefficient_Provider.GetByHousing)}" +
                        $" unable to get data for housing type: {housingType}." +
                        $" Returning default value of {defaultValue}.");
                    return defaultValue;
                }
            }
        }
    }
}