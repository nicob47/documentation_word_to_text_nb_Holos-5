using System.ComponentModel;
using H.Avalonia.ViewModels.OptionsViews.DataTransferObjects;
using H.Core.Providers.Precipitation;
using H.Core.Services.StorageService;
using Prism.Regions;

namespace H.Avalonia.ViewModels.OptionsViews
{
    public class PrecipitationSettingsViewModel : ChartBaseViewModel<PrecipitationData>
    {
        #region Fields

        private PrecipitationSettingsDTO _data = null!;

        #endregion

        #region Constructors

        public PrecipitationSettingsViewModel(IStorageService storageService) : base(storageService)
        {
            this.InitializeData();     
            base.IsInitialized = true;
        }

        #endregion

        #region Properties

        public PrecipitationSettingsDTO Data
        {
            get => _data;
            set => SetProperty(ref  _data, value);
        }

        protected override PrecipitationData ChartValuesSource => Data.PrecipitationData;

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
            this.Data = new PrecipitationSettingsDTO(base.StorageService!);
            this.Data.PropertyChanged -= this.DataOnPropertyChanged;
            this.Data.PropertyChanged += this.DataOnPropertyChanged;
            base.BuildChart();
        }

        #endregion

        #region Event Handlers

        private void DataOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            base.BuildChart();
        }

        #endregion

    }
}
