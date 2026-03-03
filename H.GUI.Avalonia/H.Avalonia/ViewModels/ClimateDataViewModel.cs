using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using CsvHelper;
using CsvHelper.TypeConversion;
using H.Avalonia.Infrastructure;
using H.Avalonia.Infrastructure.Dialogs;
using H.Avalonia.Models;
using H.Avalonia.Models.ClassMaps;
using H.Avalonia.Views;
using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using H.Avalonia.Services;
using H.Avalonia.Views.ResultViews;
using H.Core.Services.Climate;
using H.Core.Services.StorageService;
using ClimateResultsView = H.Avalonia.Views.ResultViews.ClimateResultsView;

namespace H.Avalonia.ViewModels
{
    public class ClimateDataViewModel : ViewModelBase, IDataGridFeatures
    {
        private IRegionNavigationJournal? _navigationJournal;
        private readonly IDialogService _dialogService = null!;
        private readonly ImportHelpers _importHelper = null!;
        private readonly ClimateViewItemMap _climateViewItemMap = null!;

        private IClimateService _climateService = null!;
        private ObservableCollection<ClimateViewItem> _climateViewItems = null!;

        /// <summary>
        /// Allows navigation from the current view to the <see cref="SoilResultsView"/>.
        /// </summary>
        public DelegateCommand OnGetClimateDataCommand { get; set; } = null!;

        /// <summary>
        /// A command that adds rows to the grid displayed on <see cref="ClimateDataView"/>. Each row indicates <see cref="ClimateViewItem"/>.
        /// </summary>
        public DelegateCommand AddRowCommand { get; set; } = null!;

        /// <summary>
        /// A command that removes rows to the grid displayed on <see cref="ClimateDataView"/>. Each row indicates <see cref="ClimateViewItem"/>.
        /// </summary>
        public DelegateCommand<object> DeleteRowCommand { get; set; } = null!;

        /// <summary>
        /// Deletes a row that is currently marked as selected by the user.
        /// </summary>
        public DelegateCommand DeleteSelectedRowsCommand { get; set; } = null!;

        /// <summary>
        /// Import climate data from a csv file. The csv file must have the following columns:
        /// Latitude, Longitude, Start Year, End Year, Julian day start, Julian day end (respectively).
        /// </summary>
        public DelegateCommand<object> ImportFromCsvCommand { get; set; } = null!;

        /// <summary>
        /// Toggles the select all row command. This command either selects or deselects all the rows currently displayed in the grid.
        /// </summary>
        public DelegateCommand ToggleSelectAllRowsCommand { get; set; } = null!;

        /// <summary>
        /// A bool that indicates if all climate data items are selected in the grid. Returns true if all items are selected, returns false otherwise.
        /// </summary>
        public bool AllViewItemsSelected { get; set; }

        /// <summary>
        /// A bool that indicates if the grid has any climate view items currently added to it. Returns true if Any view items exist, returns false otherwise.
        /// </summary>
        public bool HasViewItems
        {
            get { return this.ClimateViewItems != null && this.ClimateViewItems.Any(); }
        }

        /// <summary>
        /// A bool that indicates if any climate view items are selected or not. Returns true if at least one view item is selected, returns false if none are selected.
        /// </summary>
        public bool AnyViewItemsSelected
        {
            get
            {
                return this.ClimateViewItems != null &&
                       this.ClimateViewItems.Any(item => item.IsSelected);
            }
        }

        public ObservableCollection<ClimateViewItem> ClimateViewItems
        {
            get { return _climateViewItems; }
            set { SetProperty(ref _climateViewItems, value); }
        }

        public ClimateDataViewModel()
        {
        }

        public ClimateDataViewModel(
            IRegionManager regionManager,
            ImportHelpers importHelper,
            IDialogService dialogService,
            INotificationManagerService notificationManager,
            IClimateService climateService,
            IStorageService storageService) : base(regionManager, notificationManager, storageService)
        {
            if (climateService != null)
            {
                _climateService = climateService;
            }
            else
            {
                throw new ArgumentNullException(nameof(climateService));
            }

            if (importHelper != null)
            {
                _importHelper = importHelper;
            }
            else
            {
                throw new ArgumentNullException(nameof(importHelper));
            }

            if (dialogService != null)
            {
                _dialogService = dialogService;
            }
            else
            {
                throw new ArgumentNullException(nameof(dialogService));
            }

            InitializeCommands();

            _climateViewItemMap = new ClimateViewItemMap();

            this.ClimateViewItems = new ObservableCollection<ClimateViewItem>();
        }

        /// <summary>
        /// Initializes the various commands used by the related view.
        /// </summary>
        private void InitializeCommands()
        {
            this.OnGetClimateDataCommand = new DelegateCommand(OnGetClimateDataExecute).ObservesCanExecute(() => HasViewItems);
            this.AddRowCommand = new DelegateCommand(OnAddRowExecute);
            this.ImportFromCsvCommand = new DelegateCommand<object>(OnImportCsvExecute);
            this.DeleteRowCommand = new DelegateCommand<object>(OnDeleteRowExecute);
            this.DeleteSelectedRowsCommand = new DelegateCommand(OnDeleteSelectedRowsExecute).ObservesCanExecute(() => AnyViewItemsSelected);
            this.ToggleSelectAllRowsCommand = new DelegateCommand(OnToggleSelectAllRowsExecute).ObservesCanExecute(() => HasViewItems);
        }

        /// <summary>
        /// Triggered when the <see cref="Storage.ClimateViewItems"/> changes. This method raises CanExecuteChanged events for the various
        /// buttons on the page and also attaches/detaches PropertyChanged events to individual properties inside the collection so that
        /// we can be notified when an internal property changes in the collection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClimateViewItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            this.ToggleSelectAllRowsCommand.RaiseCanExecuteChanged();
            this.DeleteSelectedRowsCommand.RaiseCanExecuteChanged();
            this.OnGetClimateDataCommand.RaiseCanExecuteChanged();

            if (e.NewItems != null)
            {
                foreach (INotifyPropertyChanged item in e.NewItems)
                {
                    if (item != null)
                    {
                        item.PropertyChanged += CollectionItemOnPropertyChanged;
                    }
                }

                AllViewItemsSelected = false;
            }

            if (e.OldItems == null) return;
            {
                foreach (INotifyPropertyChanged item in e.OldItems)
                {
                    if (item != null)
                    {
                        item.PropertyChanged -= CollectionItemOnPropertyChanged;
                    }
                }
            }
        }

        /// <summary>
        /// A property changed event that is attached to each property of the <see cref="Storage.ClimateViewItems"/> collection.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">The event that was triggered.</param>
        private void CollectionItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ClimateViewItem.IsSelected))
            {
                DeleteSelectedRowsCommand.RaiseCanExecuteChanged();

                if (sender is not ClimateViewItem viewItem) return;
                if (!viewItem.IsSelected)
                {
                    AllViewItemsSelected = false;
                }
            }
        }

        /// <summary>
        /// Triggered when a user navigates to this page.
        /// </summary>
        /// <param name="navigationContext">The navigation context of the user. Contains the navigation tree and journal</param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            // When we navigate to this view, we instantiate the journal property. This allows us to do navigation through journaling.
            _navigationJournal = navigationContext.NavigationService.Journal;

            //var a = base.ActiveFarm.ClimateData.DailyClimateData;

            if (this.ClimateViewItems != null)
            {
                this.ClimateViewItems.CollectionChanged += OnClimateViewItemsCollectionChanged;
            }
        }

        /// <summary>
        /// Uses Prism framework to switch from <see cref="ClimateDataView"/> to <see cref="ClimateResultsView"/>.
        /// </summary>
        private void OnGetClimateDataExecute()
        {
            base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(ClimateResultsView), new NavigationParameters() { { "ClimateViewItems", this.ClimateViewItems } });
        }

        /// <summary>
        /// Add a row to the grid on  the <see cref="ClimateDataView"/>
        /// </summary>
        private void OnAddRowExecute()
        {
            if (base.ActiveFarm is not null)
            {
                var climateViewItem = new ClimateViewItem()
                {
                    Latitude = base.ActiveFarm.Latitude,
                    Longitude = base.ActiveFarm.Longitude,
                };


                this.ClimateViewItems?.Add(climateViewItem);
            }
        }

        /// <summary>
        /// Deletes a row from the grid on <see cref="ClimateDataView"/>
        /// </summary>
        /// <param name="obj">The <see cref="ClimateViewItem"/> that needs to be deleted.</param>
        private void OnDeleteRowExecute(object obj)
        {
            if (obj is not ClimateViewItem viewItem) return;

            var message = Core.Properties.Resources.RowDeleteMessage;
            _dialogService.ShowMessageDialog(nameof(DeleteRowDialog), message, r =>
            {
                if (r.Result == ButtonResult.OK)
                {
                    this.ClimateViewItems?.Remove(viewItem);
                }
            });

            this.OnGetClimateDataCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Deletes a group of <see cref="ClimateViewItem"/> that are selected by the user.
        /// </summary>
        private void OnDeleteSelectedRowsExecute()
        {
            if (!this.ClimateViewItems.Any()) return;

            var message = Core.Properties.Resources.RowDeleteMessage;
            _dialogService.ShowMessageDialog(nameof(DeleteRowDialog), message, r =>
            {
                if (r.Result != ButtonResult.OK) return;
                var currentItems = this.ClimateViewItems.ToList();
                foreach (var item in currentItems.Where(item => item.IsSelected))
                {
                    this.ClimateViewItems.Remove(item);
                }

                if (!HasViewItems)
                {
                    AllViewItemsSelected = false;
                }
            });

            this.OnGetClimateDataCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// Called when the user imports a csv file. The imported csv file must have the following column headers:
        /// Latitude, Longitude, Start Year, End Year, Julian day start, Julian day end (respectively).
        /// </summary>
        /// <param name="obj">The <see cref="IStorageItem"/> object passed to the method containing the file path where the csv is located.</param>
        private void OnImportCsvExecute(object? obj)
        {
            var item = obj as IReadOnlyCollection<IStorageItem>;
            var file = item?.FirstOrDefault();

            if (file == null) return;

            _importHelper.ImportPath = file.Path.AbsolutePath;
            try
            {
                this.ClimateViewItems.AddRange(_importHelper.ImportFromCsv(_climateViewItemMap));

            }
            catch (HeaderValidationException e)
            {
                NotificationManager?.ShowToast(H.Core.Properties.Resources.InvalidHeaderTitle, e.Message, NotificationType.Error);
            }
            catch (TypeConverterException e)
            {
                NotificationManager?.ShowToast(H.Core.Properties.Resources.InvalidCSVContentTitle, e.Message, NotificationType.Error);
            }
            catch (IOException e)
            {
                NotificationManager?.ShowToast(H.Core.Properties.Resources.FileInUse, e.Message, NotificationType.Error);
            }
        }

        /// <summary>
        /// Helps select all rows that are currently added to the grid.
        /// </summary>
        private void OnToggleSelectAllRowsExecute()
        {
            //if (StoragePlaceholder?.ClimateViewItems == null) return;
            if (AllViewItemsSelected)
            {
                foreach (var item in this.ClimateViewItems)
                {
                    item.IsSelected = false;
                }

                AllViewItemsSelected = false;
            }
            else
            {
                foreach (var item in this.ClimateViewItems)
                {
                    item.IsSelected = true;
                }

                AllViewItemsSelected = true;
            }
        }
    }
}
