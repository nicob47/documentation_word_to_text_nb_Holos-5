using H.Core.Enumerations;
using H.Core.Services.StorageService;

namespace H.Avalonia.ViewModels.OptionsViews.DataTransferObjects
{
    public class FarmSettingsDTO : ViewModelBase
    {
        #region Fields
        private string _coordinates = string.Empty;

        private bool _isBasicMode;
        #endregion

        #region Constructors
        public FarmSettingsDTO(IStorageService storageService) : base(storageService)
        {
            if (ActiveFarm is not null)
            {
                Coordinates = $"{ActiveFarm.Latitude}, {ActiveFarm.Longitude}";
                _isBasicMode = ActiveFarm.IsBasicMode;
            }
        }
        #endregion

        #region Properties
        ///Wrapper properties for validating and setting values
        public string Coordinates
        {
            get => _coordinates;
            set => SetProperty(ref _coordinates, value);
        }
        public string FarmComments
        {
            get => ActiveFarm?.Comments ?? string.Empty;
            set
            {
                ValidateString(value, nameof(FarmComments));
                if (HasErrors)
                    {
                        return;
                    }
                    if (ActiveFarm is not null)
                    {
                        ActiveFarm.Comments = value;
                    }
                RaisePropertyChanged(nameof(FarmComments));

            }
        }
        public string FarmName
        {
            get => ActiveFarm?.Name ?? string.Empty;
            set
            {
                ValidateString(value, nameof(FarmName));
                    if (HasErrors)
                    {
                        return;
                    }
                    if (ActiveFarm is not null)
                    {
                        ActiveFarm.Name = value;
                    }
                    RaisePropertyChanged(nameof(FarmName));
            }
        }
        public double GrowingSeasonPrecipitation
        {
            get => ActiveFarm?.ClimateData.PrecipitationData.GrowingSeasonPrecipitation ?? 0;
            set
            {
                ValidateNonNegative(value, nameof(GrowingSeasonPrecipitation));
                    if (HasErrors)
                    {
                        return;
                    }
                    if (ActiveFarm is not null)
                    {
                        ActiveFarm.ClimateData.PrecipitationData.GrowingSeasonPrecipitation = value;
                    }
                    RaisePropertyChanged(nameof(GrowingSeasonPrecipitation));
            }
        }
        public double GrowingSeasonEvapotranspiration
        {
            get => ActiveFarm?.ClimateData.EvapotranspirationData.GrowingSeasonEvapotranspiration ?? 0;
            set
            {
                ValidateNonNegative(value, nameof(GrowingSeasonEvapotranspiration));
                    if (HasErrors)
                    {
                        return;
                    }
                    if (ActiveFarm is not null)
                    {
                        ActiveFarm.ClimateData.EvapotranspirationData.GrowingSeasonEvapotranspiration = value;
                    }
                    RaisePropertyChanged(nameof(GrowingSeasonEvapotranspiration));
            }
        }
        public int PolygonId
        {
            get => ActiveFarm?.PolygonId ?? 0;
        }
        public Province Province
        {
            get => ActiveFarm?.Province ?? Province.Alberta;
        }
        public string HardinessZoneString
        {
            get => ActiveFarm?.GeographicData.HardinessZoneString ?? string.Empty;
        }
        public bool IsBasicMode
        {
            get => _isBasicMode;
            set
            {
                if(SetProperty(ref _isBasicMode, value) && ActiveFarm is not null)
                {
                    ActiveFarm.IsBasicMode = value;
                }
            }
        }
        public bool IsAdvancedMode
        {
            get => this.IsBasicMode == false;
        }
        #endregion

        #region Private Methods
        ///Validation methods for properties
        private void ValidateString(string value, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                AddError(propertyName, H.Core.Properties.Resources.ErrorNameCannotBeEmpty);
            }
            else
            {
                RemoveError(propertyName);
            }

        }
        private void ValidateNonNegative(double value, string propertyName)
        {
            if (value < 0)
            {
                AddError(propertyName, H.Core.Properties.Resources.ErrorMustBeGreaterThan0);
            }
            else
            {
                RemoveError(propertyName);
            }
        }
        #endregion
    }
}
