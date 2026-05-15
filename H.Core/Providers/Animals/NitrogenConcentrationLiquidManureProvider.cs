using System.Diagnostics;
using H.Core.Enumerations;
using NLog;

namespace H.Core.Providers.Animals
{
    /// <summary>
    /// Table 40
    /// </summary>
    public class NitrogenConcentrationLiquidManureProvider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly List<NitrogenConcentrationOfLiquidManureTableData> _data;

        #endregion

        #region Constructors

        public NitrogenConcentrationLiquidManureProvider()
        {
            _data = new List<NitrogenConcentrationOfLiquidManureTableData>();

            _data.Add(new NitrogenConcentrationOfLiquidManureTableData()
            {
                AnimalType = AnimalType.Swine,
                NitrogenConcentration = 3.5
            });

            _data.Add(new NitrogenConcentrationOfLiquidManureTableData()
            {
                AnimalType = AnimalType.Dairy,
                NitrogenConcentration = 3.4
            });

            _data.Add(new NitrogenConcentrationOfLiquidManureTableData()
            {
                AnimalType = AnimalType.Poultry,
                NitrogenConcentration = 6.0
            });
        } 

        #endregion

        public NitrogenConcentrationOfLiquidManureTableData GetData(AnimalType animalType)
        {
            if (animalType.IsSwineType())
            {
                return _data.First(data => data.AnimalType == AnimalType.Swine);
            }

            if (animalType.IsDairyCattleType())
            {
                return _data.First(data => data.AnimalType == AnimalType.Dairy);
            }

            if (animalType.IsPoultryType())
            {
                return _data.First(data => data.AnimalType == AnimalType.Poultry);
            }

            var defaultData = _data.First(data => data.AnimalType == AnimalType.Swine);

            _log.Error($"{nameof(NitrogenConcentrationLiquidManureProvider)}.{nameof(GetData)}: unknow animal type '{animalType}'. Returning default of {defaultData}");

            return defaultData;
        }
    }
}