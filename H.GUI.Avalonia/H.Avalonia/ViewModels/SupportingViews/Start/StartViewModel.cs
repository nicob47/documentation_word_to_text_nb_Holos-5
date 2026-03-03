using H.Core.Properties;
using H.Core.Providers;
using Prism.Events;
using Prism.Regions;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using H.Infrastructure;
using Avalonia.Threading;
using H.Avalonia.Views.FarmCreationViews;
using H.Core.Services.StorageService;
using H.Core.Providers.Soil;
using Microsoft.Extensions.Logging;

namespace H.Avalonia.ViewModels.SupportingViews.Start
{
    class StartViewModel : ViewModelBase, INavigationAware
    {
        #region Fields

        private bool _isBusy;
        private string? _IsBusyMessage;
        private int _progressValue;

        GeographicDataProvider? _geographicDataProvider;
        SmallAreaYieldProvider? _smallAreaYieldProvider;

        #endregion

        #region Constructors

        public StartViewModel()
        {

        }
 
        public StartViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IStorageService storageService, GeographicDataProvider geographicDataProvider, SmallAreaYieldProvider smallAreaYieldProvider, ILogger logger) : base(regionManager, eventAggregator, storageService, logger) 
        {
            if (geographicDataProvider != null)
            {
                _geographicDataProvider = geographicDataProvider;
            }
            else
            {
                throw (new ArgumentNullException(nameof(geographicDataProvider)));
            }
            if (smallAreaYieldProvider != null)
            {
                _smallAreaYieldProvider = smallAreaYieldProvider;
            }
            else
            {
                throw (new ArgumentNullException(nameof(smallAreaYieldProvider)));
            }
        }

        #endregion

        #region Properties

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                this.SetProperty(ref _isBusy, value, () =>
                {
                    if (_isBusy)
                    {
                        var backgroundWorker = new BackgroundWorker();
                        backgroundWorker.DoWork += this.OnBackgroundWorkerDoWork;
                        backgroundWorker.RunWorkerCompleted += this.OnBackgroundWorkerCompleted;
                        backgroundWorker.RunWorkerAsync();
                    }
                });
            }
        }

        public string? IsBusyMessage
        {
            get { return _IsBusyMessage; }
            set { this.SetProperty(ref _IsBusyMessage, value); }
        }

        public int ProgressValue
        {
            get { return _progressValue; }
            set { this.SetProperty(ref _progressValue, value); }
        }

        #endregion

        #region Public Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);
            this.IsBusy = true;
        }

        private void Initialize()
        {
            //_cropDefaultsViewModel.InitializeViewModel();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            this.Initialize();
        }

        #endregion

        #region Private Methods

        private void OnBackgroundWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        {
            if (sender is BackgroundWorker backgroundWorker)
            {
                backgroundWorker.DoWork -= this.OnBackgroundWorkerDoWork;
                backgroundWorker.RunWorkerCompleted -= this.OnBackgroundWorkerCompleted;
                this.InvokeOnUiThread(() => { this.IsBusy = false; });

                // When first installed, user will have no farms. Show the create new farm view.
                if (StorageService?.GetAllFarms().Any() == false)
                {
                    base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(FarmCreationView));
                }
                else
                {
                    // If this is not the first run after installation (there is at least one farm), show the farm options view.
                    base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(FarmOptionsView));
                }
            }
        }

        private void OnBackgroundWorkerDoWork(object? sender, DoWorkEventArgs e)
        {
            try
            {
                // This is running on a separate thread than the main UI thread, need to set culture for this thread
                if (Settings.Default.DisplayLanguage.Equals(H.Core.Enumerations.Languages.French.GetDescription(), StringComparison.InvariantCultureIgnoreCase))
                {
                    var culture = H.Infrastructure.InfrastructureConstants.FrenchCultureInfo;
                    Thread.CurrentThread.CurrentCulture = culture;
                    Thread.CurrentThread.CurrentUICulture = culture;
                }

                this.IsBusyMessage = H.Core.Properties.Resources.MessageLoadingPleaseWait;

                this.ProgressValue = 0;
                _geographicDataProvider?.Initialize();
                
                this.ProgressValue = 25;

                this.ProgressValue = 50;
                _smallAreaYieldProvider?.Initialize();

                //base.InvokeOnUiThread(() => _mapViewModel.LoadMapFrameworkElements());
                
                this.ProgressValue = 75;
                
                this.ProgressValue = 100;
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Invoke(new Action(() =>
                {
                    throw new Exception($"Exception in {nameof(StartViewModel)}.{nameof(StartViewModel.OnBackgroundWorkerDoWork)}. Error: {ex.Message}", ex);
                }), DispatcherPriority.Normal);
            }
        }

        #endregion
    }
}