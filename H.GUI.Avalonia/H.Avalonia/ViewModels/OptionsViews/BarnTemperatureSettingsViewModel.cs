using H.Core.Providers.Temperature;
using H.Core.Services.StorageService;
using Prism.Regions;
using System;
using System.ComponentModel;

namespace H.Avalonia.ViewModels.OptionsViews
{
    public class BarnTemperatureSettingsViewModel : ChartBaseViewModel<TemperatureData>
    {
        #region Fields

        private TemperatureData _data = new TemperatureData();

        #endregion

        #region Constructors

        public BarnTemperatureSettingsViewModel(IStorageService storageService) : base(storageService)
        {
            this.InitializeData();
            base.IsInitialized = true;
        }

        #endregion

        #region Properties

        public TemperatureData Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        protected override TemperatureData ChartValuesSource => Data;

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
            if (base.ActiveFarm?.ClimateData?.BarnTemperatureData is not null)
            {
                this.Data = base.ActiveFarm!.ClimateData.BarnTemperatureData;
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

        private void DataOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (sender is TemperatureData)
            {
                base.BuildChart();
            }
        }

        #endregion
    }
}