#region Imports

using System.Diagnostics;
using H.Core.Providers.Polygon;
using H.Core.Providers.Soil;
using NLog;

#endregion

namespace H.Core.Providers
{
    /// <summary>
    /// Provides soil data for a given location within Canada
    /// </summary>
    public class GeographicDataProvider : GeographicDataProviderBase, IGeographicDataProvider, IHolosMapPolygonIdListProvider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly ISoilDataProvider _soilDataProvider;

        #endregion

        #region Constructors

        public GeographicDataProvider()
        {
            _soilDataProvider = new NationalSoilDataBaseProvider();
        }

        #endregion

        #region Properties

        public void Initialize()
        {
            if (this.IsInitialized)
            {
                return;
            }

            _soilDataProvider.Initialize();

            this.IsInitialized = true;

            _log.Info($"{nameof(GeographicDataProvider)} has been initialized.");
        }

        public SoilData? GetPredominantSoilDataByPolygonId(int polygonId)
        {
            return _soilDataProvider.GetPredominantSoilDataByPolygonId(polygonId);
        }

        public IEnumerable<SoilData> GetAllSoilDataForAllComponentsWithinPolygon(int polygonId)
        {
            return _soilDataProvider.GetAllSoilDataForAllComponentsWithinPolygon(polygonId);
        }

        public List<int> GetPolygonIdList()
        {
            return _soilDataProvider.GetPolygonIdList();
        }

        public string GetEcodistrictName(int polygonId)
        {
            return _soilDataProvider.GetEcodistrictName(polygonId);
        }

        public bool DataExistsForPolygon(int polygonId)
        {
            return _soilDataProvider.DataExistsForPolygon(polygonId);
        }

        public GeographicData GetGeographicalData(int polygonId)
        {
            var result = new GeographicData();

            var predominantSoilDataByPolygonId = _soilDataProvider.GetPredominantSoilDataByPolygonId(polygonId);
            var allSoilData = _soilDataProvider.GetAllSoilDataForAllComponentsWithinPolygon(polygonId);

            result = new GeographicData
            {
                DefaultSoilData = predominantSoilDataByPolygonId ?? new SoilData(),
                SoilDataForAllComponentsWithinPolygon = allSoilData.ToList(),           
            };

            return result;            
        }      

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

        #region Event Handlers

        #endregion
    }
}