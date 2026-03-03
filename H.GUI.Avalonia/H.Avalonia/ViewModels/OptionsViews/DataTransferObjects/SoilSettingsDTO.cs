using System;
using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Providers.Soil;
using H.Core.Services.StorageService;

namespace H.Avalonia.ViewModels.OptionsViews.DataTransferObjects
{
    public class SoilSettingsDTO : ViewModelBase
    {
        #region Fields        
        private SoilData _bindingSoilData = new SoilData();
        #endregion

        #region Constructors
        public SoilSettingsDTO(IStorageService storageService) : base(storageService)
        {
            ManageData();
        }
        #endregion

        #region Properties
        public SoilData BindingSoilData
        {
            get => _bindingSoilData;
            set => SetProperty(ref _bindingSoilData, value);
        }

        //Wrapper properties for validating and setting values
        public double BulkDensity
        {
            get => BindingSoilData.BulkDensity;
            set
            {
                ValidateNonNegative(value, nameof(BulkDensity));
                if (HasErrors)
                {
                    return;
                }
                BindingSoilData.BulkDensity = value;
                RaisePropertyChanged(nameof(BulkDensity));
            }
        }
        public double TopLayerThickness
        {
            get => BindingSoilData.TopLayerThickness;
            set
            {
                ValidateNonNegative(value, nameof(TopLayerThickness));
                if (HasErrors)
                {
                    return;
                }
                BindingSoilData.TopLayerThickness = value;
                RaisePropertyChanged(nameof(TopLayerThickness));
            }
        }
        public double ProportionOfClayInSoil
        {
            get => BindingSoilData.ProportionOfClayInSoil;
            set
            {
                ValidateNonNegative(value, nameof(ProportionOfClayInSoil));
                if (HasErrors)
                {
                    return;
                }
                BindingSoilData.ProportionOfClayInSoil = value;
                RaisePropertyChanged(nameof(ProportionOfClayInSoil));
            }
        }
        public double ProportionOfSandInSoil
        {
            get => BindingSoilData.ProportionOfSandInSoil;
            set
            {
                ValidateNonNegative(value, nameof(ProportionOfSandInSoil));
                if (HasErrors)
                {
                    return;
                }
                BindingSoilData.ProportionOfSandInSoil = value;
                RaisePropertyChanged(nameof(ProportionOfSandInSoil));
            }
        }
        public double SoilPh
        {
            get => BindingSoilData.SoilPh;
            set
            {
                ValidateNonNegative(value, nameof(SoilPh));
                if (HasErrors)
                {
                    return;
                }
                BindingSoilData.SoilPh = value;
                RaisePropertyChanged(nameof(SoilPh));
            }
        }
        public double ProportionOfSoilOrganicCarbon
        {
            get => BindingSoilData.ProportionOfSoilOrganicCarbon;
            set
            {
                ValidateNonNegative(value, nameof(ProportionOfSoilOrganicCarbon));
                if (HasErrors)
                {
                    return;
                }
                BindingSoilData.ProportionOfSoilOrganicCarbon = value;
                RaisePropertyChanged(nameof(ProportionOfSoilOrganicCarbon));
            }
        }
        public double SoilCec
        {
            get => BindingSoilData.SoilCec;
            set
            {
                ValidateNonNegative(value, nameof(SoilCec));
                if (HasErrors)
                {
                    return;
                }
                BindingSoilData.SoilCec = value;
                RaisePropertyChanged(nameof(SoilCec));
            }
        }
        public int CarbonModellingEquilibriumYear
        {
            get => ActiveFarm?.CarbonModellingEquilibriumYear ?? 0;
            set
            {
                ValidateYear(value, nameof(CarbonModellingEquilibriumYear));
                if (HasErrors)
                {
                    return;
                }
                if (ActiveFarm is not null)
                {
                    ActiveFarm.CarbonModellingEquilibriumYear = value;
                }
                RaisePropertyChanged(nameof(CarbonModellingEquilibriumYear));
            }
        }

        //Collection of all soil textures for combo box
        public ObservableCollection<SoilTexture> SoilTextures { get; set; } = null!;
        #endregion

        #region Methods
        //Validate that the value is not negative
        public void ValidateNonNegative(double value, string propertyName)
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

        //Validate that the year is greater than 0
        private void ValidateYear(int value, string propertyName)
        {
            if (value <= 0)
            {
                AddError(nameof(propertyName), H.Core.Properties.Resources.ErrorMustbeAValidYear);
            }
            else
            {
                RemoveError(nameof(propertyName));
            }
        }
        public void ManageData()
        {
            //Binds the soil data from the active farm
            if (ActiveFarm is not null)
            {
                BindingSoilData = ActiveFarm.DefaultSoilData!;
            }

            //Populate the soil texture collection
            SoilTextures = new ObservableCollection<SoilTexture>();
            var soilTextures = Enum.GetValues(typeof(SoilTexture));
            foreach (SoilTexture soilTexture in soilTextures)
            {
                if (!SoilTextures.Contains(soilTexture))
                {
                    SoilTextures.Add(soilTexture);
                }
            }

            //Listens for changes to the soil data
            if (BindingSoilData is not null)
            {
                BindingSoilData.PropertyChanged -= OnSoilDataPropertyChanged;
                BindingSoilData.PropertyChanged += OnSoilDataPropertyChanged;
            }
        }
        #endregion

        #region Event Handlers
        public void OnSoilDataPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (ActiveFarm?.DefaultSoilData is null) return;

            if (e.PropertyName == nameof(BulkDensity))
            {
                BulkDensity = ActiveFarm.DefaultSoilData.BulkDensity;
            }
            else if (e.PropertyName == nameof(TopLayerThickness))
            {
                TopLayerThickness = ActiveFarm.DefaultSoilData.TopLayerThickness;
            }
            else if (e.PropertyName == nameof(ProportionOfClayInSoil))
            {
                ProportionOfClayInSoil = ActiveFarm.DefaultSoilData.ProportionOfClayInSoil;
            }
            else if (e.PropertyName == nameof(ProportionOfSandInSoil))
            {
                ProportionOfSandInSoil = ActiveFarm.DefaultSoilData.ProportionOfSandInSoil;
            }
            else if (e.PropertyName == nameof(ProportionOfSoilOrganicCarbon))
            {
                ProportionOfSoilOrganicCarbon = ActiveFarm.DefaultSoilData.ProportionOfSoilOrganicCarbon;
            }
            else if (e.PropertyName == nameof(SoilPh))
            {
                SoilPh = ActiveFarm.DefaultSoilData.SoilPh;
            }
            else if (e.PropertyName == nameof(SoilCec))
            {
                SoilCec = ActiveFarm.DefaultSoilData.SoilCec;
            }
        }
        #endregion
    }
}
