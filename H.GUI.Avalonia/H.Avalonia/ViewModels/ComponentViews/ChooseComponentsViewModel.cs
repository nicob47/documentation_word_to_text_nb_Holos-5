using Prism.Events;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using H.Avalonia.Events;
using H.Core.Models;
using H.Core.Models.Animals.Sheep;
using H.Core.Models.LandManagement.Fields;  
using H.Core.Models.LandManagement.Rotation;
using H.Core.Models.Animals.Beef;
using H.Core.Models.LandManagement.Shelterbelt;
using H.Core.Models.Animals.Dairy;
using H.Core.Models.Animals.OtherAnimals;
using H.Core.Models.Infrastructure;
using H.Core.Models.Animals.Swine;
using H.Core.Models.Animals.Poultry.Chicken;
using H.Core.Models.Animals.Poultry.Turkey;
using H.Core.Services.StorageService;
using H.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Prism.Commands;
using Avalonia.Controls.Notifications;
using H.Avalonia.Services;

namespace H.Avalonia.ViewModels.ComponentViews
{
    public class ComponentGroup
    {
        public string CategoryName { get; set; } = string.Empty;
        public ObservableCollection<ComponentBase> Components { get; set; } = new ObservableCollection<ComponentBase>();
    }

    public class ChooseComponentsViewModel : ViewModelBase
    {
        #region Fields

        private string? _selectedComponentTitle;
        private string? _selectedComponentDescription;

        private ComponentBase? _selectedComponent;

        private ObservableCollection<ComponentBase> _availableComponents = null!;
        private ObservableCollection<ComponentGroup> _groupedComponents = null!;

        #endregion

        #region Constructors

        public ChooseComponentsViewModel()
        {
            this.AvailableComponents = new ObservableCollection<ComponentBase>();
            InitializeAvailableComponents();
            CreateGroupedComponents();
            this.SelectedComponent = this.AvailableComponents.First();
            InitializeCommands();
        }

        public ChooseComponentsViewModel(IEventAggregator eventAggregator, IRegionManager regionManager, IStorageService storageService, ILogger logger, INotificationManagerService notificationManager) : base(regionManager, eventAggregator, storageService, logger)
        {
            this.NotificationManager = notificationManager;
            this.PropertyChanged += OnPropertyChanged;
            this.AvailableComponents = new ObservableCollection<ComponentBase>();
            InitializeAvailableComponents();
            CreateGroupedComponents();
            this.SelectedComponent = this.AvailableComponents.First();
            InitializeCommands();
        }

        #endregion

        #region Properties

        public string? SelectedComponentTitle
        {
            get => _selectedComponentTitle;
            set => SetProperty(ref _selectedComponentTitle, value);
        }

        public string? SelectedComponentDescription
        {
            get => _selectedComponentDescription;
            set => SetProperty(ref _selectedComponentDescription, value);
        }

        public ObservableCollection<ComponentBase> AvailableComponents
        {
            get => _availableComponents;
            set => SetProperty(ref _availableComponents, value);
        }

        public ObservableCollection<ComponentGroup> GroupedComponents
        {
            get => _groupedComponents;
            private set => SetProperty(ref _groupedComponents, value);
        }

        public ComponentBase? SelectedComponent
        {
            get => _selectedComponent;
            set => SetProperty(ref _selectedComponent, value);
        }

        public DelegateCommand<ComponentBase> SelectComponentCommand { get; private set; } = null!;

        #endregion

        #region Public Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);                

            this.InitializeViewModel();
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);
        }

        public override void InitializeViewModel()
        {
            this.SelectedComponent = this.AvailableComponents.First();
        }

        #endregion

        #region Private Methods

        private void InitializeCommands()
        {
            SelectComponentCommand = new DelegateCommand<ComponentBase>(OnSelectComponent);
        }

        private void OnSelectComponent(ComponentBase? component)
        {
            if (component is not null)
            {
                SelectedComponent = component;
            }
        }

        private void CreateGroupedComponents()
        {
            GroupedComponents = new ObservableCollection<ComponentGroup>();
            
            var groupedData = AvailableComponents
                .GroupBy(c => c.ComponentCategory)
                .OrderBy(g => g.Key)
                .ToList();

            foreach (var group in groupedData)
            {
                var componentGroup = new ComponentGroup
                {
                    CategoryName = group.Key.GetDescription(),
                };

                var sortedComponents = group.OrderBy(c => c.ComponentType.GetDescription()).ToList();
                foreach (var component in sortedComponents)
                {
                    componentGroup.Components.Add(component);
                }

                GroupedComponents.Add(componentGroup);
            }
        }

        private void UpdateComponentDescription()
        {
        }

        private void InitializeAvailableComponents()
        {
            /*
             * Land Management
             */

            _availableComponents.Add(new RotationComponent());
            _availableComponents.Add(new ShelterbeltComponent());
            _availableComponents.Add(new FieldSystemComponent());
            
            /*
             * Beef production
             */

            _availableComponents.Add(new CowCalfComponent());
            _availableComponents.Add(new BackgroundingComponent());
            _availableComponents.Add(new FinishingComponent());

            /*
             * Dairy cattle
             */

            _availableComponents.Add(new DairyComponent());

            /*
             * Swine
             */

            _availableComponents.Add(new GrowerToFinishComponent());
            _availableComponents.Add(new FarrowToWeanComponent());
            _availableComponents.Add(new IsoWeanComponent());
            _availableComponents.Add(new FarrowToFinishComponent());

            /*
             * Sheep
             */

            // _availableComponents.Add(new SheepComponent());
            _availableComponents.Add(new SheepFeedlotComponent());
            _availableComponents.Add(new RamsComponent());
            _availableComponents.Add(new EwesAndLambsComponent());

            /*
             * Other animals / livestock
             */

            _availableComponents.Add(new GoatsComponent());
            _availableComponents.Add(new DeerComponent());
            _availableComponents.Add(new HorsesComponent());
            _availableComponents.Add(new MulesComponent());
            _availableComponents.Add(new BisonComponent());
            _availableComponents.Add(new LlamaComponent());
            
            /*
             * Poultry
             */

            _availableComponents.Add(new ChickenPulletsComponent());
            _availableComponents.Add(new ChickenMultiplierBreederComponent());
            _availableComponents.Add(new ChickenMeatProductionComponent());
            _availableComponents.Add(new TurkeyMultiplierBreederComponent());
            _availableComponents.Add(new TurkeyMeatProductionComponent());
            _availableComponents.Add(new ChickenEggProductionComponent());
            _availableComponents.Add(new ChickenMultiplierHatcheryComponent());

            /* 
             * Infrastructure
             */

            _availableComponents.Add(new AnaerobicDigestionComponent());
        }

        #endregion

        #region Event Handlers

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(this.SelectedComponent))
            {
                if (this.SelectedComponent is not null)
                {
                    this.SelectedComponentTitle = this.SelectedComponent.ComponentType.GetDescription();
                    this.SelectedComponentDescription = this.SelectedComponent.ComponentDescriptionString;
                }
            }
        }

        public void OnAddComponentExecute()
        {
            if (this.SelectedComponent is not null)
            {
                base.EventAggregator?.GetEvent<ComponentAddedEvent>().Publish(this.SelectedComponent);

                var componentName = this.SelectedComponent.ComponentType.GetDescription();
                this.NotificationManager?.ShowToast(
                    H.Core.Properties.Resources.ToastTitleComponentAdded,
                    string.Format(H.Core.Properties.Resources.ToastMessageComponentAddedToFarm, componentName),
                    NotificationType.Success);
            }
        }

        public void OnFinishedAddingComponentsExecute()
        {
            var view = this.RegionManager?.Regions[UiRegions.ContentRegion].ActiveViews.Single();
            if (view != null)
            {
                this.RegionManager?.Regions[UiRegions.ContentRegion].Deactivate(view);
                this.RegionManager?.Regions[UiRegions.ContentRegion].Remove(view);
            }

            base.EventAggregator?.GetEvent<EditingComponentsCompletedEvent>().Publish();
        }

        #endregion
    }
}
