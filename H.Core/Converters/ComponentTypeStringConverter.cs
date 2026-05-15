using System.Diagnostics;
using H.Core.Models;
using NLog;

namespace H.Core.Converters
{
    public class ComponentTypeStringConverter : ConverterBase
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public ComponentType Convert(string input)
        {
            var cleanedInput = base.GetLettersAsLowerCase(input);
            switch (cleanedInput)
            {
                case "backgrounding":
                    return ComponentType.Backgrounding;

                case "growertofinish":
                    return ComponentType.SwineGrowers;

                case "isowean":
                    return ComponentType.IsoWean;

                case "farrowtowean":
                    return ComponentType.FarrowToWean;

                case "farrowtofinish":
                    return ComponentType.FarrowToFinish;

                default:
                {
                    _log.Error($"{nameof(ComponentTypeStringConverter)}.{nameof(ComponentTypeStringConverter.Convert)}: unknown component type {input}. Returning {ComponentType.Backgrounding}");

                    return ComponentType.Backgrounding;
                }
            }
        }
    }
}