using H.Core.Models.Results;

namespace H.Core.Models.LandManagement.Fields
{
    public partial class CropViewItem
    {
        #region Fields

        private double _totalCarbonLossFromBaleExports;
        private EstimatesOfProductionResultsViewItem _estimatesOfProductionResultsViewItem = null!;

        #endregion

        #region Properties

        /// <summary>
        /// Equation 12.3.2-4
        ///
        /// (kg C)
        /// </summary>
        public double TotalCarbonLossFromBaleExports
        {
            get => _totalCarbonLossFromBaleExports;
            set => SetProperty(ref _totalCarbonLossFromBaleExports, value);
        }

        public EstimatesOfProductionResultsViewItem EstimatesOfProductionResultsViewItem
        {
            get => _estimatesOfProductionResultsViewItem;
            set => SetProperty(ref _estimatesOfProductionResultsViewItem, value);
        }

        #endregion

        #region Public Methods

        public List<HarvestViewItem> GetHayHarvestsByYear(int year)
        {
            return this.HarvestViewItems.Where(x => x.Start.Year.Equals(year)).ToList();
        }

        public List<HarvestViewItem> GetHayHarvests()
        {
            return this.GetHayHarvestsByYear(this.Year);
        }

        #endregion
    }
}