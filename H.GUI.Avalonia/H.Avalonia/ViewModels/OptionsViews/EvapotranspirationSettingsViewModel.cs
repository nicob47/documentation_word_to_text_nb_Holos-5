using System;
using H.Core.Providers.Evapotranspiration;
using H.Core.Services.StorageService;
using Prism.Regions;

namespace H.Avalonia.ViewModels.OptionsViews
{
    public class EvapotranspirationSettingsViewModel : ChartBaseViewModel<EvapotranspirationData>
    {
        #region Fields

        private EvapotranspirationData _data = new EvapotranspirationData();

        #endregion

        #region Constructors

        public EvapotranspirationSettingsViewModel(IStorageService storageService) : base(storageService)
        {
            this.InitializeData();
            base.IsInitialized = true;
        }

        #endregion

        #region Properties

        public EvapotranspirationData Data 
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        protected override EvapotranspirationData ChartValuesSource => Data;

        #endregion

        #region Public Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!base.IsInitialized)
            {
                this.InitializeData();
                base.IsInitialized = true;
            }
        }

        #endregion

        #region Protected Methods

        protected override void InitializeData()
        {
            if (base.ActiveFarm?.ClimateData?.EvapotranspirationData is not null)
            {
                this.Data = base.ActiveFarm!.ClimateData.EvapotranspirationData;
                this.Data.PropertyChanged -= DataOnPropertyChanged;
                this.Data.PropertyChanged += DataOnPropertyChanged;
            }
            else
            {
                throw new ArgumentNullException(nameof(Data));
            }
            base.BuildChart();
        }

        #endregion

        #region Event Handlers

        private void DataOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is EvapotranspirationData)
            {
                base.BuildChart();
            }
        }

        #endregion
    }
}
