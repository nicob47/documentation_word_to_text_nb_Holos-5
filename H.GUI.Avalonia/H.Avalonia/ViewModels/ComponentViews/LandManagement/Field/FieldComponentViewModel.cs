using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using H.Avalonia.Views.ComponentViews;
using H.Avalonia.Views.ComponentViews.LandManagement.Field;
using H.Core.Factories;
using H.Core.Factories.Crops;
using H.Core.Factories.Fields;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services.LandManagement.Fields;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Field;

/// <summary>
/// The view model that is used with a <see cref="FieldComponentView"/>.
/// </summary>
public class FieldComponentViewModel : ViewModelBase
{
    #region Fields

    /// <summary>
    /// The selected field
    /// </summary>
    private FieldSystemComponent? _selectedFieldSystemComponent;

    /// <summary>
    /// The field DTO that is bound to the view and is based on the values from the <see cref="_selectedFieldSystemComponent"/> model object
    /// </summary>
    private IFieldComponentDto? _selectedFieldSystemComponentDto;

    /// <summary>
    /// The selected crop
    /// </summary>
    private CropViewItem? _selectedCropViewItem;

    /// <summary>
    /// The crop DTO that is bound to the view and is based on the values from the <see cref="_selectedCropViewItem"/>
    /// </summary>
    private ICropDto? _selectedCropDto;

    /// <summary>
    /// A service class to perform domain/business logic on field and crop DTOs/objects
    /// </summary>
    private readonly IFieldComponentService? _fieldComponentService;

    /// <summary>
    /// A logger instance
    /// </summary>
    private readonly ILogger? _logger;

    private readonly ICropFactory? _cropFactory;

    #endregion

    #region Constructors

    private void Construct()
    {
    }

    public FieldComponentViewModel()
    {
        this.Construct();
    }

    public FieldComponentViewModel(
        IRegionManager regionManager, 
        IEventAggregator eventAggregator, 
        IStorageService storageService,
        IFieldComponentService fieldComponentService,
        ILogger logger,
        ICropFactory cropFactory) : base(regionManager, eventAggregator, storageService, logger)
    {
        _cropFactory = cropFactory ?? throw new ArgumentNullException(nameof(cropFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fieldComponentService = fieldComponentService ?? throw new ArgumentNullException(nameof(fieldComponentService));

        this.Construct();

        this.AddCropCommand = new DelegateCommand<object>(OnAddCropExecute, AddCropCanExecute);
        this.RemoveCropCommand = new DelegateCommand<object>(OnRemoveCropExecute, RemoveCropCanExecute);
        this.SetSelectedCropCommand = new DelegateCommand<object>(OnSetSelectedCropExecute);
        this.RemoveSpecificCropCommand = new DelegateCommand<object>(OnRemoveSpecificCropExecute);
    }

    #endregion

    #region Properties

    /// <summary>
    /// Responsible for handling the addition of new crops
    /// </summary>
    public DelegateCommand<object> AddCropCommand { get; set; } = null!;

    /// <summary>
    /// Responsible for handling the removal of crops
    /// </summary>
    public DelegateCommand<object> RemoveCropCommand { get; set; } = null!;

    /// <summary>
    /// Responsible for setting the selected crop when a card is clicked in the timeline view
    /// </summary>
    public DelegateCommand<object> SetSelectedCropCommand { get; set; } = null!;

    /// <summary>
    /// Responsible for removing a specific crop from the timeline cards
    /// </summary>
    public DelegateCommand<object> RemoveSpecificCropCommand { get; set; } = null!;

    /// <summary>
    /// The selected <see cref="SelectedFieldSystemComponentDto"/>
    /// </summary>
    public IFieldComponentDto? SelectedFieldSystemComponentDto
    {
        get => _selectedFieldSystemComponentDto;
        set => SetProperty(ref _selectedFieldSystemComponentDto, value);
    }

    /// <summary>
    /// The selected <see cref="CropDto"/>
    /// </summary>
    public ICropDto? SelectedCropDto
    {
        get => _selectedCropDto;
        set 
        { 
            if (SetProperty(ref _selectedCropDto, value))
            {
                // Update selection states for all crops
                UpdateCropSelectionStates(value);
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// When the user navigates to a <see cref="FieldSystemComponent"/>, we must initialize the component and any DTOs
    /// that will be used with the view
    /// </summary>
    /// <param name="component">The <see cref="FieldSystemComponent"/> to display to the user</param>
    public override void InitializeViewModel(ComponentBase component)
    {
        if (component is not FieldSystemComponent fieldSystemComponent)
        {
            return;
        }

        base.InitializeViewModel(fieldSystemComponent);

        // Clean up any existing subscriptions before setting up new ones
        CleanupResources();

        this.PropertyChanged += ViewModelOnPropertyChanged;

        InitializeFieldComponent(fieldSystemComponent);
        InitializeSelectedCrop();

        FinalizeInitialization();
    }

    /// <summary>
    /// A first point of entry to this class (after the constructor is called). Get a reference to the <see cref="FieldSystemComponent"/> the
    /// user selected from the <see cref="MyComponentsView"/>.
    /// </summary>
    /// <param name="navigationContext">An object holding a reference to the selected <see cref="FieldSystemComponent"/></param>
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.ContainsKey(GuiConstants.ComponentKey))
        {
            var parameter = navigationContext.Parameters[GuiConstants.ComponentKey];
            if (parameter is FieldSystemComponent fieldSystemComponent)
            {
                this.InitializeViewModel(fieldSystemComponent);
            }
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Initializes the selected crop based on existing crops or creates a new one
    /// </summary>
    private void InitializeSelectedCrop()
    {
        var selectedCrop = DetermineSelectedCrop();

        if (selectedCrop != null)
        {
            this.SelectedCropDto = selectedCrop;
        }
        else
        {
            // There are no crops associated with this field, add a new one
            this.AddCropDto();
        }

        // Hold a reference to the selected crop view item
        UpdateSelectedCropViewItem();
    }

    /// <summary>
    /// Initializes the field component and sets up event handlers
    /// </summary>
    /// <param name="fieldSystemComponent">The field component to initialize</param>
    private void InitializeFieldComponent(FieldSystemComponent? fieldSystemComponent)
    {
        if (fieldSystemComponent is null)
        {
            return;
        }

        // Hold a reference to the selected field system object
        _selectedFieldSystemComponent = fieldSystemComponent;

        // Build a DTO to represent the model/domain object
        var fieldComponentDto = _fieldComponentService?.TransferToFieldComponentDto(_selectedFieldSystemComponent);
        if (fieldComponentDto is null) return;

        // Listen for changes on the DTO so we can validate user input before assigning values to the model
        fieldComponentDto.PropertyChanged += FieldSystemComponentDtoOnPropertyChanged;

        // Assign the DTO to the property that is bound to the view
        this.SelectedFieldSystemComponentDto = fieldComponentDto;
    }

    /// <summary>
    /// A user can add a crop under any condition
    /// </summary>
    private bool AddCropCanExecute(object arg)
    {
        return !IsDisposed && _selectedFieldSystemComponent is not null;
    }

    /// <summary>
    /// Sets the selected crop when a timeline card is clicked
    /// </summary>
    /// <param name="obj">The crop DTO to select</param>
    private void OnSetSelectedCropExecute(object obj)
    {
        if (!IsDisposed && obj is ICropDto cropDto)
        {
            // Update the selected crop
            this.SelectedCropDto = cropDto;
            
            // Update IsSelected property on all crops
            UpdateCropSelectionStates(cropDto);
        }
    }

    /// <summary>
    /// Removes a specific crop when the delete button on a card is clicked
    /// </summary>
    /// <param name="obj">The crop DTO to remove</param>
    private void OnRemoveSpecificCropExecute(object obj)
    {
        if (!IsDisposed && obj is ICropDto cropDto)
        {
            try
            {
                // Remove the specific crop from the collection
                this.SelectedFieldSystemComponentDto?.CropDtos?.Remove(cropDto);

                // Ensure consecutive ordering (by year) of all crops now that one has been removed
                if (this.SelectedFieldSystemComponentDto?.CropDtos != null)
                {
                    _fieldComponentService?.ResetAllYears(this.SelectedFieldSystemComponentDto.CropDtos);
                }

                // Update command states
                this.RemoveCropCommand?.RaiseCanExecuteChanged();

                // Remove from the system
                if (_selectedFieldSystemComponent is not null)
                {
                    _fieldComponentService?.RemoveCropFromSystem(_selectedFieldSystemComponent, cropDto);
                }

                // If the removed crop was selected, clear or select another crop
                if (this.SelectedCropDto == cropDto)
                {
                    this.SelectedCropDto = this.SelectedFieldSystemComponentDto?.CropDtos?.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to remove specific crop");
            }
        }
    }

    /// <summary>
    /// Adds a new <see cref="CropDto"/> to the <see cref="SelectedFieldSystemComponentDto"/> property
    /// </summary>
    private void OnAddCropExecute(object obj)
    {
        if (!IsDisposed)
        {
            this.AddCropDto();
        }
    }

    /// <summary>
    /// Some property on the <see cref="SelectedFieldSystemComponentDto"/> has changed. Check if we need to validate any user
    /// input before assigning the value on to the associated <see cref="FieldSystemComponent"/> domain object.
    /// </summary>
    private void FieldSystemComponentDtoOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        if (sender is FieldSystemComponentDto fieldSystemComponentDto)
        {
            /*
             * Before assigning values from the bound DTOs, check for any validation errors. If there are any validation errors
             * we should not proceed with the transfer of user input from the DTO to the model until the validation errors are fixed
             */

            if (!fieldSystemComponentDto.HasErrors)
            {
                try
                {
                    // A property on the DTO has been changed by the user, assign the new value to the system object after any unit conversion (if necessary)
                    if (_selectedFieldSystemComponent is not null)
                    {
                        _fieldComponentService?.TransferFieldDtoToSystem(fieldSystemComponentDto, _selectedFieldSystemComponent);
                    }
                }
                catch (Exception exception)
                {
                    _logger?.LogError(exception, "Failed to transfer field component DTO to domain object");
                }
            }
        }
    }

    /// <summary>
    /// Some property on the <see cref="SelectedCropDto"/> has changed. Check if we need to validate any user
    /// input before assigning the value on to the associated <see cref="CropViewItem"/>
    /// </summary>
    private void CropDtoOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsDisposed) return;

        if (sender is CropDto cropDto)
        {
            /*
             * Before assigning values from the bound DTOs, check for any validation errors. If there are any validation errors
             * we should not proceed with the transfer of user input from the DTO to the model until the validation errors are fixed
             */

            if (!cropDto.HasErrors)
            {
                try
                {
                    CropViewItem? viewItem = _selectedFieldSystemComponent is not null
                        ? _fieldComponentService?.GetCropViewItemFromDto(cropDto, _selectedFieldSystemComponent)
                        : null;

                    if (viewItem is not null)
                    {
                        // Persist the changes to the system
                        _fieldComponentService?.TransferCropDtoToSystem(cropDto, viewItem);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to transfer crop DTO to system");
                }
            }
        }
    }

    /// <summary>
    /// Used to indicate to the GUI if the command button should be enabled or disabled
    /// </summary>
    private bool RemoveCropCanExecute(object arg)
    {
        return !IsDisposed && this.SelectedFieldSystemComponentDto?.CropDtos?.Any() == true;
    }

    private void OnRemoveCropExecute(object obj)
    {
        if (IsDisposed || this.SelectedCropDto == null) return;

        try
        {
            // Keep a reference to the dto to remove before removing it from the collection
            var dtoToRemove = this.SelectedCropDto;

            this.SelectedFieldSystemComponentDto?.CropDtos?.Remove(dtoToRemove);

            // Ensure consecutive ordering (by year) of all crops now that one has been removed
            if (this.SelectedFieldSystemComponentDto?.CropDtos != null)
            {
                _fieldComponentService?.ResetAllYears(this.SelectedFieldSystemComponentDto.CropDtos);
            }

            this.RemoveCropCommand?.RaiseCanExecuteChanged();

            if (_selectedFieldSystemComponent is not null)
            {
                _fieldComponentService?.RemoveCropFromSystem(_selectedFieldSystemComponent, dtoToRemove);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to remove crop");
        }
    }

    private void ViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsDisposed) return;

        if (e.PropertyName != null && e.PropertyName.Equals(nameof(SelectedCropDto)))
        {
            RemoveCropCommand?.RaiseCanExecuteChanged();

            if (this.SelectedCropDto != null)
            {
                // Clean up previous event handler before adding new one
                this.SelectedCropDto.PropertyChanged -= CropDtoOnPropertyChanged;
                this.SelectedCropDto.PropertyChanged += CropDtoOnPropertyChanged;

                try
                {
                    if (_selectedFieldSystemComponent is not null)
                    {
                        _selectedCropViewItem = _fieldComponentService?.GetCropViewItemFromDto(this.SelectedCropDto, _selectedFieldSystemComponent);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to get crop view item from DTO");
                }
            }
        }
    }


    /// <summary>
    /// Determines which crop should be selected based on saved state and existing crops
    /// </summary>
    /// <returns>The crop DTO to select, or null if no existing crops should be selected</returns>
    private ICropDto? DetermineSelectedCrop()
    {
        // If there are no crops associated with the field, return null to trigger AddCropDto
        if (this.SelectedFieldSystemComponentDto?.CropDtos?.Any() != true)
            return null;

        FieldComponentUIState? savedState = _selectedFieldSystemComponent is not null
            ? _fieldComponentService?.GetUIState(_selectedFieldSystemComponent.Guid)
            : null;

        // First, try to restore the previously selected crop from saved state
        var selectedCrop = TryRestoreFromSavedState(savedState);

        // If no saved state or saved crop not found, use default logic
        return selectedCrop ?? GetDefaultSelectedCrop();
    }

    /// <summary>
    /// Adds a new <see cref="CropDto"/> to the <see cref="SelectedFieldSystemComponentDto"/> property
    /// Used in both the <see cref="OnAddCropExecute(object)"/> and in the <see cref="InitializeViewModel(ComponentBase)"/> methods
    /// </summary>
    private void AddCropDto()
    {
        if (IsDisposed || base.ActiveFarm is null || this.SelectedFieldSystemComponentDto is null) return;

        try
        {
            if (_cropFactory is null) return;
            var dto = _cropFactory.CreateDto(base.ActiveFarm);

            _fieldComponentService?.InitializeCropDto(this.SelectedFieldSystemComponentDto, dto);

            // Use this as the new selected instance
            this.SelectedCropDto = dto;

            // If disabled before, enable this command now so that the user can remove a DTO
            this.RemoveCropCommand?.RaiseCanExecuteChanged();

            if (_selectedFieldSystemComponent is not null)
            {
                _fieldComponentService?.AddCropDtoToSystem(_selectedFieldSystemComponent, dto);
                _selectedCropViewItem = _fieldComponentService?.GetCropViewItemFromDto(dto, _selectedFieldSystemComponent);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to add crop DTO");
        }
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Override CleanupResources to provide FieldComponentViewModel-specific cleanup logic
    /// </summary>
    protected override void CleanupResources()
    {
        // Save current UI state before cleanup
        SaveCurrentUIState();

        // Clean up FieldComponentViewModel-specific event handlers
        this.PropertyChanged -= ViewModelOnPropertyChanged;

        if (_selectedFieldSystemComponentDto is FieldSystemComponentDto fieldDto)
        {
            fieldDto.PropertyChanged -= FieldSystemComponentDtoOnPropertyChanged;
        }

        if (_selectedCropDto != null)
        {
            _selectedCropDto.PropertyChanged -= CropDtoOnPropertyChanged;
        }

        // Call base class cleanup
        base.CleanupResources();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Saves the current UI state to preserve across ViewModel disposal cycles
    /// </summary>
    private void SaveCurrentUIState()
    {
        if (_selectedFieldSystemComponent is not null && _fieldComponentService is not null)
        {
            try
            {
                var additionalState = new Dictionary<string, object>();
                
                // You can add other UI state here, such as:
                // - Scroll positions
                // - Expanded/collapsed sections
                // - Tab selections
                // - Filter states
                // additionalState["ScrollPosition"] = someScrollPosition;
                // additionalState["ExpandedSection"] = someExpandedState;

                _fieldComponentService?.SaveUIState(
                    _selectedFieldSystemComponent.Guid,
                    _selectedCropDto?.Guid,
                    additionalState);

                _logger?.LogDebug("Saved UI state for field component {ComponentName}", _selectedFieldSystemComponent.Name);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to save UI state for field component");
                // Don't rethrow - state saving shouldn't break disposal
            }
        }
    }

    /// <summary>
    /// Attempts to restore the selected crop from saved UI state
    /// </summary>
    /// <param name="savedState">The saved UI state</param>
    /// <returns>The restored crop DTO, or null if not found</returns>
    private ICropDto? TryRestoreFromSavedState(FieldComponentUIState? savedState)
    {
        if (savedState?.SelectedCropGuid.HasValue == true)
        {
            var restoredCrop = this.SelectedFieldSystemComponentDto?.CropDtos?
                .FirstOrDefault(dto => dto.Guid == savedState.SelectedCropGuid.Value);

            if (restoredCrop != null)
            {
                _logger?.LogDebug("Restored selected crop from saved state: {CropGuid}", savedState.SelectedCropGuid.Value);
                return restoredCrop;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the default selected crop using fallback logic
    /// </summary>
    /// <returns>The default crop DTO to select</returns>
    private ICropDto? GetDefaultSelectedCrop()
    {
        // Check if we can restore last selected item (legacy fallback)
        if (this.SelectedCropDto != null &&
            this.SelectedFieldSystemComponentDto?.CropDtos?.Contains(this.SelectedCropDto) == true)
        {
            return this.SelectedCropDto;
        }

        // Default to first crop
        return this.SelectedFieldSystemComponentDto?.CropDtos?.FirstOrDefault();
    }

    /// <summary>
    /// Updates the selected crop view item based on the current SelectedCropDto
    /// </summary>
    private void UpdateSelectedCropViewItem()
    {
        if (this.SelectedCropDto is not null && _selectedFieldSystemComponent is not null)
        {
            _selectedCropViewItem = _fieldComponentService?.GetCropViewItemFromDto(
                this.SelectedCropDto,
                _selectedFieldSystemComponent);
        }
    }

    /// <summary>
    /// Updates the IsSelected property on all crops based on the currently selected crop
    /// </summary>
    /// <param name="selectedCrop">The currently selected crop DTO</param>
    private void UpdateCropSelectionStates(ICropDto? selectedCrop)
    {
        if (this.SelectedFieldSystemComponentDto?.CropDtos != null)
        {
            foreach (var crop in this.SelectedFieldSystemComponentDto.CropDtos)
            {
                crop.IsSelected = crop == selectedCrop;
            }
        }
    }

    /// <summary>
    /// Finalizes the initialization process
    /// </summary>
    private void FinalizeInitialization()
    {
        this.AddCropCommand.RaiseCanExecuteChanged();
    }

    #endregion
}