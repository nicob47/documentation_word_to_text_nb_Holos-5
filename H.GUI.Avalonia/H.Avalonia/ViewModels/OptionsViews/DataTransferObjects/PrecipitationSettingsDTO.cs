using System;
using H.Core.Providers.Precipitation;
using H.Core.Services.StorageService;

namespace H.Avalonia.ViewModels.OptionsViews.DataTransferObjects
{
    public class PrecipitationSettingsDTO : ViewModelBase
    {
        #region Fields

        private PrecipitationData _precipitationData = new PrecipitationData();

        #endregion

        #region Constructors
        public PrecipitationSettingsDTO(IStorageService storageService) : base(storageService)
        {
            this.InitializePrecipitationData();
        }

        #endregion

        #region Properties
        public PrecipitationData PrecipitationData
        {
            get => _precipitationData;
            set => SetProperty(ref _precipitationData, value);
        }

        public double January
        {
            get => this.PrecipitationData.January;
            set
            {
                ValidateValue(value, nameof(January));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.January = value;
                RaisePropertyChanged(nameof(January));
            }
        }

        public double February
        {
            get => this.PrecipitationData.February;
            set
            {
                ValidateValue(value, nameof(February));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.February = value;
                RaisePropertyChanged(nameof(February));
            }
        }
        public double March
        {
            get => this.PrecipitationData.March;
            set
            {
                ValidateValue(value, nameof(March));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.March = value;
                RaisePropertyChanged(nameof(March));
            }
        }
        public double April
        {
            get => this.PrecipitationData.April;
            set
            {
                ValidateValue(value, nameof(April));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.April = value;
                RaisePropertyChanged(nameof(April));
            }
        }
        public double May
        {
            get => this.PrecipitationData.May;
            set
            {
                ValidateValue(value, nameof(May));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.May = value;
                RaisePropertyChanged(nameof(May));
            }
        }
        public double June
        {
            get => this.PrecipitationData.June;
            set
            {
                ValidateValue(value, nameof(June));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.June = value;
                RaisePropertyChanged(nameof(June));
            }
        }
        public double July
        {
            get => this.PrecipitationData.July;
            set
            {
                ValidateValue(value, nameof(July));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.July = value;
                RaisePropertyChanged(nameof(July));
            }
        }
        public double August
        {
            get => this.PrecipitationData.August;
            set
            {
                ValidateValue(value, nameof(August));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.August = value;
                RaisePropertyChanged(nameof(August));
            }
        }
        public double September
        {
            get => this.PrecipitationData.September;
            set
            {
                ValidateValue(value, nameof(September));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.September = value;
                RaisePropertyChanged(nameof(September));
            }
        }
        public double October
        {
            get => this.PrecipitationData.October;
            set
            {
                ValidateValue(value, nameof(October));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.October = value;
                RaisePropertyChanged(nameof(October));
            }
        }
        public double November
        {
            get => this.PrecipitationData.November;
            set
            {
                ValidateValue(value, nameof(November));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.November = value;
                RaisePropertyChanged(nameof(November));
            }
        }
        public double December
        {
            get => this.PrecipitationData.December;
            set
            {
                ValidateValue(value, nameof(December));
                if (HasErrors)
                {
                    return;
                }
                this.PrecipitationData.December = value;
                RaisePropertyChanged(nameof(December));
            }
        }

        #endregion

        #region Public Methods

        public void InitializePrecipitationData()
        {
            if (base.ActiveFarm?.ClimateData?.PrecipitationData is not null)
            {
                this.PrecipitationData = base.ActiveFarm!.ClimateData.PrecipitationData;
            }
            else
            {
                throw new ArgumentNullException(nameof(PrecipitationData));
            }
        }

        #endregion

        #region Private Methods

        private void ValidateValue(double value, string propertyName)
        {
            if (value < 0)
            {
                AddError(propertyName, H.Core.Properties.Resources.ErrorMustBeNonNegative);
            }
            else
            {
                RemoveError(propertyName);
            }
        }

        #endregion
    }
}
