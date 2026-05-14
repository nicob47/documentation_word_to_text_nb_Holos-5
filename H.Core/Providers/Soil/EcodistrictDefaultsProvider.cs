using System.Diagnostics;
using H.Content;
using H.Core.Converters;
using H.Core.Enumerations;
using H.Core.Tools;
using H.Infrastructure;

namespace H.Core.Providers.Soil
{
    /// <summary>
    /// Used to assist in the lookup of values in Table 1.
    /// </summary>
    public class EcodistrictDefaultsProvider
    {
        #region Fields

        private readonly EcozoneStringConverter _ecozoneStringConverter;
        private readonly ProvinceStringConverter _provinceStringConverter;
        private readonly SoilFunctionalCategoryStringConverter _soilFunctionalCategoryStringConverter;
        private readonly SoilTextureStringConverter _soilTextureStringConverter;

        // Suppress repeated Trace lines for ecodistrict ids that aren't in the defaults
        // table (e.g. id 705 hit thousands of times during a single analysis). Behaviour
        // is unchanged - we still fall back to the same default - this only collapses the
        // log volume to one entry per unique missing id.
        private static readonly HashSet<int> _warnedEcozoneMisses = new HashSet<int>();
        private static readonly HashSet<(int, Province)> _warnedFTopoMisses = new HashSet<(int, Province)>();
        private static readonly object _warnedLock = new object();

        #endregion

        #region Constructors

        public EcodistrictDefaultsProvider()
        {
            HTraceListener.AddTraceListener();

            _ecozoneStringConverter = new EcozoneStringConverter();
            _provinceStringConverter = new ProvinceStringConverter();
            _soilFunctionalCategoryStringConverter = new SoilFunctionalCategoryStringConverter();
            _soilTextureStringConverter = new SoilTextureStringConverter();

            this.Data = this.ReadFile();
        }

        #endregion

        #region Properties

        private List<EcodistrictDefaultsData> Data { get; set; }

        #endregion

        #region Public Methods       

        /// <summary>
        /// Multiple ecodistricts can exist in a single ecozone.
        /// </summary>
        public Ecozone GetEcozone(int ecodistrictId)
        {
            var result = this.Data.FirstOrDefault(x => x.EcodistrictId == ecodistrictId);
            if (result != null)
            {
                return result.Ecozone;
            }

            else
            {
                bool shouldWarn;
                lock (_warnedLock)
                {
                    shouldWarn = _warnedEcozoneMisses.Add(ecodistrictId);
                }

                if (shouldWarn)
                {
                    Trace.TraceError($"{nameof(EcodistrictDefaultsProvider)}.{nameof(EcodistrictDefaultsProvider.GetEcozone)} unable to get ecozone for ecodistrict: {ecodistrictId}. Returning default value of {Ecozone.AtlanticMaritimes.GetDescription()}. (Subsequent occurrences of this ecodistrict will be suppressed.)");
                }

                return Ecozone.AtlanticMaritimes;
            }
        }

        public double GetFractionOfLandOccupiedByPortionsOfLandscape(int ecodistrictId, Province province)
        {
            const double defaultValue = 0;

            var result = this.Data.FirstOrDefault(x => x.EcodistrictId == ecodistrictId && 
                                                       x.Province == province);
            if (result != null)
            {
                // Convert value to a fraction not a percentage (i.e. 0.20 not 20)
                return result.FTopo / 100;
            }
            else
            {
                bool shouldWarn;
                lock (_warnedLock)
                {
                    shouldWarn = _warnedFTopoMisses.Add((ecodistrictId, province));
                }

                if (shouldWarn)
                {
                    Trace.TraceError($"{nameof(EcodistrictDefaultsProvider)}.{nameof(EcodistrictDefaultsProvider.GetFractionOfLandOccupiedByPortionsOfLandscape)} unable to get FTopo value for ecodistrict: {ecodistrictId}, province: {province}. Returning default value of {defaultValue}. (Subsequent occurrences of this key will be suppressed.)");
                }

                return defaultValue;
            }
        }

        #endregion

        #region Private Methods

        private List<EcodistrictDefaultsData> ReadFile()
        {
            var results = new List<EcodistrictDefaultsData>();

            var cultureInfo = InfrastructureConstants.EnglishCultureInfo;
            var fileLines = CsvResourceReader.GetFileLines(CsvResourceNames.EcodistrictToEcozoneMapping)!;
            foreach (var line in fileLines.Skip(1))
            {
                var ecodistrictId = int.Parse(line[0], cultureInfo);
                var ecozone = _ecozoneStringConverter.Convert(line[1]);
                var province = _provinceStringConverter.Convert(line[2]);
                var pMayToOct = int.Parse(line[3], cultureInfo);
                var peMayToOct = int.Parse(line[4], cultureInfo);
                var fTopo = double.Parse(line[5], cultureInfo);
                var soilType = _soilFunctionalCategoryStringConverter.Convert(line[6]);
                var soilTexture = _soilTextureStringConverter.Convert(line[7]);

                results.Add(new EcodistrictDefaultsData()
                {
                    EcodistrictId = ecodistrictId,
                    Ecozone = ecozone,
                    Province = province,
                    PMayToOct = pMayToOct,
                    PEMayToOct = peMayToOct,
                    FTopo = fTopo,
                    SoilFunctionalCategory = soilType,
                    SoilTexture = soilTexture,
                });
            }

            return results;
        }

        #endregion
    }
}