using System.Diagnostics;
using H.Core.Enumerations;
using NLog;

namespace H.Core.Converters
{
    public class EconomicsMeasurementStringConverter
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public EconomicMeasurementUnits Convert(string measurementString)
        {
            var lower = measurementString.ToLower();

            switch (lower)
            {
                case "lb":
                    return EconomicMeasurementUnits.Pound;
                case "na":
                    return EconomicMeasurementUnits.None;
                case "t":
                    return EconomicMeasurementUnits.Tonne;
                case "bu":
                    return EconomicMeasurementUnits.Bushel;
                case "cwt":
                    return EconomicMeasurementUnits.HundredWeight;
                default:
                    _log.Error($"{lower} is not a unit of measurement. Returning default 'none'");
                    return EconomicMeasurementUnits.None;
            }

        }
    }
}
