using System.Diagnostics;
using H.Core.Enumerations;
using H.Core.Models.LandManagement.Fields;
using NLog;

namespace H.Core.Providers.Soil
{
    /// <summary>
    /// Table 13 — Canadian soil N₂O emission factors adapted from Liang et al. (2020), broken
    /// down by N source, soil texture, tillage practice, and crop type. Holds the modifier
    /// factors that <see cref="H.Core.Calculators.Nitrogen.N2OEmissionFactorCalculator"/>
    /// multiplies together with the base emission factor to get the final per-context EF.
    ///
    /// <para><b>Factor abbreviations:</b></para>
    /// <list type="bullet">
    ///   <item><c>RF_NS</c> — reduction / modifier factor for the nitrogen source (synthetic vs organic vs residue).</item>
    ///   <item><c>RF_TX</c> — modifier for soil texture (fine / medium / coarse).</item>
    ///   <item><c>RF_TILL</c> — modifier for tillage practice (intensive / reduced / no-till).</item>
    ///   <item><c>TF_CS</c> — temperature factor by climate scenario.</item>
    ///   <item><c>RF_AM</c> — modifier for application method (broadcast / banded / injected).</item>
    /// </list>
    /// </summary>
    public class Table_13_Soil_N2O_Emission_Factors_Provider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Inner Classes

        /// <summary>
        /// Discriminator used by Table 13 lookups — different RF_NS values apply depending on
        /// whether the N input is synthetic fertilizer, organic (manure / digestate), or crop
        /// residue.
        /// </summary>
        public enum NitrogenSourceTypes
        {
            /// <summary>Synthetic fertilizer N (urea, anhydrous ammonia, etc.).</summary>
            SyntheticNitrogen,

            /// <summary>Organic N from manure or digestate applications.</summary>
            OrganicNitrogen,

            /// <summary>N returned via crop residues (straw, roots, extraroots) at harvest / senescence.</summary>
            CropResidueNitrogen,
        }

        #endregion

        #region Fields      

        #endregion

        #region Constructors

        public Table_13_Soil_N2O_Emission_Factors_Provider()
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Table 16: Lookup function for Nitrogen source = RF_NS values.
        /// <para>Note: a = A Soil N2O ratio factor for nitrogen source is only applied on annual crops</para>
        /// </summary>
        /// <param name="nitrogenSourceType"></param>
        /// <param name="cropViewItem"></param>
        /// <returns></returns>
        public double GetFactorForNitrogenSource(NitrogenSourceTypes nitrogenSourceType, CropViewItem cropViewItem)
        {
            // For all perennials systems, nitrogen source has no effect (RF = 1)
            if (cropViewItem.CropType.IsPerennial() || cropViewItem.CropType.IsGrassland())
            {
                return 1;
            }

            switch (nitrogenSourceType)
            {
                case NitrogenSourceTypes.CropResidueNitrogen:
                case NitrogenSourceTypes.OrganicNitrogen:
                    return 0.84;
                case NitrogenSourceTypes.SyntheticNitrogen:
                    return 1;
                default:
                    {
                        const double defaultValue = 1;

                        _log.Error($"{nameof(Table_13_Soil_N2O_Emission_Factors_Provider)}.{nameof(GetFactorForSoilTexture)}: unknown value for {nameof(nitrogenSourceType)}: {nitrogenSourceType}. Returning {defaultValue}");
                        return 1;
                    }
            }
        }

        /// <summary>
        /// Table 16: Lookup function for Cropping System = RF_CS values.
        /// </summary>
        /// <param name="cropType"></param>
        /// <returns></returns>
        public double GetFactorForCroppingSystem(CropType cropType)
        {
            if (cropType.IsAnnual())
            {
                return 1;
            }
            else
            {
                // Perennials
                return 0.19;
            }
        }

        /// <summary>
        /// Table 16: Lookup function for Soil Texture = RF_TX values.
        /// </summary>
        /// <param name="soilTexture"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        public double GetFactorForSoilTexture(SoilTexture soilTexture, Region region)
        {
            if (region == Region.WesternCanada)
            {
                // Note b: In the Prairies and the Peace River Section of BC, RF_TX = 1

                return 1;
            }

            if (soilTexture == SoilTexture.Fine)
            {
                return 2.55;
            }

            if (soilTexture == SoilTexture.Coarse || soilTexture == SoilTexture.Medium)
            {
                return 0.49;
            }

            const double defaultValue = 1;

            _log.Error($"{nameof(Table_13_Soil_N2O_Emission_Factors_Provider)}.{nameof(GetFactorForSoilTexture)}: unknown value for {nameof(soilTexture)}: {soilTexture}, and {nameof(region)}: {region}. Returning {defaultValue}");

            return defaultValue;
        }

        /// <summary>
        /// Table 16: Lookup function for Tillage Practice = RF_TILL values.
        /// </summary>
        /// <param name="region"></param>
        /// <param name="cropViewItem"></param>
        /// <returns></returns>
        public double GetFactorForTillagePractice(Region region, CropViewItem cropViewItem)
        {
            // For all perennials systems, tillage has no effect (RF = 1)
            if (cropViewItem.CropType.IsPerennial() || cropViewItem.CropType.IsGrassland())
            {
                return 1;
            }

            var tillageType = cropViewItem.TillageType;

            // Roland says:
            // Conservation tillage will be used for both reduced tillage and no-tillage
            // Conventional tillage will be used for intensive tillage

            if (region == Region.EasternCanada)
            {
                if (tillageType == TillageType.Reduced || tillageType == TillageType.NoTill)
                {
                    // Conservation tillage

                    return 1.05;
                }
                else
                {
                    // Conventional tillage (intensive)

                    return 1;
                }
            }

            if (region == Region.WesternCanada)
            {
                if (tillageType == TillageType.Reduced || tillageType == TillageType.NoTill)
                {
                    // Conservation tillage

                    return 0.73;
                }
                else
                {
                    // Conventional tillage (intensive)

                    return 1;
                }
            }

            const double defaultValue = 1;

            _log.Error($"{nameof(Table_13_Soil_N2O_Emission_Factors_Provider)}.{nameof(GetFactorForTillagePractice)}: unknown {nameof(region)}: {region}. Returning {defaultValue}");

            return defaultValue;
        }

        /// <summary>
        /// Table 16: Lookup function for N2O Reduction factor values = RF_AM
        /// </summary>
        public double GetReductionFactorBasedOnApplicationMethod(SoilReductionFactors soilReductionFactors)
        {
            var defaultValue = 1d;
            switch (soilReductionFactors)
            {
                case SoilReductionFactors.ControlledRelease:
                    return 0.86;

                case SoilReductionFactors.NitrificationInhibitor:
                    return 0.72;

                case SoilReductionFactors.UreaseInhibitor:
                    return 1.04;

                case SoilReductionFactors.NitrificationAndUreaseInhibitor:
                    return 0.69;

                case SoilReductionFactors.None:
                    return defaultValue;

                default:
                    _log.Error($"{nameof(Table_13_Soil_N2O_Emission_Factors_Provider)}.{nameof(GetReductionFactorBasedOnApplicationMethod)} " +
                                     $":unknown Soil Reduction Factor: {nameof(soilReductionFactors)}, returning {defaultValue}");
                    return defaultValue;
            }
        }

        #endregion
    }
}
