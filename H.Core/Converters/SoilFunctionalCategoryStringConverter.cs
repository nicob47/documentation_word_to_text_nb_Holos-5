using H.Core.Enumerations;
using System.Diagnostics;
using NLog;

namespace H.Core.Converters
{
    public class SoilFunctionalCategoryStringConverter : ConverterBase
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public SoilFunctionalCategory Convert(string input)
        {
            switch (this.GetLettersAsLowerCase(input))
            {
                case "brownchernozem":
                    return SoilFunctionalCategory.BrownChernozem;
                case "darkbrownchernozem":
                    return SoilFunctionalCategory.DarkBrownChernozem;
                case "blackgraychernozem":
                    return SoilFunctionalCategory.BlackGrayChernozem;
                case "all":
                    return SoilFunctionalCategory.All;
                case "brown":
                    return SoilFunctionalCategory.Brown;
                case "darkbrown":
                    return SoilFunctionalCategory.DarkBrown;
                case "black":
                    return SoilFunctionalCategory.Black;
                case "organic":
                    return SoilFunctionalCategory.Organic;
                case "easterncanada":
                case "east":
                    return SoilFunctionalCategory.EasternCanada;
                default:
                    {
                        _log.Error($"{nameof(SoilFunctionalCategoryStringConverter)}: Soil functional category '{input}' not mapped, returning default value.");

                        return SoilFunctionalCategory.NotApplicable;
                    }
            }

        }
    }
}
