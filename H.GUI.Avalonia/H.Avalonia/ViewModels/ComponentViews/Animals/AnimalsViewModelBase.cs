using H.Avalonia.Services.DietFormulator;
using H.Core.Enumerations;
using H.Core.Factories;
using H.Core.Factories.Animals;
using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using H.Localization;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using H.Avalonia.ViewModels.ComponentViews.Animals;

namespace H.Avalonia.ViewModels.ComponentViews.Animals
{
    /// <summary>
    /// Intermediate base ViewModel for all Other Animals components (Horses, Llamas, Mules, Bison, Goats, Deer, Elk, Alpacas).
    /// Provides group selection, management period filtering per group, and enum collections for ComboBox bindings.
    /// Follows the same 3-step pattern as DairyComponentViewModel: Define groups → Configure periods → Management practices.
    /// </summary>
    public abstract class AnimalsViewModelBase : AnimalComponentViewModelBase
    {
        #region Fields

        /// <summary>
        /// Error key used for overlap validation errors on management period DTOs.
        /// </summary>
        private const string DateOverlapErrorKey = "DateOverlap";

        private AnimalGroupDto? _selectedAnimalGroup;
        private ManagementPeriodDto? _selectedManagementPractice;
        private ObservableCollection<ManagementPeriodDto> _selectedGroupManagementPeriods;
        private IEnumerable<ManureStateType>? _manureStateTypes;
        private IEnumerable<HousingType>? _housingTypes;
        private IEnumerable<BeddingMaterialType>? _beddingMaterialTypes;
        private IEnumerable<DietAdditiveType>? _dietAdditiveTypes;
        private IEnumerable<DietType>? _availableDietTypes;
        private IDietFormulatorWindowService? _dietFormulatorWindowService;

        #endregion

        #region Constructors

        public AnimalsViewModelBase()
        {
            _selectedGroupManagementPeriods = new ObservableCollection<ManagementPeriodDto>();
            this.Construct();
        }

        public AnimalsViewModelBase(
            ILogger logger,
            IAnimalComponentService animalComponentService,
            IStorageService storageService,
            IManagementPeriodService managementPeriodService,
            IDietFormulatorWindowService dietFormulatorWindowService) : base(animalComponentService, logger, storageService, managementPeriodService)
        {
            _selectedGroupManagementPeriods = new ObservableCollection<ManagementPeriodDto>();
            _dietFormulatorWindowService = dietFormulatorWindowService;
            this.Construct();
        }

        private void Construct()
        {
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

            // Initialize bedding material types
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

            // Initialize available diet types (full set by default; concrete subclasses can
            // override to filter by AnimalType, e.g. beef vs dairy vs swine).
            AvailableDietTypes = Enum.GetValues<DietType>()
                .Where(x => !x.GetType().GetMember(x.ToString())[0]
                    .GetCustomAttributes(typeof(ObsoleteAttribute), false).Any())
                .ToList();

            // Initialize commands
            RemoveAnimalGroupCommand = new DelegateCommand<AnimalGroupDto>(OnRemoveAnimalGroupExecute);
            SelectAnimalGroupCommand = new DelegateCommand<AnimalGroupDto>(OnSelectAnimalGroupExecute);
            AddManagementPracticeCommand = new DelegateCommand(OnAddManagementPracticeExecute);
            RemoveManagementPracticeCommand = new DelegateCommand<ManagementPeriodDto>(OnRemoveManagementPracticeExecute);
            OpenDietFormulatorCommand = new DelegateCommand(OnOpenDietFormulatorExecute, CanOpenDietFormulator);
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command to remove an animal group from the component.
        /// </summary>
        public ICommand RemoveAnimalGroupCommand { get; private set; } = null!;

        /// <summary>
        /// Command to select an animal group (sets SelectedAnimalGroup).
        /// </summary>
        public ICommand SelectAnimalGroupCommand { get; private set; } = null!;

        /// <summary>
        /// Command to add a new management practice (period) to the selected group.
        /// </summary>
        public ICommand AddManagementPracticeCommand { get; private set; } = null!;

        /// <summary>
        /// Command to remove a management practice from the selected group.
        /// </summary>
        public ICommand RemoveManagementPracticeCommand { get; private set; } = null!;

        /// <summary>
        /// Command to open the Diet Formulator modal scoped to the currently selected
        /// animal group's animal type. Disabled when no group is selected or when no
        /// <see cref="IDietFormulatorWindowService"/> was injected (e.g. design-time).
        /// </summary>
        public ICommand OpenDietFormulatorCommand { get; private set; } = null!;

        #endregion

        #region Properties

        /// <summary>
        /// The currently selected animal group in Step 1. When changed, updates the
        /// <see cref="SelectedGroupManagementPeriods"/> collection to show only
        /// management periods belonging to this group.
        /// </summary>
        public AnimalGroupDto? SelectedAnimalGroup
        {
            get => _selectedAnimalGroup;
            set
            {
                var previous = _selectedAnimalGroup;
                if (SetProperty(ref _selectedAnimalGroup, value))
                {
                    if (previous != null)
                        previous.PropertyChanged -= OnSelectedGroupPropertyChanged;
                    if (value != null)
                        value.PropertyChanged += OnSelectedGroupPropertyChanged;

                    RaisePropertyChanged(nameof(IsGroupSelected));
                    RaisePropertyChanged(nameof(ManagementPeriodIntroText));
                    UpdateSelectedGroupManagementPeriods();
                    (OpenDietFormulatorCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// The currently selected management practice (period) for Step 3 editing.
        /// The TabControl in Step 3 binds its DataContext to this property.
        /// </summary>
        public ManagementPeriodDto? SelectedManagementPractice
        {
            get => _selectedManagementPractice;
            set
            {
                if (SetProperty(ref _selectedManagementPractice, value))
                {
                    RaisePropertyChanged(nameof(IsPracticeSelected));
                }
            }
        }

        /// <summary>
        /// Management periods filtered by the currently selected group.
        /// Bound to the DataGrid in Step 2 and the ComboBox in Step 3.
        /// </summary>
        public ObservableCollection<ManagementPeriodDto> SelectedGroupManagementPeriods
        {
            get => _selectedGroupManagementPeriods;
            set => SetProperty(ref _selectedGroupManagementPeriods, value);
        }

        /// <summary>
        /// Whether a group is currently selected. Controls visibility of Steps 2 and 3.
        /// </summary>
        public bool IsGroupSelected => SelectedAnimalGroup != null;

        /// <summary>
        /// Dynamically formatted intro text for the Step 2 instruction panel.
        /// Embeds the selected group's localized animal type description into the template string.
        /// </summary>
        public string ManagementPeriodIntroText =>
            string.Format(
                LocalizationService.Instance["Message_ManagementPeriodTableIntro"],
                GetAnimalTypeDescription(SelectedAnimalGroup?.GroupType));

        /// <summary>
        /// Whether a management practice is currently selected. Controls visibility of the TabControl in Step 3.
        /// </summary>
        public bool IsPracticeSelected => SelectedManagementPractice != null;

        /// <summary>
        /// Whether the selected group has at least one management period.
        /// Controls visibility of the DataGrid headers in Step 2 and all of Step 3.
        /// </summary>
        public bool HasManagementPeriods => SelectedGroupManagementPeriods?.Count > 0;

        /// <summary>
        /// Collection of available manure state types for dropdown selection.
        /// </summary>
        public IEnumerable<ManureStateType>? ManureStateTypes
        {
            get => _manureStateTypes;
            set => SetProperty(ref _manureStateTypes, value);
        }

        /// <summary>
        /// Collection of available housing types for dropdown selection.
        /// </summary>
        public IEnumerable<HousingType>? HousingTypes
        {
            get => _housingTypes;
            set => SetProperty(ref _housingTypes, value);
        }

        /// <summary>
        /// Collection of available bedding material types for dropdown selection.
        /// </summary>
        public IEnumerable<BeddingMaterialType>? BeddingMaterialTypes
        {
            get => _beddingMaterialTypes;
            set => SetProperty(ref _beddingMaterialTypes, value);
        }

        /// <summary>
        /// Collection of available diet additive types for dropdown selection.
        /// </summary>
        public IEnumerable<DietAdditiveType>? DietAdditiveTypes
        {
            get => _dietAdditiveTypes;
            set => SetProperty(ref _dietAdditiveTypes, value);
        }

        /// <summary>
        /// Collection of diet types available for selection on the Diet tab. Defaults to all
        /// non-obsolete <see cref="DietType"/> values; concrete subclasses may override
        /// to filter by the animal type they represent (e.g. beef-only diets for beef components).
        /// </summary>
        public IEnumerable<DietType>? AvailableDietTypes
        {
            get => _availableDietTypes;
            set => SetProperty(ref _availableDietTypes, value);
        }

        #endregion

        #region Public Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            // Auto-select first group if available
            if (AnimalGroupDtos?.Any() == true && SelectedAnimalGroup == null)
            {
                SelectedAnimalGroup = AnimalGroupDtos.First();
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Updates the <see cref="SelectedGroupManagementPeriods"/> to reflect the management
        /// periods belonging to the currently selected group, and updates the visual selection
        /// state on all group cards.
        /// </summary>
        private void UpdateSelectedGroupManagementPeriods()
        {
            // Unsubscribe from old collection's period date changes
            UnsubscribeFromAllPeriods();

            // Update IsSelected on all groups so the UI highlights the correct card
            if (AnimalGroupDtos != null)
            {
                foreach (var group in AnimalGroupDtos)
                {
                    group.IsSelected = ReferenceEquals(group, SelectedAnimalGroup);
                }
            }

            if (SelectedAnimalGroup != null)
            {
                SelectedGroupManagementPeriods = SelectedAnimalGroup.ManagementPeriodDtos;

                // Auto-select first practice if available
                SelectedManagementPractice = SelectedGroupManagementPeriods.FirstOrDefault();
            }
            else
            {
                SelectedGroupManagementPeriods = new ObservableCollection<ManagementPeriodDto>();
                SelectedManagementPractice = null;
            }

            // Subscribe to new collection's period date changes and validate
            SubscribeToAllPeriods();
            ValidateOverlappingPeriods();

            RaisePropertyChanged(nameof(HasManagementPeriods));
        }

        /// <summary>
        /// Removes a specific animal group from the component.
        /// If the removed group was selected, auto-selects the next available group.
        /// </summary>
        private void OnRemoveAnimalGroupExecute(AnimalGroupDto? group)
        {
            if (group == null || AnimalGroupDtos == null) return;

            var wasSelected = Equals(group, SelectedAnimalGroup);
            AnimalGroupDtos.Remove(group);

            if (wasSelected)
            {
                SelectedAnimalGroup = AnimalGroupDtos.FirstOrDefault();
            }

            Logger?.LogDebug("Removed animal group: {GroupName}", group.Name ?? "Unnamed");
        }

        /// <summary>
        /// Selects an animal group, updating Steps 2 and 3 to show its data.
        /// </summary>
        private void OnSelectAnimalGroupExecute(AnimalGroupDto? group)
        {
            if (group == null) return;
            SelectedAnimalGroup = group;
        }

        /// <summary>
        /// Adds a new management practice to the selected group's management periods.
        /// </summary>
        private void OnAddManagementPracticeExecute()
        {
            if (SelectedAnimalGroup == null) return;

            var practiceNumber = SelectedAnimalGroup.ManagementPeriodDtos.Count + 1;

            // Default the new period's start date to follow the end date of the last existing period
            // so that newly added periods don't overlap by default.
            DateTime startDate;
            DateTime endDate;
            int numberOfDays;

            var lastPeriod = SelectedAnimalGroup.ManagementPeriodDtos.LastOrDefault();
            if (lastPeriod != null && lastPeriod.End != default)
            {
                startDate = lastPeriod.End;
                endDate = startDate.AddDays(365 - 1);
                numberOfDays = 365;
            }
            else
            {
                startDate = new DateTime(DateTime.Now.Year, 1, 1);
                endDate = new DateTime(DateTime.Now.Year, 12, 31);
                numberOfDays = (endDate - startDate).Days + 1;
            }

            var newPractice = new ManagementPeriodDto
            {
                Name = $"Practice {practiceNumber}",
                Start = startDate,
                End = endDate,
                NumberOfDays = numberOfDays,
                NumberOfAnimals = 20,
                ManureStateType = ManureStateType.NotSelected,
                HousingType = HousingType.NotSelected,
            };

            SelectedAnimalGroup.ManagementPeriodDtos.Add(newPractice);
            SubscribeToPeriodChanges(newPractice);
            SelectedManagementPractice = newPractice;
            ValidateOverlappingPeriods();
            RaisePropertyChanged(nameof(HasManagementPeriods));

            Logger?.LogDebug("Added management practice to group: {GroupName}", SelectedAnimalGroup.Name ?? "Unnamed");
        }

        /// <summary>
        /// Opens the Diet Formulator modal, scoped to the currently selected animal group's
        /// animal type. When the user clicks Save in the modal, applies the saved diet
        /// to the currently selected management practice's flattened diet fields so the
        /// Diet tab refreshes immediately. Cancel/close discards.
        /// </summary>
        private async void OnOpenDietFormulatorExecute()
        {
            if (_dietFormulatorWindowService == null
                || SelectedAnimalGroup?.GroupType == null
                || SelectedAnimalGroup.GroupType == AnimalType.NotSelected)
            {
                return;
            }

            var saved = await _dietFormulatorWindowService.ShowAsync(SelectedAnimalGroup.GroupType.Value);
            if (saved == null || SelectedManagementPractice == null)
            {
                return;
            }

            // Apply the saved diet's flattened fields to the management period DTO.
            // The Diet tab binds to these properties directly, so the UI updates as soon
            // as we set them. Round-tripping back to the underlying ManagementPeriod model's
            // SelectedDiet (full Diet object) happens via the existing reverse mapper at
            // save-farm time.
            SelectedManagementPractice.SelectedDietType = saved.DietType;
            SelectedManagementPractice.CrudeProtein = saved.CrudeProtein;
            SelectedManagementPractice.Forage = saved.Forage;
            SelectedManagementPractice.TotalDigestibleNutrient = saved.TotalDigestibleNutrient;
            SelectedManagementPractice.DailyDryMatterFeedIntakeOfFeed = saved.DailyDryMatterFeedIntakeOfFeed;
            SelectedManagementPractice.MethaneConversionFactor = saved.MethaneConversionFactor;
        }

        /// <summary>
        /// Returns true when the Diet Formulator can be opened: a group must be selected
        /// and the dialog service must have been injected.
        /// </summary>
        private bool CanOpenDietFormulator()
            => _dietFormulatorWindowService != null
               && SelectedAnimalGroup != null
               && SelectedAnimalGroup.GroupType.HasValue
               && SelectedAnimalGroup.GroupType.Value != AnimalType.NotSelected;

        /// <summary>
        /// Removes a management practice from the selected group.
        /// Auto-selects the next available practice if the removed one was selected.
        /// </summary>
        private void OnRemoveManagementPracticeExecute(ManagementPeriodDto? practice)
        {
            if (practice == null || SelectedAnimalGroup == null) return;

            var wasSelected = Equals(practice, SelectedManagementPractice);
            UnsubscribeFromPeriodChanges(practice);
            SelectedAnimalGroup.ManagementPeriodDtos.Remove(practice);

            if (wasSelected)
            {
                SelectedManagementPractice = SelectedAnimalGroup.ManagementPeriodDtos.FirstOrDefault();
            }

            ValidateOverlappingPeriods();
            RaisePropertyChanged(nameof(HasManagementPeriods));

            Logger?.LogDebug("Removed management practice: {PracticeName}", practice.Name ?? "Unnamed");
        }

        /// <summary>
        /// Subscribes to PropertyChanged on a single management period to detect date changes
        /// and trigger overlap validation.
        /// </summary>
        private void SubscribeToPeriodChanges(ManagementPeriodDto period)
        {
            period.PropertyChanged += OnManagementPeriodDateChanged;
        }

        /// <summary>
        /// Unsubscribes from PropertyChanged on a single management period.
        /// </summary>
        private void UnsubscribeFromPeriodChanges(ManagementPeriodDto period)
        {
            period.PropertyChanged -= OnManagementPeriodDateChanged;
        }

        /// <summary>
        /// Subscribes to all periods in the current collection.
        /// </summary>
        private void SubscribeToAllPeriods()
        {
            foreach (var period in SelectedGroupManagementPeriods)
            {
                SubscribeToPeriodChanges(period);
            }
        }

        /// <summary>
        /// Unsubscribes from all periods in the current collection.
        /// </summary>
        private void UnsubscribeFromAllPeriods()
        {
            foreach (var period in SelectedGroupManagementPeriods)
            {
                UnsubscribeFromPeriodChanges(period);
            }
        }

        /// <summary>
        /// Handles date property changes on any management period, triggering overlap validation.
        /// </summary>
        private void OnManagementPeriodDateChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName is nameof(ManagementPeriodDto.Start) or nameof(ManagementPeriodDto.End))
            {
                ValidateOverlappingPeriods();
            }
        }

        /// <summary>
        /// Handles property changes on the currently selected group.
        /// Re-raises <see cref="ManagementPeriodIntroText"/> when <see cref="AnimalGroupDto.GroupType"/> changes,
        /// and refreshes <see cref="OpenDietFormulatorCommand"/> CanExecute so the button enables/disables
        /// as the user picks an animal type for the group.
        /// </summary>
        private void OnSelectedGroupPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AnimalGroupDto.GroupType))
            {
                RaisePropertyChanged(nameof(ManagementPeriodIntroText));
                (OpenDietFormulatorCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Validates all management periods in the selected group for overlapping date ranges.
        /// Two periods overlap when one starts before the other ends and ends after the other starts.
        /// Adds/removes overlap errors on each affected period.
        /// </summary>
        private void ValidateOverlappingPeriods()
        {
            var periods = SelectedGroupManagementPeriods?.ToList();
            if (periods == null) return;

            // Clear all existing overlap errors first
            foreach (var period in periods)
            {
                period.RemoveError(DateOverlapErrorKey);
            }

            if (periods.Count < 2) return;

            // Check each pair for date range overlap
            for (var i = 0; i < periods.Count; i++)
            {
                for (var j = i + 1; j < periods.Count; j++)
                {
                    var a = periods[i];
                    var b = periods[j];

                    // Skip pairs where either has unset dates
                    if (a.Start == default || a.End == default || b.Start == default || b.End == default)
                        continue;

                    // Two intervals overlap when A starts before B ends AND A ends after B starts
                    if (a.Start < b.End && a.End > b.Start)
                    {
                        a.AddError(DateOverlapErrorKey,
                            string.Format(H.Core.Properties.Resources.ErrorManagementPeriodsOverlap, b.Name));
                        b.AddError(DateOverlapErrorKey,
                            string.Format(H.Core.Properties.Resources.ErrorManagementPeriodsOverlap, a.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Returns the localized display name for an <see cref="AnimalType"/> value by reading
        /// its <see cref="System.ComponentModel.DescriptionAttribute"/> (which
        /// <see cref="H.Infrastructure.LocalizedDescriptionAttribute"/> overrides to return a
        /// resource-file string). Falls back to <see cref="Enum.ToString"/> when no attribute exists.
        /// </summary>
        private static string GetAnimalTypeDescription(AnimalType? animalType)
        {
            if (animalType is null) return string.Empty;

            var field = animalType.Value.GetType().GetField(animalType.Value.ToString());
            if (field is null) return animalType.Value.ToString();

            var attr = field.GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false)
                            .FirstOrDefault() as System.ComponentModel.DescriptionAttribute;

            return attr?.Description ?? animalType.Value.ToString();
        }

        #endregion
    }
}
