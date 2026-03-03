using System;
using System.ComponentModel;
using H.Core.Services.StorageService;
using H.Core;
using H.Core.Enumerations;

namespace H.Avalonia.ViewModels.OptionsViews.DataTransferObjects
{
    public class SoilN2OBreakdownSettingsDTO : ViewModelBase
    {
        #region Fields

        private MonthlyValueBase<double> _monthlyValues = new MonthlyValueBase<double>();

        #endregion

        #region Constructors
        public SoilN2OBreakdownSettingsDTO(IStorageService storageService) : base(storageService)
        {
            this.InitializeSoilN2OBreakdownSettings();
            MonthlyValues.PropertyChanged += ValidateTotalLessThan100;
        }
        #endregion

        #region Properties

        ///Wrapper properties for validating and setting values

        public MonthlyValueBase<double> MonthlyValues
        {
            get => _monthlyValues;
            set => SetProperty(ref _monthlyValues, value);
        }

        public double January
        {
            get => this.MonthlyValues.January;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.January = value;
                RaisePropertyChanged(nameof(January));
            }
        }
        public double February
        {
            get => this.MonthlyValues.February;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.February = value;
                RaisePropertyChanged(nameof(February));
            }
        }
        public double March
        {
            get => this.MonthlyValues.March;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.March = value;
                RaisePropertyChanged(nameof(March));
            }
        }
        public double April
        {
            get => this.MonthlyValues.April;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.April = value;
                RaisePropertyChanged(nameof(April));
            }
        }
        public double May
        {
            get => this.MonthlyValues.May;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.May = value;
                RaisePropertyChanged(nameof(May));
            }
        }
        public double June
        {
            get => this.MonthlyValues.June;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.June = value;
                RaisePropertyChanged(nameof(June));
            }
        }
        public double July
        {
            get => this.MonthlyValues.July;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.July = value;
                RaisePropertyChanged(nameof(July));
            }
        }
        public double August
        {
            get => this.MonthlyValues.August;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.August = value;
                RaisePropertyChanged(nameof(August));

            }
        }
        public double September
        {
            get => this.MonthlyValues.September;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.September = value;
                RaisePropertyChanged(nameof(September));

            }
        }
        public double October
        {
            get => this.MonthlyValues.October;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.October = value;
                RaisePropertyChanged(nameof(October));

            }
        }
        public double November
        {
            get => this.MonthlyValues.November;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.November = value;
                RaisePropertyChanged(nameof(November));
            }
        }
        public double December
        {
            get => this.MonthlyValues.December;
            set
            {
                if (HasErrors)
                {
                    return;
                }
                this.MonthlyValues.December = value;
                RaisePropertyChanged(nameof(December));

            }
        }
        #endregion

        #region Private Methods

        private void InitializeSoilN2OBreakdownSettings()
        {
            if (ActiveFarm?.AnnualSoilN2OBreakdown is not null)
            {
                this.MonthlyValues = ActiveFarm!.AnnualSoilN2OBreakdown;
            }
            else
            {
                throw new ArgumentNullException(nameof(MonthlyValues));
            }
        }

        #endregion

        #region Methods

        private void ValidateTotalLessThan100(object? sender, PropertyChangedEventArgs e)
        {
            double total = 0;

            foreach (Months month in Enum.GetValues(typeof(Months)))
            {
                total += this.MonthlyValues.GetValueByMonth(month);
            }
            if (total == 100.00)
            {
                if (e.PropertyName != null) RemoveError(e.PropertyName);
            }
        }
        #endregion
    }
}
