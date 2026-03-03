using H.Core.Factories.Animals.Dairy;
using H.Core.Factories.Animals;
using H.Core.Models;
using H.Core.Models.Animals.Dairy;
using H.Core.Services.Animals.Dairy;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Prism.Regions;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using H.Core.Enumerations;
using Prism.Commands;

namespace H.Avalonia.ViewModels.ComponentViews.Dairy
{
    /// <summary>
    /// View model for the Dairy Component feature, which allows users to manage dairy cattle operations
    /// including herd composition, lactation stages, and production parameters.
    /// </summary>
    public class DairyComponentViewModel : ViewModelBase
    {
        #region Fields

        /// <summary>
        /// Service for managing dairy component operations and data transfer
        /// </summary>
        private readonly IDairyComponentService? _dairyComponentService;

        /// <summary>
        /// The domain model object representing the dairy component being edited
        /// </summary>
        private DairyComponent? _selectedDairyComponent;

        /// <summary>
        /// Data transfer object containing dairy component parameters (herd overview, lactation stages, etc.)
        /// This DTO is bound to the view and includes validation logic
        /// </summary>
        private IDairyComponentDto? _selectedDairyComponentDto;

        /// <summary>
        /// Tracks which herd stage card is currently selected (Calf, Heifer, Lactating, or Dry)
        /// </summary>
        private string? _selectedHerdStage;

        /// <summary>
        /// Indicates if the Calf card is selected
        /// </summary>
        private bool _isCalfSelected;

        /// <summary>
        /// Indicates if the Heifer card is selected
        /// </summary>
        private bool _isHeiferSelected;

        /// <summary>
        /// Indicates if the Lactating card is selected
        /// </summary>
        private bool _isLactatingSelected;

        /// <summary>
        /// Indicates if the Dry card is selected
        /// </summary>
        private bool _isDrySelected;

        /// <summary>
        /// Collection of available manure state types for dropdown selection
        /// </summary>
        private IEnumerable<ManureStateType>? _manureStateTypes;

        /// <summary>
        /// Collection of available housing types for dropdown selection
        /// </summary>
        private IEnumerable<HousingType>? _housingTypes;
        
        /// <summary>
        /// Collection of available bedding material types for dropdown selection
        /// </summary>
        private IEnumerable<BeddingMaterialType>? _beddingMaterialTypes;

        /// <summary>
        /// Collection of available diet additive types for dropdown selection
        /// </summary>
        private IEnumerable<DietAdditiveType>? _dietAdditiveTypes;

        /// <summary>
        /// Collection of available diet types for dropdown selection
        /// </summary>
        private IEnumerable<DietType>? _availableDietTypes;

        /// <summary>
        /// The currently selected management practice for the calf stage
        /// </summary>
        private ManagementPeriodDto? _selectedCalfPractice;

        /// <summary>
        /// The currently selected management practice for the heifer stage
        /// </summary>
        private ManagementPeriodDto? _selectedHeiferPractice;

        /// <summary>
        /// The currently selected management practice for the lactating stage
        /// </summary>
        private ManagementPeriodDto? _selectedLactatingPractice;

        /// <summary>
        /// The currently selected management practice for the dry stage
        /// </summary>
        private ManagementPeriodDto? _selectedDryPractice;

        /// <summary>
        /// The currently selected calf group for management configuration
        /// </summary>
        private DairyPopulationGroup? _selectedCalfGroup;

        /// <summary>
        /// The currently selected heifer group for management configuration
        /// </summary>
        private DairyPopulationGroup? _selectedHeiferGroup;

        /// <summary>
        /// The currently selected lactating group for management configuration
        /// </summary>
        private DairyPopulationGroup? _selectedLactatingGroup;

        /// <summary>
        /// The currently selected dry group for management configuration
        /// </summary>
        private DairyPopulationGroup? _selectedDryGroup;

        #endregion

        #region Constructors

        /// <summary>
        /// Default parameterless constructor for design-time support and testing.
        /// Initializes collections and commands but does not inject dependencies.
        /// </summary>
        public DairyComponentViewModel()
        {
            this.Construct();
        }

        /// <summary>
        /// Primary constructor with dependency injection for runtime use.
        /// Validates all injected dependencies and initializes the view model.
        /// </summary>
        /// <param name="regionManager">Prism region manager for navigation between views</param>
        /// <param name="eventAggregator">Event aggregator for pub/sub messaging between components</param>
        /// <param name="storageService">Service for accessing application storage and active farm data</param>
        /// <param name="dairyComponentService">Service for dairy component operations and data transfer</param>
        /// <param name="logger">Logger instance for diagnostic and error logging</param>
        /// <exception cref="ArgumentNullException">Thrown if any required dependency is null</exception>
        public DairyComponentViewModel(
            IRegionManager regionManager,
            IEventAggregator eventAggregator,
            IStorageService storageService,
            IDairyComponentService dairyComponentService,
            ILogger logger) : base(regionManager, eventAggregator, storageService, logger)
        {
            // Validate and store dairy component service dependency
            _dairyComponentService = dairyComponentService ?? throw new ArgumentNullException(nameof(dairyComponentService));

            // Initialize collections and commands
            this.Construct();
        }

        #endregion

        #region Properties

        /// <summary>
        /// The DTO representing the dairy component's herd overview and production parameters.
        /// This property is bound to the view and includes validation logic for user input.
        /// 
        /// ARCHITECTURE NOTE:
        /// This DTO contains AnimalGroupDtos which the view should bind to directly via:
        /// {Binding SelectedDairyComponentDto.AnimalGroupDtos}
        /// 
        /// This ensures proper validation and data flow through the DTO layer.
        /// The collection is guaranteed to be non-null (initialized in AnimalComponentDto constructor).
        /// </summary>
        public IDairyComponentDto? SelectedDairyComponentDto
        {
            get => _selectedDairyComponentDto;
            set
            {
                // Unsubscribe from old DTO if it exists
                if (_selectedDairyComponentDto != null)
                {
                    _selectedDairyComponentDto.PropertyChanged -= OnDairyComponentDtoPropertyChanged;
                }

                SetProperty(ref _selectedDairyComponentDto, value);

                // Subscribe to new DTO
                if (_selectedDairyComponentDto != null)
                {
                    _selectedDairyComponentDto.PropertyChanged += OnDairyComponentDtoPropertyChanged;
                }
            }
        }

        /// <summary>
        /// Gets or sets the currently selected herd stage
        /// </summary>
        public string? SelectedHerdStage
        {
            get => _selectedHerdStage;
            set => SetProperty(ref _selectedHerdStage, value);
        }

        /// <summary>
        /// Gets or sets whether the Calf card is selected
        /// </summary>
        public bool IsCalfSelected
        {
            get => _isCalfSelected;
            set
            {
                if (SetProperty(ref _isCalfSelected, value))
                {
                    RaisePropertyChanged(nameof(IsAnyStageSelected));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the Heifer card is selected
        /// </summary>
        public bool IsHeiferSelected
        {
            get => _isHeiferSelected;
            set
            {
                if (SetProperty(ref _isHeiferSelected, value))
                {
                    RaisePropertyChanged(nameof(IsAnyStageSelected));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the Lactating card is selected
        /// </summary>
        public bool IsLactatingSelected
        {
            get => _isLactatingSelected;
            set
            {
                if (SetProperty(ref _isLactatingSelected, value))
                {
                    RaisePropertyChanged(nameof(IsAnyStageSelected));
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the Dry card is selected
        /// </summary>
        public bool IsDrySelected
        {
            get => _isDrySelected;
            set
            {
                if (SetProperty(ref _isDrySelected, value))
                {
                    RaisePropertyChanged(nameof(IsAnyStageSelected));
                }
            }
        }

        /// <summary>
        /// Gets or sets the collection of available manure state types
        /// </summary>
        public IEnumerable<ManureStateType>? ManureStateTypes
        {
            get => _manureStateTypes;
            set => SetProperty(ref _manureStateTypes, value);
        }

        /// <summary>
        /// Gets or sets the collection of available housing types
        /// </summary>
        public IEnumerable<HousingType>? HousingTypes
        {
            get => _housingTypes;
            set => SetProperty(ref _housingTypes, value);
        }

        /// <summary>
        /// Gets or sets the collection of available bedding material types for dairy animals
        /// </summary>
        public IEnumerable<BeddingMaterialType>? BeddingMaterialTypes
        {
            get => _beddingMaterialTypes;
            set => SetProperty(ref _beddingMaterialTypes, value);
        }

        /// <summary>
        /// Gets or sets the collection of available diet additive types
        /// </summary>
        public IEnumerable<DietAdditiveType>? DietAdditiveTypes
        {
            get => _dietAdditiveTypes;
            set => SetProperty(ref _dietAdditiveTypes, value);
        }

        /// <summary>
        /// Gets or sets the collection of available diet types for dairy animals
        /// </summary>
        public IEnumerable<DietType>? AvailableDietTypes
        {
            get => _availableDietTypes;
            set => SetProperty(ref _availableDietTypes, value);
        }

        /// <summary>
        /// Gets or sets the currently selected management practice for the calf stage
        /// </summary>
        public ManagementPeriodDto? SelectedCalfPractice
        {
            get => _selectedCalfPractice;
            set => SetProperty(ref _selectedCalfPractice, value);
        }

        /// <summary>
        /// Gets or sets the currently selected management practice for the heifer stage
        /// </summary>
        public ManagementPeriodDto? SelectedHeiferPractice
        {
            get => _selectedHeiferPractice;
            set => SetProperty(ref _selectedHeiferPractice, value);
        }

        /// <summary>
        /// Gets or sets the currently selected management practice for the lactating stage
        /// </summary>
        public ManagementPeriodDto? SelectedLactatingPractice
        {
            get => _selectedLactatingPractice;
            set => SetProperty(ref _selectedLactatingPractice, value);
        }

        /// <summary>
        /// Gets or sets the currently selected management practice for the dry stage
        /// </summary>
        public ManagementPeriodDto? SelectedDryPractice
        {
            get => _selectedDryPractice;
            set => SetProperty(ref _selectedDryPractice, value);
        }

        /// <summary>
        /// Gets whether any lifecycle stage card is currently selected.
        /// Used to control visibility of Step 3 (Lifecycle Configuration).
        /// </summary>
        public bool IsAnyStageSelected => IsCalfSelected || IsHeiferSelected || IsLactatingSelected || IsDrySelected;
        
        /// <summary>
        /// Command to add a new population group to the calf stage
        /// </summary>
        public ICommand AddCalfGroupCommand { get; private set; } = null!;

        /// <summary>
        /// Command to add a new population group to the heifer stage
        /// </summary>
        public ICommand AddHeiferGroupCommand { get; private set; } = null!;

        /// <summary>
        /// Command to add a new population group to the lactating stage
        /// </summary>
        public ICommand AddLactatingGroupCommand { get; private set; } = null!;

        /// <summary>
        /// Command to add a new population group to the dry stage
        /// </summary>
        public ICommand AddDryGroupCommand { get; private set; } = null!;

        /// <summary>
        /// Command to remove a population group from any stage
        /// </summary>
        public ICommand RemoveGroupCommand { get; private set; } = null!;

        /// <summary>
        /// Command to add a new management practice to the calf stage
        /// </summary>
        public ICommand AddCalfManagementPracticeCommand { get; private set; } = null!;

        /// <summary>
        /// Command to add a new management practice to the heifer stage
        /// </summary>
        public ICommand AddHeiferManagementPracticeCommand { get; private set; } = null!;

        /// <summary>
        /// Command to add a new management practice to the lactating stage
        /// </summary>
        public ICommand AddLactatingManagementPracticeCommand { get; private set; } = null!;

        /// <summary>
        /// Command to add a new management practice to the dry stage
        /// </summary>
        public ICommand AddDryManagementPracticeCommand { get; private set; } = null!;

        /// <summary>
        /// Command to remove a management practice from any stage
        /// </summary>
        public ICommand RemoveManagementPracticeCommand { get; private set; } = null!;
        
        /// <summary>
        /// Gets or sets the currently selected calf group for management configuration.
        /// In Simple mode, this is null (management applies to the single implicit group).
        /// In Advanced mode, user selects which group to configure.
        /// </summary>
        public DairyPopulationGroup? SelectedCalfGroup
        {
            get => _selectedCalfGroup;
            set => SetProperty(ref _selectedCalfGroup, value);
        }
        
        /// <summary>
        /// Gets or sets the currently selected heifer group for management configuration
        /// </summary>
        public DairyPopulationGroup? SelectedHeiferGroup
        {
            get => _selectedHeiferGroup;
            set => SetProperty(ref _selectedHeiferGroup, value);
        }
        
        /// <summary>
        /// Gets or sets the currently selected lactating group for management configuration
        /// </summary>
        public DairyPopulationGroup? SelectedLactatingGroup
        {
            get => _selectedLactatingGroup;
            set => SetProperty(ref _selectedLactatingGroup, value);
        }
        
        /// <summary>
        /// Gets or sets the currently selected dry group for management configuration
        /// </summary>
        public DairyPopulationGroup? SelectedDryGroup
        {
            get => _selectedDryGroup;
            set => SetProperty(ref _selectedDryGroup, value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Entry point when navigating to this view. Gets a reference to the <see cref="DairyComponent"/>
        /// the user selected from the component selection view.
        /// </summary>
        /// <param name="navigationContext">An object holding a reference to the selected <see cref="DairyComponent"/></param>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey(GuiConstants.ComponentKey))
            {
                var parameter = navigationContext.Parameters[GuiConstants.ComponentKey];
                if (parameter is DairyComponent dairyComponent)
                {
                    this.InitializeViewModel(dairyComponent);
                }
            }
        }

        /// <summary>
        /// Called when navigating away from this view. Performs final transfer of DTO data to domain model
        /// and validates that no errors exist before allowing navigation.
        /// </summary>
        /// <param name="navigationContext">Navigation context containing navigation parameters</param>
        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            base.OnNavigatedFrom(navigationContext);

            // Perform final transfer from DTO to domain model before leaving
            if (_selectedDairyComponent is not null && _dairyComponentService is not null && _selectedDairyComponentDto is DairyComponentDto dairyComponentDto)
            {
                // Only transfer if there are no validation errors
                if (!dairyComponentDto.HasErrors)
                {
                    try
                    {
                        _dairyComponentService.TransferDairyDtoToSystem(
                            dairyComponentDto,
                            _selectedDairyComponent);

                        Logger?.LogInformation("Successfully saved dairy component changes");
                    }
                    catch (Exception exception)
                    {
                        Logger?.LogError(exception, "Error saving dairy component changes during navigation");
                    }
                }
                else
                {
                    Logger?.LogWarning("Dairy component has validation errors, changes not saved");
                }
            }

            // Clean up event handlers
            if (_selectedDairyComponentDto != null)
            {
                _selectedDairyComponentDto.PropertyChanged -= OnDairyComponentDtoPropertyChanged;
                
                if (_selectedDairyComponentDto is DairyComponentDto dto)
                {
                    dto.PropertyChanged -= DairyComponentDtoOnPropertyChanged;
                }
            }
        }

        /// <summary>
        /// Initializes the view model with a dairy component
        /// </summary>
        /// <param name="component">The dairy component to initialize with</param>
        public override void InitializeViewModel(ComponentBase component)
        {
            if (component is not DairyComponent dairyComponent)
            {
                return;
            }

            base.InitializeViewModel(component);

            this.InitializeDairyComponent(dairyComponent);
        }

        /// <summary>
        /// Initializes the dairy component and sets up data binding.
        /// 
        /// ARCHITECTURE NOTE:
        /// This method creates a DairyComponentDto from the domain model.
        /// The DTO will contain AnimalGroupDtos (not domain AnimalGroup objects).
        /// The service layer handles the conversion between domain objects and DTOs.
        /// </summary>
        /// <param name="dairyComponent">The dairy component to initialize</param>
        public void InitializeDairyComponent(DairyComponent? dairyComponent)
        {
            if (dairyComponent is null)
            {
                return;
            }

            // Hold a reference to the selected dairy component domain object
            _selectedDairyComponent = dairyComponent;

            // Build a DTO to represent the model/domain object using the dairy-specific service
            // This will also convert AnimalGroup domain objects to AnimalGroupDtos
            if (_dairyComponentService is null) return;

            var dairyComponentDto = _dairyComponentService.TransferToDairyComponentDto(dairyComponent);

            // Listen for changes on the DTO so we can validate user input before assigning values to the model
            dairyComponentDto.PropertyChanged += this.DairyComponentDtoOnPropertyChanged;

            // Assign the DTO to the property that is bound to the view
            // This will also trigger RaisePropertyChanged for AnimalGroupDtos
            this.SelectedDairyComponentDto = dairyComponentDto;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Common initialization method called by both constructors.
        /// Sets up empty collections and initializes all commands with their execution and validation logic.
        /// This ensures consistent initialization regardless of which constructor is used.
        /// </summary>
        private void Construct()
        {
            // Initialize selection properties
            _selectedHerdStage = string.Empty;
            _isCalfSelected = false;
            _isHeiferSelected = false;
            _isLactatingSelected = false;
            _isDrySelected = false;

            // Initialize manure state types (excluding obsolete options)
            ManureStateTypes = Enum.GetValues<ManureStateType>()
                .Where(x => !x.GetType().GetMember(x.ToString())[0]
                    .GetCustomAttributes(typeof(ObsoleteAttribute), false).Any())
                .ToList();

            // Initialize housing types (excluding obsolete options)
            HousingTypes = Enum.GetValues<HousingType>()
                .Where(x => !x.GetType().GetMember(x.ToString())[0]
                    .GetCustomAttributes(typeof(ObsoleteAttribute), false).Any())
                .ToList();

            // Initialize bedding material types valid for dairy animals
            BeddingMaterialTypes = new List<BeddingMaterialType>
            {
                BeddingMaterialType.None,
                BeddingMaterialType.Sand,
                BeddingMaterialType.SeparatedManureSolid,
                BeddingMaterialType.StrawLong,
                BeddingMaterialType.StrawChopped,
                BeddingMaterialType.Shavings,
                BeddingMaterialType.Sawdust,
            };

            // Initialize diet additive types
            DietAdditiveTypes = Enum.GetValues<DietAdditiveType>().ToList();

            // Initialize available diet types for dairy animals
            AvailableDietTypes = new List<DietType>
            {
                DietType.None,
                DietType.CloseUp,
                DietType.FarOffDry,
                DietType.HighEnergy,
                DietType.HighEnergyAndProtein,
                DietType.LowEnergy,
                DietType.LowEnergyAndProtein,
                DietType.MediumEnergy,
                DietType.MediumEnergyAndProtein,
            };

            // Initialize commands for adding/removing population groups
            AddCalfGroupCommand = new DelegateCommand(AddCalfGroup);
            AddHeiferGroupCommand = new DelegateCommand(AddHeiferGroup);
            AddLactatingGroupCommand = new DelegateCommand(AddLactatingGroup);
            AddDryGroupCommand = new DelegateCommand(AddDryGroup);
            RemoveGroupCommand = new DelegateCommand<DairyPopulationGroup>(RemoveGroup);

            // Initialize commands for adding/removing management practices
            AddCalfManagementPracticeCommand = new DelegateCommand(AddCalfManagementPractice);
            AddHeiferManagementPracticeCommand = new DelegateCommand(AddHeiferManagementPractice);
            AddLactatingManagementPracticeCommand = new DelegateCommand(AddLactatingManagementPractice);
            AddDryManagementPracticeCommand = new DelegateCommand(AddDryManagementPractice);
            RemoveManagementPracticeCommand = new DelegateCommand<ManagementPeriodDto>(RemoveManagementPractice);
        }

        /// <summary>
        /// Selects a specific herd stage card and deselects others
        /// </summary>
        /// <param name="stage">The stage to select ("Calf", "Heifer", "Lactating", or "Dry")</param>
        public void SelectHerdStage(string stage)
        {
            // Deselect all cards first
            IsCalfSelected = false;
            IsHeiferSelected = false;
            IsLactatingSelected = false;
            IsDrySelected = false;

            // Select the clicked card
            SelectedHerdStage = stage;

            switch (stage)
            {
                case "Calf":
                    IsCalfSelected = true;
                    // Auto-select first group if in advanced mode
                    if (SelectedDairyComponentDto?.UseAdvancedPopulationMode == true &&
                        SelectedDairyComponentDto.CalfPopulationGroups.Any())
                    {
                        SelectedCalfGroup = SelectedDairyComponentDto.CalfPopulationGroups.First();
                    }
                    // Auto-select first practice if available
                    SelectedCalfPractice ??= SelectedDairyComponentDto?.CalfManagementPractices.FirstOrDefault();
                    break;
                case "Heifer":
                    IsHeiferSelected = true;
                    if (SelectedDairyComponentDto?.UseAdvancedPopulationMode == true &&
                        SelectedDairyComponentDto.HeiferPopulationGroups.Any())
                    {
                        SelectedHeiferGroup = SelectedDairyComponentDto.HeiferPopulationGroups.First();
                    }
                    SelectedHeiferPractice ??= SelectedDairyComponentDto?.HeiferManagementPractices.FirstOrDefault();
                    break;
                case "Lactating":
                    IsLactatingSelected = true;
                    if (SelectedDairyComponentDto?.UseAdvancedPopulationMode == true &&
                        SelectedDairyComponentDto.LactatingPopulationGroups.Any())
                    {
                        SelectedLactatingGroup = SelectedDairyComponentDto.LactatingPopulationGroups.First();
                    }
                    SelectedLactatingPractice ??= SelectedDairyComponentDto?.LactatingManagementPractices.FirstOrDefault();
                    break;
                case "Dry":
                    IsDrySelected = true;
                    if (SelectedDairyComponentDto?.UseAdvancedPopulationMode == true &&
                        SelectedDairyComponentDto.DryPopulationGroups.Any())
                    {
                        SelectedDryGroup = SelectedDairyComponentDto.DryPopulationGroups.First();
                    }
                    SelectedDryPractice ??= SelectedDairyComponentDto?.DryManagementPractices.FirstOrDefault();
                    break;
            }

            Logger?.LogDebug($"Selected herd stage: {stage}");
        }
        
        /// <summary>
        /// Adds a new population group to the calf stage
        /// </summary>
        private void AddCalfGroup()
        {
            if (SelectedDairyComponentDto?.CalfPopulationGroups == null) return;
            
            var groupNumber = SelectedDairyComponentDto.CalfPopulationGroups.Count + 1;
            var newGroup = new DairyPopulationGroup($"Group {groupNumber}", 0);
            
            SelectedDairyComponentDto.CalfPopulationGroups.Add(newGroup);
            
            // Auto-select the newly added group
            SelectedCalfGroup = newGroup;
            
            Logger?.LogDebug("Added new calf population group");
        }
        
        /// <summary>
        /// Adds a new population group to the heifer stage
        /// </summary>
        private void AddHeiferGroup()
        {
            if (SelectedDairyComponentDto?.HeiferPopulationGroups == null) return;
            
            var groupNumber = SelectedDairyComponentDto.HeiferPopulationGroups.Count + 1;
            var newGroup = new DairyPopulationGroup($"Group {groupNumber}", 0);
            
            SelectedDairyComponentDto.HeiferPopulationGroups.Add(newGroup);
            SelectedHeiferGroup = newGroup;
            
            Logger?.LogDebug("Added new heifer population group");
        }
        
        /// <summary>
        /// Adds a new population group to the lactating stage
        /// </summary>
        private void AddLactatingGroup()
        {
            if (SelectedDairyComponentDto?.LactatingPopulationGroups == null) return;
            
            var groupNumber = SelectedDairyComponentDto.LactatingPopulationGroups.Count + 1;
            var newGroup = new DairyPopulationGroup($"Group {groupNumber}", 0);
            
            SelectedDairyComponentDto.LactatingPopulationGroups.Add(newGroup);
            SelectedLactatingGroup = newGroup;
            
            Logger?.LogDebug("Added new lactating population group");
        }
        
        /// <summary>
        /// Adds a new population group to the dry stage
        /// </summary>
        private void AddDryGroup()
        {
            if (SelectedDairyComponentDto?.DryPopulationGroups == null) return;
            
            var groupNumber = SelectedDairyComponentDto.DryPopulationGroups.Count + 1;
            var newGroup = new DairyPopulationGroup($"Group {groupNumber}", 0);
            
            SelectedDairyComponentDto.DryPopulationGroups.Add(newGroup);
            SelectedDryGroup = newGroup;
            
            Logger?.LogDebug("Added new dry population group");
        }
        
        /// <summary>
        /// Removes a population group from the appropriate stage
        /// </summary>
        private void RemoveGroup(DairyPopulationGroup? group)
        {
            if (group is null || SelectedDairyComponentDto is null) return;
            
            // Check if we're removing the currently selected group
            bool wasSelectedCalf = Equals(group, SelectedCalfGroup);
            bool wasSelectedHeifer = Equals(group, SelectedHeiferGroup);
            bool wasSelectedLactating = Equals(group, SelectedLactatingGroup);
            bool wasSelectedDry = Equals(group, SelectedDryGroup);
            
            // Try to remove from each collection
            var removed = SelectedDairyComponentDto.CalfPopulationGroups.Remove(group) ||
                         SelectedDairyComponentDto.HeiferPopulationGroups.Remove(group) ||
                         SelectedDairyComponentDto.LactatingPopulationGroups.Remove(group) ||
                         SelectedDairyComponentDto.DryPopulationGroups.Remove(group);
            
            if (removed)
            {
                // If we removed the selected group, select another one
                if (wasSelectedCalf && SelectedDairyComponentDto.CalfPopulationGroups.Any())
                {
                    SelectedCalfGroup = SelectedDairyComponentDto.CalfPopulationGroups.First();
                }
                else if (wasSelectedCalf)
                {
                    SelectedCalfGroup = null;
                }
                
                if (wasSelectedHeifer && SelectedDairyComponentDto.HeiferPopulationGroups.Any())
                {
                    SelectedHeiferGroup = SelectedDairyComponentDto.HeiferPopulationGroups.First();
                }
                else if (wasSelectedHeifer)
                {
                    SelectedHeiferGroup = null;
                }
                
                if (wasSelectedLactating && SelectedDairyComponentDto.LactatingPopulationGroups.Any())
                {
                    SelectedLactatingGroup = SelectedDairyComponentDto.LactatingPopulationGroups.First();
                }
                else if (wasSelectedLactating)
                {
                    SelectedLactatingGroup = null;
                }
                
                if (wasSelectedDry && SelectedDairyComponentDto.DryPopulationGroups.Any())
                {
                    SelectedDryGroup = SelectedDairyComponentDto.DryPopulationGroups.First();
                }
                else if (wasSelectedDry)
                {
                    SelectedDryGroup = null;
                }
                
                Logger?.LogDebug($"Removed population group: {group.GroupName}");
            }
        }

        /// <summary>
        /// Adds a new management practice to the calf stage
        /// </summary>
        private void AddCalfManagementPractice()
        {
            if (SelectedDairyComponentDto?.CalfManagementPractices == null) return;

            var practiceNumber = SelectedDairyComponentDto.CalfManagementPractices.Count + 1;
            var newPractice = new ManagementPeriodDto
            {
                Name = $"Practice {practiceNumber}",
                ManureStateType = ManureStateType.NotSelected,
                HousingType = HousingType.NotSelected,
            };

            SelectedDairyComponentDto.CalfManagementPractices.Add(newPractice);
            SelectedCalfPractice = newPractice;

            Logger?.LogDebug("Added new calf management practice");
        }

        /// <summary>
        /// Adds a new management practice to the heifer stage
        /// </summary>
        private void AddHeiferManagementPractice()
        {
            if (SelectedDairyComponentDto?.HeiferManagementPractices == null) return;

            var practiceNumber = SelectedDairyComponentDto.HeiferManagementPractices.Count + 1;
            var newPractice = new ManagementPeriodDto
            {
                Name = $"Practice {practiceNumber}",
                ManureStateType = ManureStateType.NotSelected,
                HousingType = HousingType.NotSelected,
            };

            SelectedDairyComponentDto.HeiferManagementPractices.Add(newPractice);
            SelectedHeiferPractice = newPractice;

            Logger?.LogDebug("Added new heifer management practice");
        }

        /// <summary>
        /// Adds a new management practice to the lactating stage
        /// </summary>
        private void AddLactatingManagementPractice()
        {
            if (SelectedDairyComponentDto?.LactatingManagementPractices == null) return;

            var practiceNumber = SelectedDairyComponentDto.LactatingManagementPractices.Count + 1;
            var newPractice = new ManagementPeriodDto
            {
                Name = $"Practice {practiceNumber}",
                ManureStateType = ManureStateType.NotSelected,
                HousingType = HousingType.NotSelected,
            };

            SelectedDairyComponentDto.LactatingManagementPractices.Add(newPractice);
            SelectedLactatingPractice = newPractice;

            Logger?.LogDebug("Added new lactating management practice");
        }

        /// <summary>
        /// Adds a new management practice to the dry stage
        /// </summary>
        private void AddDryManagementPractice()
        {
            if (SelectedDairyComponentDto?.DryManagementPractices == null) return;

            var practiceNumber = SelectedDairyComponentDto.DryManagementPractices.Count + 1;
            var newPractice = new ManagementPeriodDto
            {
                Name = $"Practice {practiceNumber}",
                ManureStateType = ManureStateType.NotSelected,
                HousingType = HousingType.NotSelected,
            };

            SelectedDairyComponentDto.DryManagementPractices.Add(newPractice);
            SelectedDryPractice = newPractice;

            Logger?.LogDebug("Added new dry management practice");
        }

        /// <summary>
        /// Removes a management practice from the appropriate stage
        /// </summary>
        private void RemoveManagementPractice(ManagementPeriodDto? practice)
        {
            if (practice is null || SelectedDairyComponentDto is null) return;

            // Track if removed practice was the selected one for its stage
            var wasCalfPractice = Equals(practice, SelectedCalfPractice);
            var wasHeiferPractice = Equals(practice, SelectedHeiferPractice);
            var wasLactatingPractice = Equals(practice, SelectedLactatingPractice);
            var wasDryPractice = Equals(practice, SelectedDryPractice);

            var removed = SelectedDairyComponentDto.CalfManagementPractices.Remove(practice) ||
                         SelectedDairyComponentDto.HeiferManagementPractices.Remove(practice) ||
                         SelectedDairyComponentDto.LactatingManagementPractices.Remove(practice) ||
                         SelectedDairyComponentDto.DryManagementPractices.Remove(practice);

            if (removed)
            {
                // Auto-select next practice if we removed the selected one
                if (wasCalfPractice)
                    SelectedCalfPractice = SelectedDairyComponentDto.CalfManagementPractices.FirstOrDefault();
                if (wasHeiferPractice)
                    SelectedHeiferPractice = SelectedDairyComponentDto.HeiferManagementPractices.FirstOrDefault();
                if (wasLactatingPractice)
                    SelectedLactatingPractice = SelectedDairyComponentDto.LactatingManagementPractices.FirstOrDefault();
                if (wasDryPractice)
                    SelectedDryPractice = SelectedDairyComponentDto.DryManagementPractices.FirstOrDefault();

                Logger?.LogDebug($"Removed management practice: {practice.Name}");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles property changes on the DairyComponentDto.
        /// This is where we can add logic to respond to specific property changes.
        /// </summary>
        private void OnDairyComponentDtoPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Handle property change notifications from the DTO
            // Views binding directly to SelectedDairyComponentDto.AnimalGroupDtos will
            // automatically receive collection change notifications via INotifyPropertyChanged
        }

        /// <summary>
        /// Some property on the <see cref="SelectedDairyComponentDto"/> has changed. Check if we need to validate any user
        /// input before assigning the value on to the associated <see cref="DairyComponent"/> domain object.
        /// 
        /// ARCHITECTURE NOTE:
        /// Changes to AnimalGroupDtos will also flow through this handler.
        /// The service layer handles transferring AnimalGroupDto changes back to AnimalGroup domain objects.
        /// </summary>
        private void DairyComponentDtoOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            if (sender is DairyComponentDto dairyComponentDto)
            {
                /*
                 * Before assigning values from the bound DTOs, check for any validation errors. If there are any validation errors
                 * we should not proceed with the transfer of user input from the DTO to the model until the validation errors are fixed
                 */

                if (!dairyComponentDto.HasErrors && _selectedDairyComponent is not null && _dairyComponentService is not null)
                {
                    try
                    {
                        // A property on the DTO has been changed by the user, assign the new value to the system object after any unit conversion (if necessary)
                        // This includes changes to AnimalGroupDtos which will be transferred to AnimalGroup domain objects
                        _dairyComponentService.TransferDairyDtoToSystem(dairyComponentDto, _selectedDairyComponent);
                    }
                    catch (Exception exception)
                    {
                        Logger?.LogError(exception, "Error transferring dairy component DTO to domain object");
                    }
                }
            }
        }

        #endregion
    }
}