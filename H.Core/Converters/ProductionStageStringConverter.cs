using System.Diagnostics;
using H.Core.Enumerations;
using H.Infrastructure;
using NLog;

namespace H.Core.Converters
{
    public class ProductionStageStringConverter : ConverterBase
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public ProductionStages Convert(string input)
        {
            var cleanedInput = base.GetLettersAsLowerCase(input);
            switch (cleanedInput)
            {
                case "gestating":
                    return ProductionStages.Gestating;

                case "lactating":
                    return ProductionStages.Lactating;

                case "open":
                    return ProductionStages.Open;

                case "weaning":
                    return ProductionStages.Weaning;

                case "growingandfinishing":
                    return ProductionStages.GrowingAndFinishing;

                case "breedingstock":
                    return ProductionStages.BreedingStock;

                case "weaned":
                    return ProductionStages.Weaned;

                default:
                {
                        ProductionStages notFound = ProductionStages.Gestating;

                    _log.Error($"{nameof(ProductionStageStringConverter)}.{nameof(ProductionStageStringConverter.Convert)}: unknown production stage '{input}'. Returning {notFound.GetDescription()}");

                    return notFound;
                }
            }
        }
    }
}