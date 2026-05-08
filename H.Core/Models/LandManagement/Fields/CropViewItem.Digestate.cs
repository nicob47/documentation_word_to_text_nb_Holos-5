using System.Collections.ObjectModel;
using System.Collections.Specialized;
using H.Core.Enumerations;
using H.Core.Providers.Climate;

namespace H.Core.Models.LandManagement.Fields
{
    public partial class CropViewItem
    {
        #region Fields

        private ObservableCollection<DigestateApplicationViewItem> _digestateApplicationViewItems = null!;

        private double _digestateCarbonInputsPerHectare;
        private double _digestateCarbonInputsPerHectareFromApplicationsOnly;

        private BiogasAndMethaneProductionParametersData _biogasAndMethaneProductionParametersData = null!;

        #endregion

        #region Properties

        /// <summary>
        /// (kg C ha^-1)
        ///
        /// Total digestate C from all digestate applications
        /// </summary>
        public double DigestateCarbonInputsPerHectare
        {
            get { return _digestateCarbonInputsPerHectare;}
            set { SetProperty(ref _digestateCarbonInputsPerHectare, value); }
        }

        /// <summary>
        /// (kg C ha^-1)
        ///
        /// Total digestate C from digestate applications only. Does not include remaining amounts.
        /// </summary>
        public double DigestateCarbonInputsPerHectareFromApplicationsOnly
        {
            get { return _digestateCarbonInputsPerHectareFromApplicationsOnly; }
            set { SetProperty(ref _digestateCarbonInputsPerHectareFromApplicationsOnly, value); }
        }

        public ObservableCollection<DigestateApplicationViewItem> DigestateApplicationViewItems
        {
            get => _digestateApplicationViewItems;
            set => SetProperty(ref _digestateApplicationViewItems, value);
        }

        public bool HasDigestateApplications { get; set; }

        public BiogasAndMethaneProductionParametersData BiogasAndMethaneProductionParametersData
        {
            get => _biogasAndMethaneProductionParametersData;
            set => SetProperty(ref _biogasAndMethaneProductionParametersData, value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// (kg C year^-1)
        /// </summary>
        public double GetTotalCarbonFromAppliedDigestate(ManureLocationSourceType location)
        {
            var digestateApplications = this.GetDigestateApplicationsInYear(location);
            return this.CalculateTotalCarbonFromDigestateApplications(digestateApplications);
        }

        /// <summary>
        /// Equation 4.7.1-1
        /// Equation 4.7.1-2
        ///
        /// (kg C year^-1)
        /// </summary>
        public double CalculateTotalCarbonFromDigestateApplications(IEnumerable<DigestateApplicationViewItem> applications)
        {
            var result = 0d;

            foreach (var application in applications)
            {
                result += application.AmountOfCarbonAppliedPerHectare * this.Area;
            }

            return result;
        }

        public IEnumerable<DigestateApplicationViewItem> GetDigestateApplicationsInYear(ManureLocationSourceType locationSourceType)
        {
            return this.DigestateApplicationViewItems.Where(d =>
                d.ManureLocationSourceType == locationSourceType &&
                d.DateCreated.Year == this.Year);
        }

        public IEnumerable<DigestateApplicationViewItem> GetDigestateApplicationViewItems(
            DateTime date,
            DigestateState state,
            ManureLocationSourceType source)
        {
            return this.DigestateApplicationViewItems.Where(x =>
                x.DateCreated.Date == date &&
                x.DigestateState == state &&
                x.ManureLocationSourceType == source);
        }

        public double GetRemainingNitrogenFromDigestateAtEndOfYear()
        {
            var itemsByYear = this.DigestateApplicationViewItems.Where(x => x.DateCreated.Year == this.Year);
            if (itemsByYear.Any())
            {
                // All digestate view items have the amount of digestate remaining at end of year when detail view items are created. Since all items from
                // the same year will have the same value for amount remaining, we return the amount from the first item.
                var firstItem = itemsByYear.First();
                var amount = itemsByYear.First().AmountOfNitrogenRemainingAtEndOfYear;
                if (amount >= 0)
                {
                    return amount;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }

        #endregion

        #region Event Handlers

        private void DigestateApplicationViewItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            this.HasDigestateApplications = this.DigestateApplicationViewItems.Count > 0;
            this.RaisePropertyChanged(nameof(this.HasDigestateApplications));
        }

        #endregion
    }
}