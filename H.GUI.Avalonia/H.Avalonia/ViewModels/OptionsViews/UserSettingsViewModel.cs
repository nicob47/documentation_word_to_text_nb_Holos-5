using System;
using H.Avalonia.ViewModels.OptionsViews.DataTransferObjects;
using H.Core.Services.StorageService;
using Prism.Regions;

namespace H.Avalonia.ViewModels.OptionsViews
{
    public class UserSettingsViewModel : ViewModelBase
    {
        #region Fields

        private UserSettingsDTO? _data;

        #endregion

        #region Constructors

        public UserSettingsViewModel() { }

        public UserSettingsViewModel(IStorageService storageService) : base(storageService)
        {
            this.Initialize();
            base.IsInitialized = true;
        }

        #endregion

        #region Properties

        public UserSettingsDTO? Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        #endregion

        #region Public Methods 

        public void Initialize()
        {
            Data = new UserSettingsDTO(base.StorageService!);
        }

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!base.IsInitialized)
            {
                this.Initialize();
                base.IsInitialized = true;
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Override this method to provide specific cleanup logic for UserSettingsViewModel resources
        /// </summary>
        protected override void CleanupResources()
        {
            // Always call base implementation first to clean up ViewModelBase resources
            base.CleanupResources();

            // Clean up UserSettingsDTO if it implements IDisposable
            if (_data is IDisposable disposableData)
            {
                disposableData.Dispose();
                _data = null;
            }
            else if (_data is not null)
            {
                // Even if it doesn't implement IDisposable, clear the reference to help GC
                _data = null;
            }
        }

        #endregion

        #region Event Handlers

        #endregion
    }
}
