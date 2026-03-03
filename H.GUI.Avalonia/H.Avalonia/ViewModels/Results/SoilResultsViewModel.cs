using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using H.Avalonia.Models;
using H.Avalonia.Models.ClassMaps;
using H.Avalonia.Models.Results;
using H.Avalonia.Infrastructure;
using H.Core.Providers;
using Prism.Commands;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using H.Avalonia.Views.ComponentViews;
using System.Linq;
using H.Avalonia.Services;
using H.Avalonia.Views;
using H.Core.Services.StorageService;

namespace H.Avalonia.ViewModels.Results
{
    public class SoilResultsViewModel : ResultsViewModelBase
    {
        #region Fields
        
        private readonly IRegionManager? _regionManager;
        private IRegionNavigationJournal? _navigationJournal;
        private readonly ExportHelpers _exportHelpers = null!;
        private readonly KmlHelpers _kmlHelpers = null!;
        private readonly GeographicDataProvider _geographicDataProvider = null!;
        private readonly SoilResultsViewItemMap _soilResultsViewItemMap = null!;
        private SoilResultsViewItem _singleSoilResultsViewItem = null!;
        private CancellationTokenSource _cancellationTokenSource = null!;

        #endregion

        #region Constructors

        public SoilResultsViewModel()
        {
        }

#pragma warning disable CS0618 // Storage is obsolete but still required during migration
        public SoilResultsViewModel(IRegionManager regionManager, INotificationManagerService notificationManager ,ExportHelpers exportHelpers, KmlHelpers kmlHelpers, GeographicDataProvider geographicDataProvider, Storage storage, IStorageService storageService) : base(regionManager, notificationManager, storageService)
#pragma warning restore CS0618
        {
            this.StoragePlaceholder = storage;

            _regionManager = regionManager;
            _exportHelpers = exportHelpers;
            _geographicDataProvider = geographicDataProvider;
            _kmlHelpers = kmlHelpers;
            GoBackCommand = new DelegateCommand(OnGoBack);
            ExportToCsvCommand = new DelegateCommand<object>(OnExportToCsv);
            _soilResultsViewItemMap = new SoilResultsViewItemMap();
            this.ChooseSelectedSoilCommand = new DelegateCommand(OnChooseSelectedSoilExecute);
            SwitchToClimateViewCommand = new DelegateCommand(OnClickSwitchToClimateView);
        }

        #endregion

        #region Properties

        /// <summary>
        /// A collection of <see cref="SoilResultsViewItem"/> that are attached to the climate results page. Each viewitem denotes a row in the grid. This collection
        /// is populated using the coordinates entered in the multi-coordinate page.
        /// </summary>
        public ObservableCollection<SoilResultsViewItem> SoilResultsViewItems { get; set; } = new();

        /// <summary>
        /// A collection of <see cref="SoilResultsViewItem"/> that are attached to the climate results page. Each viewitem denotes a row in the grid. This collection
        /// is populated using the coordinates entered in the single-coordinate page.
        /// </summary>
        public ObservableCollection<SoilResultsViewItem> SingleSoilResultsViewItems { get; set; } = new();

        /// <summary>
        /// Allows the user to select which soil they want to use as a <see cref="Farm"/>-level default
        /// </summary>
        public DelegateCommand ChooseSelectedSoilCommand { get; set; } = null!;


        /// <summary>
        /// A command that helps switch the view to <see cref="ClimateDataView"/>
        /// </summary>
        public DelegateCommand SwitchToClimateViewCommand { get; set; } = null!;

        public SoilResultsViewItem SelectedSingleSoilResult
        {
            get { return _singleSoilResultsViewItem; }
            set { SetProperty(ref _singleSoilResultsViewItem, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Triggered when a user navigates to this page.
        /// </summary>
        /// <param name="navigationContext">The navigation context of the user. Contains the navigation tree and journal</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            _navigationJournal = navigationContext.NavigationService.Journal;
            GoBackCommand.RaiseCanExecuteChanged();
            AddViewItemsAsync();
        }

        /// <summary>
        /// Triggered when the user navigates from this page to a different page.
        /// </summary>
        /// <param name="navigationContext"></param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            SoilResultsViewItems.Clear();
            SingleSoilResultsViewItems.Clear();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when the user goes back to the previous page.
        /// </summary>
        private void OnGoBack()
        {
            if (_navigationJournal is not null && _navigationJournal.CanGoBack)
            {
                _cancellationTokenSource.Cancel();
                _navigationJournal.GoBack();
            }
        }

        /// <summary>
        /// Called when the user exports the current grid contents to a csv.
        /// </summary>
        /// <param name="obj">A <see cref="IStorageFile"/> object. Contains the path where the user wants to export the csv.</param>
        private void OnExportToCsv(object obj)
        {
            if (obj is not IStorageFile file) return;
            try
            {
                _exportHelpers.ExportPath = file.Path.AbsolutePath;
                if (StoragePlaceholder != null && StoragePlaceholder.ShowSingleCoordinateResults)
                {
                    _exportHelpers.ExportToCSV(SingleSoilResultsViewItems, _soilResultsViewItemMap);
                }
                else
                {
                    _exportHelpers.ExportToCSV(SoilResultsViewItems, _soilResultsViewItemMap);
                }
            }
            catch (IOException e)
            {
                NotificationManager?.ShowToast(H.Core.Properties.Resources.FileInUse, e.Message, NotificationType.Error);
            }
        }

        /// <summary>
        /// Asynchronously adds viewitems to the grid displayed in the page. Also creates a cancellation token that is
        /// used to cancel the task of adding view items. This task is cancelled when the user goes back from this page to the previous page.
        /// </summary>
        private async void AddViewItemsAsync()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;
            try
            {
                if (StoragePlaceholder == null) return;
                if (StoragePlaceholder.ShowSingleCoordinateResults)
                {
                    var sourceCollection = new ObservableCollection<SoilViewItem>
                    {
                        StoragePlaceholder.SingleSoilViewItem

                    };

                    await AddViewItemsToCollection(cancellationToken, sourceCollection, SingleSoilResultsViewItems);
                }
                else if (StoragePlaceholder.SoilViewItems != null)
                {
                    await AddViewItemsToCollection(cancellationToken, StoragePlaceholder.SoilViewItems, SoilResultsViewItems);
                }
            }
            catch (TaskCanceledException e)
            {
                Trace.TraceInformation($@"{e.Message} and TaskCanceledException thrown in method 
                                            {nameof(AddViewItemsAsync)} in class {nameof(SoilResultsViewModel)}");
                _cancellationTokenSource.Dispose();
            }
        }

        private async Task AddViewItemsToCollection(CancellationToken cancellationToken,
                                                    ObservableCollection<SoilViewItem> sourceCollection,
                                                    ObservableCollection<SoilResultsViewItem> resultsCollection)
        {
            IsProcessingData = true;

            foreach (var viewItem in sourceCollection)
            {
                var polygon = await Task.Run(() => _kmlHelpers.GetPolygonFromCoordinateAsync(viewItem.Latitude, viewItem.Longitude), cancellationToken);
                var polygonData = _geographicDataProvider.GetGeographicalData(polygon);
                var soilData = polygonData.SoilDataForAllComponentsWithinPolygon;
                foreach (var soil in soilData)
                {
                    resultsCollection.Add(new SoilResultsViewItem
                    {
                        Latitude = viewItem.Latitude,
                        Longitude = viewItem.Longitude,
                        SoilGreatGroup = soil.SoilGreatGroup,
                        SoilTexture = soil.SoilTexture,
                        Province = soil.Province,
                        SoilPh = soil.SoilPh,
                        PercentClayInSoil = soil.ProportionOfClayInSoilAsPercentage,
                        PercentOrganicMatterInSoil = soil.ProportionOfSoilOrganicCarbon,
                    });
                }
            }

            IsProcessingData = false;
        }

        /// <summary>
        /// Once the user has selected a soil type (one must be selected as the default for the <see cref="Farm"/>>), navigate to
        /// the <see cref="MyComponentsView"/>.
        /// </summary>
        private void OnChooseSelectedSoilExecute()
        {
            base.RegionManager?.RequestNavigate(UiRegions.SidebarRegion, nameof(MyComponentsView));
            var view = this.RegionManager?.Regions[UiRegions.ContentRegion].ActiveViews.Single();
            if (view != null)
            {
                this.RegionManager?.Regions[UiRegions.ContentRegion].Deactivate(view);
                this.RegionManager?.Regions[UiRegions.ContentRegion].Remove(view);
            }
        }

        private void OnClickSwitchToClimateView()
        {
            this.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(ClimateDataView));
        }

        #endregion
    }
}