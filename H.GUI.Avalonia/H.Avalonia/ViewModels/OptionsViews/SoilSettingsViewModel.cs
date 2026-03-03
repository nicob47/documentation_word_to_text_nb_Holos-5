using H.Avalonia.ViewModels.OptionsViews.DataTransferObjects;
using H.Core.Services.StorageService;
using Prism.Regions;

namespace H.Avalonia.ViewModels.OptionsViews
{
    public class SoilSettingsViewModel : ViewModelBase
    {
        #region Fields

        SoilSettingsDTO _data = null!;

        #endregion

        #region Constructors
        public SoilSettingsViewModel() { }
        public SoilSettingsViewModel(IStorageService storageService) : base(storageService)
        {
            Data = new SoilSettingsDTO(StorageService!);
        }
        #endregion

        #region Properties
        public SoilSettingsDTO Data 
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }
        #endregion

        #region Public Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!IsInitialized)
            {
                Data = new SoilSettingsDTO(StorageService!);
                IsInitialized = true;
            }
        }

        #endregion

        #region Event Handlers

        #endregion
    }
}
