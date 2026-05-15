using H.Core.Enumerations;
using System.Diagnostics;
using NLog;

namespace H.Core.Providers.AnaerobicDigestion
{
    /// <summary>
    /// Default values for the fraction of volatile solids that biodegrades during digestate
    /// storage (post-digester, pre-field-application). Drives the storage-stage CH₄ loss
    /// estimate — biodegradation during storage represents methane that's vented to atmosphere
    /// before the digestate gets applied to a field, and it directly reduces the C input the
    /// field-level soil-carbon pipeline receives.
    ///
    /// <para>
    /// Consumed by <see cref="H.Core.Calculators.Infrastructure.ADCalculator"/> alongside
    /// <see cref="EmissionFactorsForDigestateStorageProvider"/> when building the per-day
    /// storage-stage emission rows.
    /// </para>
    /// </summary>
    public class BiodegradedFractionDuringStorageProvider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Fields
        #endregion

        #region Constructors
        public BiodegradedFractionDuringStorageProvider() { }

        #endregion

        #region Properties
        #endregion

        #region Public Methods

        /// <summary>
        /// Method takes in a <see cref="DigestateParameters"/> and a <see cref="DigestateState"/> and returns an instance of <see cref="BiodegradedFractionDuringStorageData"/>.
        /// The returning instance contains a "No Cover" and "Covered" fraction representing the Biodegraded fraction during storage.
        /// </summary>
        /// <param name="parameter">The digestate parameter for which biodegraded fraction is needed.</param>
        /// <param name="state">The digestate state, which is either Raw, Liquid or Solid.</param>
        /// <returns>Returns an instance of BiodegradedFractionDuringStorageData. If nothing found returns an empty instance.</returns>
        public BiodegradedFractionDuringStorageData GetBiodegradedFractionData(DigestateParameters parameter, DigestateState state)
        {
            if (parameter == DigestateParameters.TotalSolids)
            {
                if (state == DigestateState.Raw)
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.05,
                        Cover = 0.05
                    };
                }
                else if (state == DigestateState.LiquidPhase)
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.08,
                        Cover = 0.08
                    };
                }
                else
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.22,
                        Cover = 0.30
                    };
                }
            }
            else if (parameter == DigestateParameters.VolatileSolids)
            {
                if (state == DigestateState.Raw)
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.07,
                        Cover = 0.07
                    };
                }
                else if (state == DigestateState.LiquidPhase)
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.13,
                        Cover = 0.13
                    };
                }
                else
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.30,
                        Cover = 0.30
                    };
                }
            }
            else if (parameter == DigestateParameters.TotalCarbon)
            {
                if (state == DigestateState.Raw)
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.08,
                        Cover = 0.08
                    };
                }
                else if (state == DigestateState.LiquidPhase)
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.15,
                        Cover = 0.15
                    };
                }
                else
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.34,
                        Cover = 0.34
                    };
                }
            }
            else if (parameter == DigestateParameters.OrganicNitrogen)
            {
                if (state == DigestateState.Raw)
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0,
                        Cover = 0
                    };
                }
                else if (state == DigestateState.LiquidPhase)
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0,
                        Cover = 0
                    };
                }
                else
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.09,
                        Cover = 0.09
                    };
                }
            }
            else if (parameter == DigestateParameters.TotalAmmoniaNitrogen)
            {
                if (state == DigestateState.Raw)
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.36,
                        Cover = 0.07
                    };
                }
                else if (state == DigestateState.LiquidPhase)
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.44,
                        Cover = 0.09
                    };
                }
                else
                {
                    return new BiodegradedFractionDuringStorageData
                    {
                        NoCover = 0.51,
                        Cover = 0.51
                    };
                }
            }
            else
            {
                _log.Error($"{nameof(BiodegradedFractionDuringStorageProvider)}.{nameof(BiodegradedFractionDuringStorageProvider.GetBiodegradedFractionData)} " +
                    $"could not find DigestateParameters: {parameter}. Returning an empty instance of BiodegradedFractionDuringStorageData");
                return new BiodegradedFractionDuringStorageData();
            }
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
