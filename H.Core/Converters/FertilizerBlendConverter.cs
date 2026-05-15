using System.Diagnostics;
using H.Core.Enumerations;
using H.Infrastructure;
using NLog;

namespace H.Core.Converters
{
    public class FertilizerBlendConverter : ConverterBase
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        public FertilizerBlends Convert(string input)
        {
            var cleaned = base.GetLettersAsLowerCase(input);
            switch (cleaned)
            {
                case "urea":
                    return FertilizerBlends.Urea;

                case "ammonia":
                    return FertilizerBlends.Ammonia;

                case "ureaammoniumnitrate":
                    return FertilizerBlends.UreaAmmoniumNitrate;

                case "ammoniumnitrate":
                case "ammoniumnitrateprilled":
                    return FertilizerBlends.AmmoniumNitratePrilled;

                case "ammoniumnitrategranulated":
                    return FertilizerBlends.AmmoniumNitrateGranulated;

                case "calciumammoniumnitrate":
                    return FertilizerBlends.CalciumAmmoniumNitrate;

                case "ammoniumsulphate":
                    return FertilizerBlends.AmmoniumSulphate;

                case "mes":
                    return FertilizerBlends.MesS15;

                case "monoammoniumphosphate":
                    return FertilizerBlends.MonoAmmoniumPhosphate;

                case "diammoniumphosphate":
                    return FertilizerBlends.DiAmmoniumPhosphate;

                case "triplesuperphosphate":
                    return FertilizerBlends.TripleSuperPhosphate;

                case "superphosphate":
                    return FertilizerBlends.SuperPhosphate;

                case "potash":
                    return FertilizerBlends.Potash;

                case "potassiumsulphate":
                    return FertilizerBlends.PotassiumSulphate;

                case "npk":
                case "npkmixedacid":
                    return FertilizerBlends.NpkMixedAcid;

                case "npknitrophosphate":
                    return FertilizerBlends.NpkNitrophosphate;

                case "calciumnitrate":
                    return FertilizerBlends.CalciumNitrate;

                case "ammoniumnitrosulphate":
                    return FertilizerBlends.AmmoniumNitroSulphate;

                default:
                {
                    _log.Error($"{nameof(FertilizerBlendConverter)}.{nameof(FertilizerBlendConverter.Convert)}: unknown input '{cleaned}'. Returning default value of {FertilizerBlends.Urea.GetDescription()}");

                    return FertilizerBlends.Urea;
                }
            }
        }
    }
}