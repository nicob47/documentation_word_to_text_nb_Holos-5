using H.Core.Enumerations;
using System.Diagnostics;
using NLog;

namespace H.Core.Providers.AnaerobicDigestion
{
    /// <summary>
    /// Table 47 — solid-liquid separation coefficients for digestate. After raw digestate is
    /// produced, on-farm separators (screw press, decanter centrifuge, etc.) split it into a
    /// solid fraction and a liquid fraction. Each property (dry matter, total N, TAN, total
    /// C, etc.) partitions differently — this table provides the default fraction that ends
    /// up in the <i>solid</i> fraction; the remainder goes to the liquid fraction.
    ///
    /// <para>
    /// <see cref="H.Core.Calculators.Infrastructure.ADCalculator"/> uses these coefficients to
    /// split the raw-digestate daily totals into the separate raw / solid / liquid tanks that
    /// <see cref="H.Core.Models.DigestateTank"/> tracks. Source: Guilayn et al. (2019).
    /// </para>
    /// </summary>
    public class Table_47_Solid_Liquid_Separation_Coefficients_Provider
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Fields
        #endregion

        #region Constructors

        public Table_47_Solid_Liquid_Separation_Coefficients_Provider() { }

        #endregion

        #region Properties
        #endregion

        #region Public Methods

        /// <summary>
        /// Takes a <see cref="DigestateParameters"/> enumeration and returns an instance of <see cref="SolidLiquidSeparationCoefficientsData"/>
        /// containing default values for Centrifuge and Belt Press.
        /// </summary>
        /// <param name="coefficient"></param>
        /// <returns>An instance of <see cref="SolidLiquidSeparationCoefficientsData"/> containing default values for Centrifuge and Belt Press.
        /// In case of wrong parameter, returns an empty instance.</returns>
        public SolidLiquidSeparationCoefficientsData GetSolidLiquidSeparationCoefficientInstance(DigestateParameters coefficient)
        {
            switch (coefficient)
            {
                case DigestateParameters.RawMaterial:
                    return new SolidLiquidSeparationCoefficientsData 
                    {
                        SeparationCoefficient = SeparationCoefficients.FractionRawMaterials,
                        Centrifuge = 0.29,
                        BeltPress = 0.10,
                    };

                case DigestateParameters.TotalSolids:
                    return new SolidLiquidSeparationCoefficientsData
                    {
                        SeparationCoefficient = SeparationCoefficients.FractionTotalSolids,
                        Centrifuge = 0.81,
                        BeltPress = 0.32,
                    };

                case DigestateParameters.VolatileSolids:
                    return new SolidLiquidSeparationCoefficientsData
                    {
                        SeparationCoefficient = SeparationCoefficients.FractionVolatileSolids,
                        Centrifuge = 0.83,
                        BeltPress = 0.38,
                    };

                case DigestateParameters.TotalAmmoniaNitrogen:
                    return new SolidLiquidSeparationCoefficientsData
                    {
                        SeparationCoefficient = SeparationCoefficients.FractionTotalAmmoniumNitrogen,
                        Centrifuge = 0.29,
                        BeltPress = 0.10,
                    };

                case DigestateParameters.OrganicNitrogen:
                    return new SolidLiquidSeparationCoefficientsData
                    {
                        SeparationCoefficient = SeparationCoefficients.OrganicNitrogen,
                        Centrifuge = 0.78,
                        BeltPress = 0.19,
                    };

                case DigestateParameters.TotalCarbon:
                    return new SolidLiquidSeparationCoefficientsData
                    {
                        SeparationCoefficient = SeparationCoefficients.FractionCarbon,
                        Centrifuge = 0.81,
                        BeltPress = 0.32,
                    };

                default:
                    {
                        _log.Error($"{nameof(Table_47_Solid_Liquid_Separation_Coefficients_Provider)}.{nameof(Table_47_Solid_Liquid_Separation_Coefficients_Provider.GetSolidLiquidSeparationCoefficientInstance)} " +
                            $"invalid DigestateParameters specified : {coefficient}. Returning an empty instance of SolidLiquidSeparationCoefficientsData.");
                        return new SolidLiquidSeparationCoefficientsData();
                    }
            }
        }

        #endregion

        #region Private Methods
        #endregion
    }
}
