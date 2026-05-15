using H.Core.Enumerations;

namespace H.Core.Models
{
    /// <summary>
    /// One "tank" of digestate — a running balance of stored raw / solid / liquid digestate
    /// plus the available N for land application. Populated by
    /// <see cref="H.Core.Services.Animals.DigestateService"/> from the underlying AD calculator's
    /// daily outputs.
    ///
    /// <para><b>Three digestate states:</b></para>
    /// <list type="bullet">
    ///   <item><b>Raw</b> — digester effluent before any solid-liquid separation.</item>
    ///   <item><b>Solid</b> — solids fraction after separation; lower N, higher C content.</item>
    ///   <item><b>Liquid</b> — liquid fraction after separation; higher N, lower dry-matter.</item>
    /// </list>
    /// Each tank tracks both the gross produced amount (before field applications drew anything
    /// down) and the net available amount (after drawdown). The two are kept distinct so audit
    /// / export views can see the gross production while the field-application paths read net
    /// availability.
    /// </summary>
    public class DigestateTank : StorageTankBase
    {
        #region Fields

        /// <summary>Raw / Solid / Liquid — see class doc.</summary>
        private DigestateState _digestateState;

        // Amount of digestate available after all field applications
        private double _totalRawDigestateAvailable;
        private double _totalSolidDigestateAvailable;
        private double _totalLiquidDigestateAvailable;

        // Amount of digestate produced by system (not reduced by field applications)
        private double _totalRawDigestateProduced;
        private double _totalSolidDigestateProduced;
        private double _totalLiquidDigestateProduced;

        /*
         * Nitrogen available for land application
         */

        /// <summary>
        /// (kg N)
        /// </summary>
        public double NitrogenFromRawDigestate { get; set; }

        /// <summary>
        /// (kg N)
        /// </summary>
        public double NitrogenFromSolidDigestate { get; set; }

        /// <summary>
        /// (kg N)
        /// </summary>
        public double NitrogenFromLiquidDigestate  { get; set; }

        /*
         * Carbon available for land application
         */

        /// <summary>
        /// (kg C)
        /// </summary>
        public double CarbonFromRawDigestate { get; set; }

        /// <summary>
        /// (kg C)
        /// </summary>
        public double CarbonFromSolidDigestate { get; set; }

        /// <summary>
        /// (kg C)
        /// </summary>
        public double CarbonFromLiquidDigestate { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// A tank of digestate can store whole (raw) digestate or separated digestate
        /// </summary>
        public DigestateState DigestateState
        {
            get => _digestateState;
            set => SetProperty(ref _digestateState, value);
        }

        /// <summary>
        /// Total amount of raw digestate available after all field applications have been considered
        ///  
        /// (kg)
        /// </summary>
        public double TotalRawDigestateAvailable
        {
            get => _totalRawDigestateAvailable;
            set => SetProperty(ref _totalRawDigestateAvailable, value);
        }

        /// <summary>
        /// (kg)
        /// </summary>
        public double TotalSolidDigestateAvailable
        {
            get => _totalSolidDigestateAvailable;
            set => SetProperty(ref _totalSolidDigestateAvailable, value);
        }

        /// <summary>
        /// (kg)
        /// </summary>
        public double TotalLiquidDigestateAvailable
        {
            get => _totalLiquidDigestateAvailable;
            set => SetProperty(ref _totalLiquidDigestateAvailable, value);
        }

        public double TotalRawDigestateProduced
        {
            get => _totalRawDigestateProduced;
            set => SetProperty(ref _totalRawDigestateProduced, value);
        }

        public double TotalSolidDigestateProduced
        {
            get => _totalSolidDigestateProduced;
            set => SetProperty(ref _totalSolidDigestateProduced, value);
        }

        public double TotalLiquidDigestateProduced
        {
            get => _totalLiquidDigestateProduced;
            set => SetProperty(ref _totalLiquidDigestateProduced, value);
        }

        #endregion

        #region Public Methods

        #endregion
    }
}