using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Avalonia.Media;
using H.Core.Enumerations;
using H.Avalonia.Views.ComponentViews;
using H.Avalonia.Views.ComponentViews.LandManagement.Field;
using H.Avalonia.ViewModels.ComponentViews.LandManagement.Rotation;
using H.Core.Factories;
using H.Core.Factories.Crops;
using H.Core.Factories.Fields;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services.CropColorService;
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

    private readonly ICropColorService? _cropColorService;

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
        ICropFactory cropFactory,
        ICropColorService cropColorService) : base(regionManager, eventAggregator, storageService, logger)
    {
        _cropFactory = cropFactory ?? throw new ArgumentNullException(nameof(cropFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fieldComponentService = fieldComponentService ?? throw new ArgumentNullException(nameof(fieldComponentService));
        _cropColorService = cropColorService ?? throw new ArgumentNullException(nameof(cropColorService));

        this.Construct();

        this.AddCropCommand = new DelegateCommand<object>(OnAddCropExecute, AddCropCanExecute);
        this.RemoveCropCommand = new DelegateCommand<object>(OnRemoveCropExecute, RemoveCropCanExecute);
        this.SetSelectedCropCommand = new DelegateCommand<object>(OnSetSelectedCropExecute);
        this.RemoveSpecificCropCommand = new DelegateCommand<object>(OnRemoveSpecificCropExecute);
        this.ToggleCoverCropCommand = new DelegateCommand<object>(OnToggleCoverCropExecute);
        this.MoveCropUpCommand = new DelegateCommand<object>(OnMoveCropUpExecute);
        this.MoveCropDownCommand = new DelegateCommand<object>(OnMoveCropDownExecute);
        this.SetSelectedCropFromCellCommand = new DelegateCommand<object>(OnSetSelectedCropFromCellExecute);
    }

    private void OnMoveCropUpExecute(object parameter)
    {
        if (parameter is ICropDto cropDto && SelectedFieldSystemComponentDto?.CropDtos is { } cropDtos)
        {
            var index = cropDtos.IndexOf(cropDto);
            if (index > 0)
            {
                cropDtos.Move(index, index - 1);
                UpdateSequenceNumbers(cropDtos);
            }
        }
    }

    private void OnMoveCropDownExecute(object parameter)
    {
        if (parameter is ICropDto cropDto && SelectedFieldSystemComponentDto?.CropDtos is { } cropDtos)
        {
            var index = cropDtos.IndexOf(cropDto);
            if (index >= 0 && index < cropDtos.Count - 1)
            {
                cropDtos.Move(index, index + 1);
                UpdateSequenceNumbers(cropDtos);
            }
        }
    }

    private static void UpdateSequenceNumbers(System.Collections.ObjectModel.ObservableCollection<ICropDto> cropDtos)
    {
        for (var i = 0; i < cropDtos.Count; i++)
        {
            cropDtos[i].SequenceNumber = i + 1;
        }
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
    /// Toggles cover crop on/off for a given crop DTO
    /// </summary>
    public DelegateCommand<object> ToggleCoverCropCommand { get; set; } = null!;

    /// <summary>
    /// Moves a crop one position earlier in the field history sequence.
    /// </summary>
    public DelegateCommand<object> MoveCropUpCommand { get; set; } = null!;

    /// <summary>
    /// Moves a crop one position later in the field history sequence.
    /// </summary>
    public DelegateCommand<object> MoveCropDownCommand { get; set; } = null!;

    /// <summary>
    /// Selects a crop from the preview grid cell
    /// </summary>
    public DelegateCommand<object> SetSelectedCropFromCellCommand { get; set; } = null!;

    /// <summary>
    /// Flat list of year/crop assignments for the field preview strip
    /// </summary>
    private ObservableCollection<YearCropAssignment> _fieldPreviewAssignments = new();
    public ObservableCollection<YearCropAssignment> FieldPreviewAssignments
    {
        get => _fieldPreviewAssignments;
        set => SetProperty(ref _fieldPreviewAssignments, value);
    }

    /// <summary>
    /// Whether the field preview strip should be visible
    /// </summary>
    public bool ShouldShowFieldPreview =>
        SelectedFieldSystemComponentDto != null &&
        SelectedFieldSystemComponentDto.CropDtos != null &&
        SelectedFieldSystemComponentDto.CropDtos.Count > 0;

    /// <summary>
    /// The selected <see cref="SelectedFieldSystemComponentDto"/>.
    /// On change, manages PropertyChanged subscriptions on every crop's CoverCropDto so
    /// edits to ANY row's cover crop refresh the Step 3 preview, not just the currently
    /// selected crop. Subscriptions are mirrored on the CropDtos collection so newly
    /// added crops are wired automatically.
    /// </summary>
    public IFieldComponentDto? SelectedFieldSystemComponentDto
    {
        get => _selectedFieldSystemComponentDto;
        set
        {
            var previous = _selectedFieldSystemComponentDto;
            if (SetProperty(ref _selectedFieldSystemComponentDto, value))
            {
                if (previous?.CropDtos is { } oldCrops)
                {
                    foreach (var crop in oldCrops)
                    {
                        UnsubscribeCoverCropChanges(crop);
                    }
                    if (oldCrops is INotifyCollectionChanged oldNcc)
                    {
                        oldNcc.CollectionChanged -= OnCropDtosCollectionChangedForCoverCrops;
                    }
                }

                if (value?.CropDtos is { } newCrops)
                {
                    foreach (var crop in newCrops)
                    {
                        SubscribeCoverCropChanges(crop);
                    }
                    if (newCrops is INotifyCollectionChanged newNcc)
                    {
                        newNcc.CollectionChanged -= OnCropDtosCollectionChangedForCoverCrops;
                        newNcc.CollectionChanged += OnCropDtosCollectionChangedForCoverCrops;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Subscribe to a crop's existing CoverCropDto so PropertyChanged fires the preview-refresh
    /// handler. No-op when the crop has no cover crop. Idempotent: safe to call repeatedly
    /// because we unsubscribe-before-subscribe.
    /// </summary>
    private void SubscribeCoverCropChanges(ICropDto crop)
    {
        if (crop?.CoverCropDto is INotifyPropertyChanged inpc)
        {
            inpc.PropertyChanged -= CoverCropDtoOnPropertyChanged;
            inpc.PropertyChanged += CoverCropDtoOnPropertyChanged;
        }
    }

    /// <summary>Unsubscribe a crop's CoverCropDto. No-op when there is no cover crop.</summary>
    private void UnsubscribeCoverCropChanges(ICropDto crop)
    {
        if (crop?.CoverCropDto is INotifyPropertyChanged inpc)
        {
            inpc.PropertyChanged -= CoverCropDtoOnPropertyChanged;
        }
    }

    /// <summary>
    /// When a crop is added to (or removed from) the field's CropDtos collection, wire/unwire
    /// its cover crop subscription so the preview refresh handler keeps catching all rows.
    /// </summary>
    private void OnCropDtosCollectionChangedForCoverCrops(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is ICropDto crop) SubscribeCoverCropChanges(crop);
            }
        }
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is ICropDto crop) UnsubscribeCoverCropChanges(crop);
            }
        }
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

                // If the newly selected crop doesn't have a cover crop, reset to main crop editing
                if (value?.HasCoverCrop != true)
                {
                    IsEditingCoverCrop = false;
                }
            }
        }
    }

    // ── Enum lists for crop tab ComboBoxes ──

    public IReadOnlyList<TillageType> TillageTypes { get; } = Enum.GetValues<TillageType>();
    public IReadOnlyList<HarvestMethods> HarvestMethodTypes { get; } = Enum.GetValues<HarvestMethods>();
    public IReadOnlyList<NitrogenFertilizerType> NitrogenFertilizerTypes { get; } = Enum.GetValues<NitrogenFertilizerType>();
    public IReadOnlyList<FertilizerBlends> FertilizerBlendTypes { get; } = Enum.GetValues<FertilizerBlends>();
    public IReadOnlyList<SoilReductionFactors> SoilReductionFactorTypes { get; } = Enum.GetValues<SoilReductionFactors>();
    public IReadOnlyList<ManureLocationSourceType> ManureLocationSourceTypes { get; } = Enum.GetValues<ManureLocationSourceType>();
    public IReadOnlyList<ManureAnimalSourceTypes> ManureAnimalSourceTypes { get; } = Enum.GetValues<ManureAnimalSourceTypes>();
    public IReadOnlyList<ManureApplicationTypes> ManureApplicationTypes { get; } = Enum.GetValues<ManureApplicationTypes>();
    public IReadOnlyList<ManureStateType> ManureStateTypes { get; } = Enum.GetValues<ManureStateType>();
    public IReadOnlyList<CoverCropTerminationType> CoverCropTerminationTypes { get; } = Enum.GetValues<CoverCropTerminationType>();

    private bool _isEditingCoverCrop;

    /// <summary>
    /// When true, the crop editor area shows cover crop properties instead of the main crop tabs.
    /// </summary>
    public bool IsEditingCoverCrop
    {
        get => _isEditingCoverCrop;
        set => SetProperty(ref _isEditingCoverCrop, value);
    }

    // ── Checklist category toggles ──

    private bool _isFertilizerActive = true;
    public bool IsFertilizerActive
    {
        get => _isFertilizerActive;
        set => SetProperty(ref _isFertilizerActive, value);
    }

    private bool _isManureActive;
    public bool IsManureActive
    {
        get => _isManureActive;
        set => SetProperty(ref _isManureActive, value);
    }

    private bool _isGrazingActive;
    public bool IsGrazingActive
    {
        get => _isGrazingActive;
        set => SetProperty(ref _isGrazingActive, value);
    }

    private bool _isSoilActive;
    public bool IsSoilActive
    {
        get => _isSoilActive;
        set => SetProperty(ref _isSoilActive, value);
    }

    private bool _isResidueActive;
    public bool IsResidueActive
    {
        get => _isResidueActive;
        set => SetProperty(ref _isResidueActive, value);
    }

    private bool _isEconomicsActive;
    public bool IsEconomicsActive
    {
        get => _isEconomicsActive;
        set => SetProperty(ref _isEconomicsActive, value);
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

        // Subscribe to crop collection changes for preview regeneration
        SubscribeToCropDtoChanges();
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

                    // Regenerate preview when start/end year changes
                    if (e.PropertyName == nameof(IFieldComponentDto.StartYear) ||
                        e.PropertyName == nameof(IFieldComponentDto.EndYear))
                    {
                        GenerateFieldPreview();
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

    private void OnToggleCoverCropExecute(object obj)
    {
        if (IsDisposed || obj is not ICropDto cropDto || _cropFactory is null) return;

        try
        {
            if (cropDto.HasCoverCrop)
            {
                // Remove cover crop
                if (_selectedFieldSystemComponent is not null && cropDto.CoverCropDto is not null)
                {
                    _fieldComponentService?.RemoveCoverCropFromSystem(_selectedFieldSystemComponent, cropDto.CoverCropDto);
                }
                cropDto.CoverCropDto = null;

                // Switch back to main crop editing since cover crop no longer exists
                IsEditingCoverCrop = false;
            }
            else
            {
                // Add cover crop
                var coverDto = _cropFactory.CreateCoverCropDto(cropDto.Year);
                cropDto.CoverCropDto = coverDto;

                if (_selectedFieldSystemComponent is not null)
                {
                    _fieldComponentService?.AddCoverCropToSystem(_selectedFieldSystemComponent, coverDto);
                }

                // Subscribe to cover crop property changes
                coverDto.PropertyChanged += CoverCropDtoOnPropertyChanged;
            }

            // Force UI to re-evaluate all SelectedCropDto.* bindings (e.g. HasCoverCrop)
            if (ReferenceEquals(cropDto, SelectedCropDto))
            {
                RaisePropertyChanged(nameof(SelectedCropDto));
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to toggle cover crop");
        }
    }

    private void CoverCropDtoOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (IsDisposed) return;

        if (sender is CropDto coverCropDto && !coverCropDto.HasErrors && _selectedFieldSystemComponent is not null)
        {
            try
            {
                var viewItem = _selectedFieldSystemComponent.CoverCrops
                    .SingleOrDefault(cc => cc.Guid.Equals(coverCropDto.Guid));

                if (viewItem is not null)
                {
                    _fieldComponentService?.TransferCropDtoToSystem(coverCropDto, viewItem);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to transfer cover crop DTO to system");
            }

            // Refresh the Step 3 preview so the cover-crop indicator on each year card
            // reflects the new cover crop type. Each cell's CropDto is a CLONE of the
            // source crop (built by CreateDtoFromDtoTemplate in GenerateFieldPreview),
            // so reference-equality matching against the source's CoverCropDto won't
            // work for in-place updates. Cover-crop type changes are infrequent user
            // interactions, so a full preview rebuild is acceptable.
            if (e.PropertyName == nameof(ICropDto.CropType)
                || e.PropertyName == nameof(ICropDto.SelectedCropTypeItem))
            {
                GenerateFieldPreview();
            }
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

                // Subscribe to cover crop changes if present
                if (this.SelectedCropDto.CoverCropDto is CropDto coverDto)
                {
                    coverDto.PropertyChanged -= CoverCropDtoOnPropertyChanged;
                    coverDto.PropertyChanged += CoverCropDtoOnPropertyChanged;
                }

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
        UnsubscribeFromCropDtoChanges();
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
        GenerateFieldPreview();
    }

    /// <summary>
    /// Handles clicking a cell in the field preview strip to select the corresponding crop DTO
    /// </summary>
    private void OnSetSelectedCropFromCellExecute(object obj)
    {
        if (IsDisposed || obj is not YearCropAssignment assignment) return;

        this.SelectedCropDto = assignment.CropDto;

        // Update selection state on preview cells
        foreach (var cell in FieldPreviewAssignments)
        {
            cell.IsSelected = ReferenceEquals(cell.CropDto, assignment.CropDto);
        }
    }

    /// <summary>
    /// Generates the flat list of year/crop assignments for the field preview strip
    /// </summary>
    private void GenerateFieldPreview()
    {
        var assignments = new ObservableCollection<YearCropAssignment>();

        if (SelectedFieldSystemComponentDto?.CropDtos == null || SelectedFieldSystemComponentDto.CropDtos.Count == 0)
        {
            FieldPreviewAssignments = assignments;
            RaisePropertyChanged(nameof(ShouldShowFieldPreview));
            return;
        }

        var cropDtos = SelectedFieldSystemComponentDto.CropDtos;
        var startYear = SelectedFieldSystemComponentDto.StartYear;
        var endYear = SelectedFieldSystemComponentDto.EndYear;

        if (startYear <= 0 || endYear <= 0 || endYear < startYear)
        {
            FieldPreviewAssignments = assignments;
            RaisePropertyChanged(nameof(ShouldShowFieldPreview));
            return;
        }

        var totalYears = endYear - startYear + 1;

        for (int yearIndex = 0; yearIndex < totalYears; yearIndex++)
        {
            int year = startYear + yearIndex;
            int cropIndex = yearIndex % cropDtos.Count;
            var sourceCrop = cropDtos[cropIndex];

            // Create a cloned DTO for this cell
            ICropDto cellCropDto;
            if (_cropFactory is not null)
            {
                cellCropDto = (ICropDto)_cropFactory.CreateDtoFromDtoTemplate(sourceCrop);
                cellCropDto.Year = year;
            }
            else
            {
                cellCropDto = sourceCrop;
            }

            var assignment = new YearCropAssignment
            {
                Year = year.ToString(),
                CropType = sourceCrop.CropType,
                CropDto = cellCropDto,
                CropDisplay = _cropColorService?.GetCropDisplayName(sourceCrop.CropType) ?? sourceCrop.CropType.ToString(),
                CropBackground = _cropColorService is not null
                    ? Brush.Parse(_cropColorService.GetCropColorHex(sourceCrop.CropType))
                    : Brush.Parse("#F5F5F5"),
                IsSelected = false,
                CoverCropDisplay = sourceCrop.HasCoverCrop && sourceCrop.CoverCropDto != null
                    ? (_cropColorService?.GetCropDisplayName(sourceCrop.CoverCropDto.CropType) ?? sourceCrop.CoverCropDto.CropType.ToString())
                    : null,
            };

            assignments.Add(assignment);
        }

        FieldPreviewAssignments = assignments;
        RaisePropertyChanged(nameof(ShouldShowFieldPreview));
    }

    /// <summary>
    /// Subscribes to CropDtos collection changes and individual DTO property changes for preview regeneration
    /// </summary>
    private void SubscribeToCropDtoChanges()
    {
        if (SelectedFieldSystemComponentDto?.CropDtos == null) return;

        SelectedFieldSystemComponentDto.CropDtos.CollectionChanged += CropDtosCollectionChangedForPreview;

        foreach (var cropDto in SelectedFieldSystemComponentDto.CropDtos)
        {
            cropDto.PropertyChanged += CropDtoPropertyChangedForPreview;
        }
    }

    /// <summary>
    /// Unsubscribes from CropDtos collection changes for preview regeneration
    /// </summary>
    private void UnsubscribeFromCropDtoChanges()
    {
        if (SelectedFieldSystemComponentDto?.CropDtos == null) return;

        SelectedFieldSystemComponentDto.CropDtos.CollectionChanged -= CropDtosCollectionChangedForPreview;

        foreach (var cropDto in SelectedFieldSystemComponentDto.CropDtos)
        {
            cropDto.PropertyChanged -= CropDtoPropertyChangedForPreview;
        }
    }

    private void CropDtosCollectionChangedForPreview(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (IsDisposed) return;

        // Subscribe/unsubscribe from individual items
        if (e.NewItems != null)
        {
            foreach (ICropDto item in e.NewItems)
            {
                item.PropertyChanged += CropDtoPropertyChangedForPreview;
            }
        }

        if (e.OldItems != null)
        {
            foreach (ICropDto item in e.OldItems)
            {
                item.PropertyChanged -= CropDtoPropertyChangedForPreview;
            }
        }

        GenerateFieldPreview();
    }

    private void CropDtoPropertyChangedForPreview(object? sender, PropertyChangedEventArgs e)
    {
        if (IsDisposed) return;

        // Persist the property change to the underlying model so edits made on Step 2
        // timeline cards (e.g. crop type changes) survive navigation away from the field
        // component. Without this, CropDtoOnPropertyChanged is only subscribed to the
        // currently SelectedCropDto, so changes on other timeline cards never reach the
        // model and are lost when the view is re-initialized from the model on return.
        if (sender is CropDto cropDto && !cropDto.HasErrors && _selectedFieldSystemComponent is not null)
        {
            try
            {
                var viewItem = _fieldComponentService?.GetCropViewItemFromDto(cropDto, _selectedFieldSystemComponent);
                if (viewItem is not null)
                {
                    _fieldComponentService?.TransferCropDtoToSystem(cropDto, viewItem);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to persist timeline crop DTO change to the model");
            }
        }

        // Regenerate preview when crop type changes
        if (e.PropertyName == nameof(ICropDto.CropType) || e.PropertyName == nameof(ICropDto.HasCoverCrop))
        {
            GenerateFieldPreview();
        }
    }

    #endregion
}