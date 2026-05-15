using H.Core.Enumerations;
using H.Infrastructure;
using NLog;

namespace H.Core.Providers.Animals
{
    /// <summary>
    /// Table 39. Crude protein content in feed, as fed (% of feed intake), by swine group.
    /// <para>Source: D. Beaulieu, U. of Saskatchewan, (pers. comm.)</para>
    /// </summary>
    public class Table_39_Crude_Protein_Content_Swine_Feed_Provider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Fields

        #endregion

        #region Constructors

        public Table_39_Crude_Protein_Content_Swine_Feed_Provider()
        {
        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public Dictionary<DietType, double> GetByProvince(Province province)
        {
            return new Dictionary<DietType, double>
            {
                {DietType.Gestation, 14.28},
                {DietType.Lactation, 19.07},
                {DietType.NurseryWeanersStarter1, 23.88},
                {DietType.NurseryWeanersStarter2, 21.45},
                {DietType.GrowerFinisherDiet1, 20.27},
                {DietType.GrowerFinisherDiet2, 19.89},
                {DietType.GrowerFinisherDiet3, 19.92},
                {DietType.GrowerFinisherDiet4, 19.66},
                {DietType.Boars, 20.1}
            };
        }

        public double GetCrudeProteinInFeedForSwineGroupByProvince(Province province, DietType dietType)
        {
            var byProvince = this.GetByProvince(province);
            if (byProvince.ContainsKey(dietType))
            {
                return byProvince[dietType];
            }
            else
            {
                _log.Error($"{nameof(Table_39_Crude_Protein_Content_Swine_Feed_Provider)}.{nameof(GetCrudeProteinInFeedForSwineGroupByProvince)}" +
                                                    $" unable to get data for province: {province} and diet type: {dietType.GetDescription()}" +
                                                    $" Returning default value of 0.");

                return 0;
            }
        }

        #endregion
    }
}


