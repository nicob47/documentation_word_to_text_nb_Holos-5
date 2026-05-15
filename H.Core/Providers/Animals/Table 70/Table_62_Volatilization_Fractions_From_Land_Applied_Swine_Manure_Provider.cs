using H.Content;
using H.Core.Enumerations;
using H.Core.Providers.Animals.Table_69;
using H.Infrastructure;
using System.Diagnostics;
using NLog;

namespace H.Core.Providers.Animals.Table_70
{
    public class Table_62_Volatilization_Fractions_From_Land_Applied_Swine_Manure_Provider : Table_61_Volatilization_Fractions_From_Land_Applied_Dairy_Manure_Provider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Constructors

        public Table_62_Volatilization_Fractions_From_Land_Applied_Swine_Manure_Provider() : base()
        {
            base.ReadFile(CsvResourceNames.SwineFractionOfNAmmoniaLandAppliedManure);
        }

        #endregion

        #region Public Methods

        public override VolatilizationFractionsFromLandAppliedManureData GetData(AnimalType animalType, Province province, int year)
        {
            var notFound = new VolatilizationFractionsFromLandAppliedManureData();

            if (animalType.IsSwineType() == false)
            {
                _log.Error($"{nameof(Table_62_Volatilization_Fractions_From_Land_Applied_Swine_Manure_Provider)}.{nameof(GetData)}" +
                                 $" can only provide data for {AnimalType.Dairy.GetDescription()} animals.");

                return notFound;
            }

            if (_validProvinces.Contains(province) == false)
            {
                _log.Error($"{nameof(Table_62_Volatilization_Fractions_From_Land_Applied_Swine_Manure_Provider)}.{nameof(GetData)}" +
                                 $" unable to find province {province} in the available data.");

                return notFound;
            }

            var closestYear = MathHelpers.Closest(_data.Select(x => x.Key).ToArray(), year);

            return _data[closestYear][province];
        }

        #endregion
    }
}