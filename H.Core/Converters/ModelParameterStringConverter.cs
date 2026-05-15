using System.Diagnostics;
using H.Core.Enumerations;
using NLog;

namespace H.Core.Converters
{
    public class ModelParameterStringConverter : ConverterBase
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public ModelParameters Convert(string input)
        {
            switch(this.GetLowerCase(input))
            {
                case "tillfac":
                    return ModelParameters.TillageModifier;

                case "ws":
                    return ModelParameters.SlopeParameter;

                case "kfaca":
                    return ModelParameters.DecayRateActive;

                case "kfacs":
                    return ModelParameters.DecayRateSlow;

                case "kfacp":
                    return ModelParameters.DecayRatePassive;

                case "f1":
                    return ModelParameters.FractionMetabolicDMActivePool;

                case "f2":
                    return ModelParameters.FractionStructuralDMActivePool;

                case "f3":
                    return ModelParameters.FractionStructuralDMSlowPool;

                case "f5":
                    return ModelParameters.FractionActiveDecayToPassive;

                case "f6":
                    return ModelParameters.FractionSlowDecayToPassive;

                case "f7":
                    return ModelParameters.FractionSlowDecayToActive;

                case "f8":
                    return ModelParameters.FractionPassiveDecayToActive;

                case "topt":
                    return ModelParameters.OptimumTemperature;

                case "tmax":
                    return ModelParameters.MaximumAvgTemperature;
    
                default:
                    _log.Error($"{nameof(ModelParameterStringConverter)}.{nameof(ModelParameterStringConverter.Convert)}: unknown model parameter: {input}");
                    return ModelParameters.TillageModifier;
            }
        }
    }
}
