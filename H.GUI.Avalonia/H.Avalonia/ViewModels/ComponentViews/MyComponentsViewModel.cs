using System;
using System.Collections.ObjectModel;
using System.Linq;
using H.Avalonia.Events;
using H.Avalonia.Models;
using H.Avalonia.Views;
using H.Avalonia.Views.ComponentViews;
using H.Avalonia.Views.ResultViews;
using H.Core.Models;
using H.Core.Services;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace H.Avalonia.ViewModels.ComponentViews;

public class MyComponentsViewModel : ViewModelBase
{
    #region Fields

    private ComponentBase? _selectedComponent;
    private ComponentItemViewModel? _selectedComponentItem;
    private ObservableCollection<ComponentBase> _myComponents = null!;
    private ObservableCollection<ComponentItemViewModel> _myComponentItems = null!;

    private IComponentInitializationService? _componentInitializationService;

    #endregion

    #region Constructors

    public MyComponentsViewModel()
    {
        this.MyComponents = new ObservableCollection<ComponentBase>();
        this.MyComponentItems = new ObservableCollection<ComponentItemViewModel>();
    }

    public MyComponentsViewModel(IRegionManager regionManager, IEventAggregator eventAggregator, IStorageService storageService, IComponentInitializationService componentInitializationService, ILogger logger) : base(regionManager, eventAggregator, storageService, logger)
    {
        if (componentInitializationService != null)
        {
            _componentInitializationService = componentInitializationService; 
        }
        else
        {
            throw new ArgumentNullException(nameof(componentInitializationService));
        }
        
        base.PropertyChanged += OnPropertyChanged;

        this.MyComponents = new ObservableCollection<ComponentBase>();
        this.MyComponentItems = new ObservableCollection<ComponentItemViewModel>();
        
        // Initialize commands
        RemoveComponent = new DelegateCommand(OnRemoveComponentExecute, OnRemoveComponentCanExecute);
        SetSelectedComponentCommand = new DelegateCommand<object>(OnSetSelectedComponentExecute);
        RemoveSpecificComponentCommand = new DelegateCommand<object>(OnRemoveSpecificComponentExecute);
        
        base.EventAggregator?.GetEvent<ComponentAddedEvent>().Subscribe(OnComponentAddedEvent);
        base.EventAggregator?.GetEvent<EditingComponentsCompletedEvent>().Subscribe(OnEditingComponentsCompletedEvent);
        
        // Subscribe to active farm changes
        if (base.StorageService?.Storage?.ApplicationData?.GlobalSettings != null)
        {
            base.StorageService.Storage.ApplicationData.GlobalSettings.PropertyChanged += OnActiveFarmChanged;
        }
    }

    #endregion

    #region Properties

    public ComponentBase? SelectedComponent
    {
        get => _selectedComponent;
        set 
        {
            if (SetProperty(ref _selectedComponent, value))
            {
                // Update selection states for all components
                UpdateComponentSelectionStates(value);
                RemoveComponent.RaiseCanExecuteChanged();
            }
        }
    }

    public ComponentItemViewModel? SelectedComponentItem
    {
        get => _selectedComponentItem;
        set 
        {
            if (SetProperty(ref _selectedComponentItem, value))
            {
                // Update the actual selected component
                SelectedComponent = value?.Component;
            }
        }
    }

    public ObservableCollection<ComponentBase> MyComponents
    {
        get => _myComponents;
        set => SetProperty(ref _myComponents, value);
    }

    public ObservableCollection<ComponentItemViewModel> MyComponentItems
    {
        get => _myComponentItems;
        set => SetProperty(ref _myComponentItems, value);
    }

    public DelegateCommand RemoveComponent { get; } = null!;
    public DelegateCommand<object> SetSelectedComponentCommand { get; set; } = null!;
    public DelegateCommand<object> RemoveSpecificComponentCommand { get; set; } = null!;

    #endregion

    #region Public Methods

    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);
        this.InitializeViewModel();
    }

    public override void InitializeViewModel()
    {
        if (!IsInitialized && base.ActiveFarm is not null)
        {
            MyComponents.Clear();
            MyComponentItems.Clear();

            foreach (var component in base.ActiveFarm.Components)
            {
                this.MyComponents.Add(component);
                this.MyComponentItems.Add(new ComponentItemViewModel(component));
            }

            base.IsInitialized = true;

            var firstComponent = this.MyComponents.FirstOrDefault();
            this.SelectedComponent = firstComponent;
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Handles active farm changes and resets initialization state
    /// </summary>
    private void OnActiveFarmChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(GlobalSettings.ActiveFarm))
        {
            base.IsInitialized = false;
        }
    }

    /// <summary>
    /// Sets the selected component when a component card is clicked
    /// </summary>
    /// <param name="obj">The ComponentItemViewModel to select</param>
    private void OnSetSelectedComponentExecute(object obj)
    {
        if (!IsDisposed && obj is ComponentItemViewModel componentItem)
        {
            this.SelectedComponentItem = componentItem;
        }
    }

    /// <summary>
    /// Removes a specific component when the delete button on a card is clicked
    /// </summary>
    /// <param name="obj">The ComponentItemViewModel to remove</param>
    private void OnRemoveSpecificComponentExecute(object obj)
    {
        if (!IsDisposed && obj is ComponentItemViewModel componentItem)
        {
            try
            {
                var componentToRemove = componentItem.Component;

                // Remove from the local collections
                if (componentToRemove is not null)
                {
                    this.MyComponents.Remove(componentToRemove);
                }
                this.MyComponentItems.Remove(componentItem);
                componentItem.Cleanup(); // Cleanup the wrapper

                // Remove from the farm's Components collection
                if (componentToRemove is not null)
                {
                    base.ActiveFarm?.Components.Remove(componentToRemove);
                }

                // If the removed component was selected, select another component
                if (this.SelectedComponent is not null && Equals(this.SelectedComponent, componentToRemove))
                {
                    this.SelectedComponent = this.MyComponents.LastOrDefault();
                }

                if (this.MyComponents.Any() == false)
                {
                    this.ClearActiveView();
                }
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "Failed to remove specific component");
            }
        }
    }

    /// <summary>
    /// Updates the IsSelected property on all component items based on the currently selected component
    /// </summary>
    /// <param name="selectedComponent">The currently selected component</param>
    private void UpdateComponentSelectionStates(ComponentBase? selectedComponent)
    {
        foreach (var item in this.MyComponentItems)
        {
            item.IsSelected = Equals(item.Component, selectedComponent);
        }
        
        // Also update the SelectedComponentItem to match
        this.SelectedComponentItem = this.MyComponentItems.FirstOrDefault(x => Equals(x.Component, selectedComponent));
    }

    public void OnEditComponentsExecute()
    {
        var activeViews = this.RegionManager?.Regions[UiRegions.ContentRegion].ActiveViews;
        if (activeViews != null && activeViews.All(x => x.GetType() != typeof(ChooseComponentsView)))
        {
            this.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(ChooseComponentsView));
        }
    }

    public void OnResultsButtonClicked()
    {
        this.RegionManager?.RequestNavigate(UiRegions.SidebarRegion, nameof(ResultsSidebarView));
        this.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(ResultsSummaryView));
    }

    /// <summary>
    /// Phase 5 vertical slice: navigates straight to the GHG / carbon results view, which kicks
    /// off the ICBM analysis on the active farm via IFarmAnalysisService. Separate from
    /// OnResultsButtonClicked so the existing placeholder summary view stays reachable.
    /// </summary>
    public void OnRunGhgAnalysisButtonClicked()
    {
        this.RegionManager?.RequestNavigate(UiRegions.SidebarRegion, nameof(ResultsSidebarView));
        this.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(GHGResultsView));
    }

    public void OnRemoveComponentExecute()
    {
        if (this.SelectedComponent is not null)
        {
            // Store the component to remove since the SelectedComponent will be null after removal from the local collection
            var componentToRemove = this.SelectedComponent;
            var componentItemToRemove = this.MyComponentItems.FirstOrDefault(x => Equals(x.Component, componentToRemove));

            // Remove from the local collections
            this.MyComponents.Remove(componentToRemove);
            if (componentItemToRemove != null)
            {
                this.MyComponentItems.Remove(componentItemToRemove);
                componentItemToRemove.Cleanup(); // Cleanup the wrapper
            }
            
            // Remove from the farm's Components collection
            base.ActiveFarm?.Components.Remove(componentToRemove);

            this.SelectedComponent = this.MyComponents.LastOrDefault();

            if (this.MyComponents.Any() == false)
            {
                this.ClearActiveView();
            }
        }
    }

    private bool OnRemoveComponentCanExecute()
    {
        return this.SelectedComponent is not null;
    }

    private void OnComponentAddedEvent(ComponentBase componentBase)
    {
        var instanceType = componentBase.GetType();
        var instance = Activator.CreateInstance(instanceType) as ComponentBase;

        if (instance is null) return;

        _componentInitializationService?.Initialize(instance);

        this.MyComponents.Add(instance);
        this.MyComponentItems.Add(new ComponentItemViewModel(instance));
        this.SelectedComponent = instance;

        base.ActiveFarm?.Components.Add(instance);
        if (base.ActiveFarm is not null)
        {
            base.ActiveFarm.SelectedComponent = instance;
        }
    }

    public void OnOptionsExecute()
    {
        base.RegionManager?.RequestNavigate(UiRegions.SidebarRegion, nameof(Views.OptionsViews.OptionsView));
        base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.OptionsViews.SelectOptionView));
    }

    private void OnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName != null && e.PropertyName.Equals(nameof(this.SelectedComponent)))
        {
            System.Diagnostics.Debug.WriteLine($"{SelectedComponent} has been selected");
            var isInEditMode = this.RegionManager?.Regions[UiRegions.ContentRegion].ActiveViews.Any(x => x.GetType() == typeof(ChooseComponentsView)) ?? false;
            if (!isInEditMode)
            {
                
                this.NavigateToSelectedComponent();
            }
        }
    }

    private void OnEditingComponentsCompletedEvent()
    {
        this.NavigateToSelectedComponent();
    }

    private void ClearActiveView()
    {
        // Clear current view
        var activeView = this.RegionManager?.Regions[UiRegions.ContentRegion].ActiveViews.SingleOrDefault();
        if (activeView != null)
        {
            this.RegionManager?.Regions[UiRegions.ContentRegion].Deactivate(activeView);
            this.RegionManager?.Regions[UiRegions.ContentRegion].Remove(activeView);
        }
    }

    private void NavigateToSelectedComponent()
    {
        // When the user is finished editing components, navigate to the selected component
        if (this.SelectedComponent is not null)
        {
            var viewName = ComponentTypeToViewTypeMapper.GetViewName(this.SelectedComponent);

            var navigationParameters = new NavigationParameters { { GuiConstants.ComponentKey, this.SelectedComponent } };
            this.RegionManager?.RequestNavigate(UiRegions.ContentRegion, viewName, navigationParameters);
        }
    }

    #endregion

}