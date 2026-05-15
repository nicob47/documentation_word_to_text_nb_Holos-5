using H.Core.Enumerations;
using System.Diagnostics;
using NLog;

namespace H.Core.Converters
{
    public class DigestateStateStringConverter : ConverterBase
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public DigestateState Convert(string input)
        {
            switch (this.GetLettersAsLowerCase(input))
            {
                case "raw":
                case "rawdigestate":
                    return DigestateState.Raw;

                case "liquid":
                case "liquidfraction":
                case "liquidphase":
                    return DigestateState.LiquidPhase;

                case "solid":
                case "solidfraction":
                case "solidphase":
                    return DigestateState.SolidPhase;

                default:
                    {
                        _log.Error($"{nameof(DigestateStateStringConverter)}.{nameof(DigestateStateStringConverter.Convert)} " +
                            $"unknown DigestateState type: {input}. Returning {DigestateState.Raw}.");
                        return DigestateState.Raw;
                    }
            }
        }
    }
}
