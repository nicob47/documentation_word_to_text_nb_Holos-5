using System.Diagnostics;
using H.Content;
using H.Infrastructure;
using NLog;

namespace H.Core.Providers.Shelterbelt
{
    public static class ShelterbeltEcodistrictToClusterLookupProvider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Constructors

         static ShelterbeltEcodistrictToClusterLookupProvider()
        {
            var cultureInfo = InfrastructureConstants.EnglishCultureInfo;
            var filename = CsvResourceNames.LookupEcodistrictClusters;
            var fileLines = CsvResourceReader.GetFileLines(filename)!;

            foreach (var line in fileLines.Skip(1))
            {
                var entry = new EcodistrictToClusterData();

                entry.EcodistrictId = int.Parse(line[1], cultureInfo);
                entry.ClusterId = line[2];
                entry.SoilZone = line[3];

                Data.Add(entry);
            }
        }

        #endregion

        #region Properties

        private static List<EcodistrictToClusterData> Data { get; } = new List<EcodistrictToClusterData>();

        #endregion

        public static EcodistrictToClusterData GetClusterData(int ecodistrictId)
        {
            var clusterData = Data.SingleOrDefault(x => x.EcodistrictId == ecodistrictId);
            if (clusterData == null)
            {
                var defaultValue = new EcodistrictToClusterData();
                _log.Error($"{nameof(ShelterbeltEcodistrictToClusterLookupProvider)}.{nameof(GetClusterData)}" +
                    $" unable to get data for the ecodistrict id: {ecodistrictId}." +
                    $" Returning default value of {defaultValue}.");
                return defaultValue;
            }
            return clusterData;
        }

        public static bool CanLookupByEcodistrict(int ecodistrict)
        {
            var clusterData = GetClusterData(ecodistrict);
            if (string.IsNullOrWhiteSpace(clusterData.ClusterId))
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}