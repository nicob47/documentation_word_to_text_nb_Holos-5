using System.Collections.ObjectModel;
using DynamicData;
using System.Linq;
using H.Core.Models;
using H.Core.Services.StorageService;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using H.Avalonia.Views.FarmCreationViews;
using Avalonia.Controls.Notifications;
using Microsoft.Extensions.Logging;
using H.Avalonia.Services;

namespace H.Avalonia.ViewModels.OptionsViews.FileMenuViews
{
    public class FarmManagementViewModel : ViewModelBase
    {
        #region Fields
        private ObservableCollection<Farm> _farms = null!;
        private Farm? _selectedFarm;
        private string _searchText = string.Empty;
        #endregion

        #region Constructors

        public FarmManagementViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IStorageService storageService, INotificationManagerService notificationManager) : base(regionManager, eventAggregator, storageService, notificationManager)
        {
            RemoveFarm = new DelegateCommand(OnRemoveFarmExecute, OnRemoveFarmCanExecute);
            Farms = new ObservableCollection<Farm>();
        }

        public FarmManagementViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IStorageService storageService, ILogger logger) : base(regionManager, eventAggregator, storageService, logger)
        {
            RemoveFarm = new DelegateCommand(OnRemoveFarmExecute, OnRemoveFarmCanExecute);
            Farms = new ObservableCollection<Farm>();
        }

        #endregion

        #region Properties

        public DelegateCommand RemoveFarm { get; }

        public ObservableCollection<Farm> Farms
        {
            get => _farms;
            set => SetProperty(ref _farms, value);
        }

        public Farm? SelectedFarm
        {
            get => _selectedFarm;
            set
            {
                SetProperty(ref _selectedFarm, value);
                RemoveFarm.RaiseCanExecuteChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                if (base.StorageService == null) return;

                if (string.IsNullOrEmpty(value))
                {
                    Farms.Clear();
                    var farms = base.StorageService.GetAllFarms();
                    Farms.Add(farms);
                }
                else
                {
                    Farms.Clear();
                    var farms = base.StorageService.GetAllFarms().Where(f => (f.Name?.ToLower().Contains(value.ToLower()) ?? false) || (f.DefaultSoilData?.EcodistrictName?.ToLower().Contains(value.ToLower()) ?? false) || f.Province.ToString().ToLower().Contains(value.ToLower()));
                    Farms.Add(farms);
                }
            }
        }

        #endregion

        #region Public Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            Farms.Clear();
            if (StorageService != null)
            {
                Farms.AddRange(StorageService.Storage.ApplicationData.Farms);
            }
        }

        #endregion

        #region Private Methods

        private void OnRemoveFarmExecute()
        {
            if (this.Farms.Count > 1 && this.SelectedFarm is not null)
            {
                var userDeletedCurrentFarm = Equals(this.SelectedFarm, base.StorageService?.GetActiveFarm());

                base.StorageService?.Storage.ApplicationData.Farms.Remove(this.SelectedFarm);
                this.Farms.Clear();
                this.Farms.AddRange(base.StorageService?.Storage.ApplicationData.Farms ?? Enumerable.Empty<Farm>());


                if (userDeletedCurrentFarm)
                {
                    this.ClearActiveView();
                    base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(FarmOptionsView));
                }
            }
            else
            {
                NotificationManager?.ShowToast(H.Core.Properties.Resources.CantDeleteCurrentFarmTitle, H.Core.Properties.Resources.CantDeleteCurrentFarmBody, NotificationType.Warning);
            }
        }

        private void ClearActiveView()
        {
            // Clear content region
            var contentView = this.RegionManager?.Regions[UiRegions.ContentRegion].ActiveViews.SingleOrDefault();
            if (contentView != null)
            {
                this.RegionManager?.Regions[UiRegions.ContentRegion].Deactivate(contentView);
                this.RegionManager?.Regions[UiRegions.ContentRegion].Remove(contentView);
            }

            // Clear sidebar region
            var sidebarView = this.RegionManager?.Regions[UiRegions.SidebarRegion].ActiveViews.SingleOrDefault();
            if (sidebarView != null)
            {
                this.RegionManager?.Regions[UiRegions.SidebarRegion].Deactivate(sidebarView);
                this.RegionManager?.Regions[UiRegions.SidebarRegion].Remove(sidebarView);
            }
        }

        private bool OnRemoveFarmCanExecute()
        {
            return this.SelectedFarm is not null;
        }

        #endregion
    }
}

