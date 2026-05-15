using System.Diagnostics;
using H.Core.Converters;
using H.Core.Enumerations;
using H.Infrastructure;
using H.Content;
using NLog;


namespace H.Core.Providers.AnaerobicDigestion
{
    /// <summary>
    /// Per-(digestate state, emission type) lookup for storage-stage emission factors. Tells
    /// the AD calculator how much CH₄ / N₂O / NH₃ leaves the digestate storage tank per kg
    /// of stored substrate.
    ///
    /// <para><b>Lookup key:</b></para>
    /// <list type="bullet">
    ///   <item><see cref="DigestateState"/> — raw vs solid vs liquid (each has different surface area + dry matter, so different emission profiles).</item>
    ///   <item><see cref="EmissionType"/> — CH₄, N₂O, or NH₃.</item>
    /// </list>
    ///
    /// <para>
    /// Pairs with <see cref="BiodegradedFractionDuringStorageProvider"/>: the biodegraded
    /// fraction drives substrate loss, this provider scales the emissions that result from
    /// that loss.
    /// </para>
    /// </summary>
    public class EmissionFactorsForDigestateStorageProvider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly DigestateStateStringConverter _digestateStateStringConverter;
        private readonly EmissionTypeStringConverter _emissionTypeStringConverter;

        #endregion

        #region Constructors

        public EmissionFactorsForDigestateStorageProvider()
        {
            _digestateStateStringConverter = new DigestateStateStringConverter();
            _emissionTypeStringConverter = new EmissionTypeStringConverter();

            this.Data = this.ReadFile();
        }

        #endregion

        #region Properties

        private List<EmissionFactorsForDigestateStorageData> Data { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Takes an emission type and emission origin (digestate state) and returns a single instance of <see cref="EmissionFactorsForDigestateStorageData"/> corresponding to the parameter.
        /// </summary>
        /// <param name="emissionType">The type of emission from the digestate during storage.</param>
        /// <param name="emissionOrigin">The state of the digestate during storage e.g. Raw, Liquid or Solid.</param>
        /// <returns>Returns a single instance of <see cref="EmissionFactorsForDigestateStorageData"/> . If nothing found, returns an empty instance.</returns>
        public EmissionFactorsForDigestateStorageData GetEmissionFactorInstance(EmissionTypes emissionType, DigestateState emissionOrigin)
        {
            EmissionFactorsForDigestateStorageData? data = this.Data.Find(x => (x.EmissionType == emissionType) && (x.EmissionOrigin == emissionOrigin));

            if (data != null)
            {
                return data;
            }

            data = this.Data.Find(x => x.EmissionType == emissionType);

            if (data != null)
            {
                _log.Error($"{nameof(EmissionFactorsForDigestateStorageProvider)}.{nameof(EmissionFactorsForDigestateStorageProvider.GetEmissionFactorInstance)}: " +
                    $"cannot find Emission Origin: {emissionOrigin}. Returning an empty instance of EmissionFactorsForDigestateStorageData.");
            }
            else
            {
                _log.Error($"{nameof(EmissionFactorsForDigestateStorageProvider)}.{nameof(EmissionFactorsForDigestateStorageProvider.GetEmissionFactorInstance)}: " +
                    $"cannot find Emission Type: {emissionType}. Returning an empty instance of EmissionFactorsForDigestateStorageData.");
            }

            return new EmissionFactorsForDigestateStorageData();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reads the csv file containing data for emission factors.
        /// </summary>
        /// <returns>Returns a list of <see cref="EmissionFactorsForDigestateStorageData"/>. Each entry in the list corresponds to a single row in the csv.</returns>
        private List<EmissionFactorsForDigestateStorageData> ReadFile()
        {
            var results = new List<EmissionFactorsForDigestateStorageData>();

            var cultureInfo = InfrastructureConstants.EnglishCultureInfo;
            IEnumerable<string[]> fileLines = CsvResourceReader.GetFileLines(CsvResourceNames.EmissionFactorsForDigestateStorage)!;

            foreach (string[] line in fileLines.Skip(1))
            {
                EmissionTypes emissionType = _emissionTypeStringConverter.Convert(line[0]);
                DigestateState emissionOrigin = _digestateStateStringConverter.Convert(line[1]);
                var emissionFactor = double.Parse(line[2], cultureInfo);
                var description = line[3];

                results.Add(new EmissionFactorsForDigestateStorageData
                {
                    EmissionType = emissionType,
                    EmissionOrigin = emissionOrigin,
                    EmissionFactor = emissionFactor,
                    Description = description,
                });
            }

            return results;
        }

        #endregion
    }
}
