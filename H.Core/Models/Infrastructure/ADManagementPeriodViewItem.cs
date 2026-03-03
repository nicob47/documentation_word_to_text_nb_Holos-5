using H.Core.Models.Animals;

namespace H.Core.Models.Infrastructure
{
    public class ADManagementPeriodViewItem : SubstrateViewItemBase
    {
        #region Fields

        private bool _isSelected;

        #endregion

        #region Constructors

        public ADManagementPeriodViewItem()
        {
            this.DailyPercentageOfManureAdded = 100;
        }

        #endregion

        #region Properties

        public ManagementPeriod ManagementPeriod { get; set; } = null!;
        public AnimalComponentBase AnimalComponent { get; set; } = null!;
        public AnimalGroup AnimalGroup { get; set; } = null!;

        public string ManureStateTypeString { get; set; } = string.Empty;
        public string AnimalTypeString { get; set; } = string.Empty;
        public string ComponentName { get; set; } = string.Empty;

        public bool IsSelected
        {
            get { return _isSelected;}
            set { SetProperty(ref _isSelected, value); }
        }

        /// <summary>
        /// Percentage
        /// </summary>
        public double DailyPercentageOfManureAdded { get; set; }

        /// <summary>
        /// Fraction
        /// </summary>
        public double DailyFractionOfManureAdded 
        {
            get
            {
                return this.DailyPercentageOfManureAdded / 100.0;
            }
        }

        #endregion
    }
}