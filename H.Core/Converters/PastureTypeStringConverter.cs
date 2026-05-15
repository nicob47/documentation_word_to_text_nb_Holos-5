using System.Diagnostics;
using H.Core.Enumerations;
using NLog;

namespace H.Core.Converters
{
    public class PastureTypeStringConverter : ConverterBase
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public PastureType Convert (string input)
        {
            switch (GetLettersAsLowerCase(input))
            {
                case "pasturegrasshighquality":
                    return PastureType.PastureGrassHigh;

                case "pasturegrassmediumquality":
                    return PastureType.PastureGrassMedium;

                case "pasturegrasslowquality":
                    return PastureType.PastureGrassLow;

                default:
                    _log.Error($"{nameof(PastureTypeStringConverter)}.{nameof(Convert)}: could not parse " +
                                     $"string input: {input}. Returning {nameof(PastureType)}.{nameof(PastureType.None)}");
                    return PastureType.None;
            }
        }
    }
}
