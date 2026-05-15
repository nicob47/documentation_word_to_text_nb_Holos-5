using H.Core.Enumerations;
using System.Diagnostics;
using NLog;

namespace H.Core.Converters
{
    public class BeddingMaterialTypeStringConverter : ConverterBase
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public BeddingMaterialType Convert(string input)
        {
            switch(this.GetLettersAsLowerCase(input))
            {
                case "straw":
                    return BeddingMaterialType.Straw;
                case "woodchip":
                    return BeddingMaterialType.WoodChip;
                case "separatedmanuresolid":
                    return BeddingMaterialType.SeparatedManureSolid;
                case "sand":
                    return BeddingMaterialType.Sand;
                case "strawlong":
                    return BeddingMaterialType.StrawLong;
                case "strawchopped":
                    return BeddingMaterialType.StrawChopped;
                case "shavings":
                    return BeddingMaterialType.Shavings;
                case "sawdust":
                    return BeddingMaterialType.Sawdust;
                case "none":
                    return BeddingMaterialType.None;

                default:
                {
                    _log.Error($"{nameof(BeddingMaterialTypeStringConverter)}.{nameof(BeddingMaterialTypeStringConverter.Convert)} cannot find the given bedding material {input}. Returning {BeddingMaterialType.None}");
                    return BeddingMaterialType.None;
                 }


            }
        }
    }
}
