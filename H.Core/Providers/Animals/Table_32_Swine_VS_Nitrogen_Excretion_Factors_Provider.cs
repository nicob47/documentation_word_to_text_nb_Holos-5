using H.Core.Enumerations;
using System.Diagnostics;
using H.Infrastructure;
using NLog;

namespace H.Core.Providers.Animals
{
    /// <summary>
    /// Table 32. Volatile solid and nitrogen excretion adjustment factors for swine, by diet.
    /// <para>Greenhouse Gas System Pork Protocol 2006. Note: the VS and N adjustment factors used in Holos for all diets are currently set to 1, as we work to update the linkages between these factors and the Holos default swine diets</para>
    /// </summary>
    public class Table_32_Swine_VS_Nitrogen_Excretion_Factors_Provider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<string, DefaultVSAndNitrogenExcretionAdjustmentFactorsForDietsData> _cache = new Dictionary<string, DefaultVSAndNitrogenExcretionAdjustmentFactorsForDietsData>();

        public Table_32_Swine_VS_Nitrogen_Excretion_Factors_Provider()
        {
            _cache.Add(DietType.Standard.GetDescription().ToUpperInvariant(), new DefaultVSAndNitrogenExcretionAdjustmentFactorsForDietsData()
            {
                Name = DietType.Standard.GetDescription(),
                VSAdjustment = 1,
                NitrogenExcretedAdjustment = 1

            });

            _cache.Add(DietType.ReducedProtein.GetDescription().ToUpperInvariant(), new DefaultVSAndNitrogenExcretionAdjustmentFactorsForDietsData()
            {
                Name = DietType.ReducedProtein.GetDescription(),
                VSAdjustment = 0.99,
                NitrogenExcretedAdjustment = 0.7
            });

            _cache.Add(DietType.HighlyDigestibleFeed.GetDescription().ToUpperInvariant(), new DefaultVSAndNitrogenExcretionAdjustmentFactorsForDietsData()
            {
                Name = DietType.HighlyDigestibleFeed.GetDescription(),
                VSAdjustment = 0.95,
                NitrogenExcretedAdjustment = 0.95
            });
        }

        public DefaultVSAndNitrogenExcretionAdjustmentFactorsForDietsData GetByDietType(string dietName)
        {
            if (_cache.ContainsKey(dietName.ToUpperInvariant()))
            {
                return _cache[dietName.ToUpperInvariant()];
            }
            else
            {
                var defaultValue = _cache.First().Value;

                _log.Error($"{nameof(Table_32_Swine_VS_Nitrogen_Excretion_Factors_Provider)}.{nameof(GetByDietType)}: unknown diet type, returning default value {defaultValue}");

                return defaultValue;
            }
        }
    }
}
