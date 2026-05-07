using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using H.Avalonia.ViewModels;
using H.Core.Enumerations;
using H.Core.Mappers;
using H.Core.Providers.Feed;
using H.Core.Services.DietService;
using H.Core.Services.StorageService;
using Prism.Commands;

namespace H.Avalonia.ViewModels.SupportingViews.DietFormulator;

/// <summary>
/// View model backing the <c>DietFormulatorWindow</c> modal.
/// Phase 3 — edits run on a deep-cloned working copy of the selected diet so
/// changes don't leak into <see cref="IDietService"/>'s shared default diets.
/// Save commits the working copy back via <see cref="SavedDiet"/>; Cancel
/// discards it. The hosting service awaits <see cref="SavedDiet"/> via the
/// <see cref="CloseRequested"/> event sequence.
/// </summary>
public class DietFormulatorWindowViewModel : ViewModelBase
{
    private readonly IDietService _dietService;
    private readonly IFeedIngredientProvider _feedIngredientProvider;
    private readonly IStorageService? _storageService;
    private readonly DietToDietDtoMapper _dietToDtoMapper = new();
    private readonly AnimalType _animalType;

    private IDietDto? _selectedDiet;
    private DietDto? _workingDiet;
    private IFeedIngredient? _selectedAvailableIngredient;
    private IFeedIngredient? _selectedIngredientInDiet;
    private bool _isSaveAsPromptVisible;
    private string _saveAsName = string.Empty;

    public DietFormulatorWindowViewModel()
    {
        // Design-time constructor.
        _dietService = null!;
        _feedIngredientProvider = null!;
        _animalType = AnimalType.NotSelected;
        MyDiets = new ObservableCollection<IDietDto>();
        AvailableIngredients = new ObservableCollection<IFeedIngredient>();
        SelectedDietIngredients = new ObservableCollection<IFeedIngredient>();
        SelectedDietIngredients.CollectionChanged += OnSelectedDietIngredientsChanged;
        SaveCommand = new DelegateCommand(OnSaveExecute, CanSave);
        CancelCommand = new DelegateCommand(OnCancelExecute);
        AddIngredientCommand = new DelegateCommand(OnAddIngredientExecute, CanAddIngredient);
        RemoveIngredientCommand = new DelegateCommand(OnRemoveIngredientExecute, CanRemoveIngredient);
        BeginSaveAsCustomCommand = new DelegateCommand(OnBeginSaveAsCustomExecute, CanBeginSaveAsCustom);
        ConfirmSaveAsCustomCommand = new DelegateCommand(OnConfirmSaveAsCustomExecute, CanConfirmSaveAsCustom);
        CancelSaveAsCustomCommand = new DelegateCommand(OnCancelSaveAsCustomExecute);
    }

    public DietFormulatorWindowViewModel(
        IDietService dietService,
        IFeedIngredientProvider feedIngredientProvider,
        IStorageService storageService,
        AnimalType animalType)
    {
        _dietService = dietService ?? throw new ArgumentNullException(nameof(dietService));
        _feedIngredientProvider = feedIngredientProvider ?? throw new ArgumentNullException(nameof(feedIngredientProvider));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _animalType = animalType;

        MyDiets = new ObservableCollection<IDietDto>();
        AvailableIngredients = new ObservableCollection<IFeedIngredient>();
        SelectedDietIngredients = new ObservableCollection<IFeedIngredient>();
        SelectedDietIngredients.CollectionChanged += OnSelectedDietIngredientsChanged;

        SaveCommand = new DelegateCommand(OnSaveExecute, CanSave);
        CancelCommand = new DelegateCommand(OnCancelExecute);
        AddIngredientCommand = new DelegateCommand(OnAddIngredientExecute, CanAddIngredient);
        RemoveIngredientCommand = new DelegateCommand(OnRemoveIngredientExecute, CanRemoveIngredient);
        BeginSaveAsCustomCommand = new DelegateCommand(OnBeginSaveAsCustomExecute, CanBeginSaveAsCustom);
        ConfirmSaveAsCustomCommand = new DelegateCommand(OnConfirmSaveAsCustomExecute, CanConfirmSaveAsCustom);
        CancelSaveAsCustomCommand = new DelegateCommand(OnCancelSaveAsCustomExecute);

        Load();
    }

    /// <summary>Diets matching the constructor's <see cref="AnimalType"/>.</summary>
    public ObservableCollection<IDietDto> MyDiets { get; }

    /// <summary>Feed ingredient library for the constructor's <see cref="AnimalType"/>.</summary>
    public ObservableCollection<IFeedIngredient> AvailableIngredients { get; }

    /// <summary>
    /// Ingredients of the currently-edited working diet. Deep-cloned from the source
    /// so user edits never mutate the shared <see cref="IDietService"/> instances.
    /// </summary>
    public ObservableCollection<IFeedIngredient> SelectedDietIngredients { get; }

    /// <summary>
    /// Diet selected from MyDiets. The setter deep-clones the source diet into
    /// <see cref="_workingDiet"/> so subsequent edits are isolated.
    /// </summary>
    public IDietDto? SelectedDiet
    {
        get => _selectedDiet;
        set
        {
            if (SetProperty(ref _selectedDiet, value))
            {
                _workingDiet = value == null ? null : CloneForEdit(value);
                RefreshSelectedDietIngredients();
                (AddIngredientCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                (SaveCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                (BeginSaveAsCustomCommand as DelegateCommand)?.RaiseCanExecuteChanged();
                RaisePropertyChanged(nameof(WorkingDietName));
            }
        }
    }

    /// <summary>
    /// Display name for the working diet (used by the right-pane subheader).
    /// Reflects the working copy, not the original.
    /// </summary>
    public string WorkingDietName => _workingDiet?.Name ?? string.Empty;

    public IFeedIngredient? SelectedAvailableIngredient
    {
        get => _selectedAvailableIngredient;
        set
        {
            if (SetProperty(ref _selectedAvailableIngredient, value))
                (AddIngredientCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }
    }

    public IFeedIngredient? SelectedIngredientInDiet
    {
        get => _selectedIngredientInDiet;
        set
        {
            if (SetProperty(ref _selectedIngredientInDiet, value))
                (RemoveIngredientCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }
    }

    public AnimalType AnimalType => _animalType;

    /// <summary>Commit edits and close. Sets <see cref="SavedDiet"/> for the launcher to consume.</summary>
    public ICommand SaveCommand { get; }

    /// <summary>Discard edits and close. <see cref="SavedDiet"/> stays null.</summary>
    public ICommand CancelCommand { get; }

    public ICommand AddIngredientCommand { get; }
    public ICommand RemoveIngredientCommand { get; }

    /// <summary>Reveals the inline "Save As Custom" name prompt panel.</summary>
    public ICommand BeginSaveAsCustomCommand { get; }
    /// <summary>Commits the Save As Custom: copies working diet to <c>Farm.Diets</c> and closes.</summary>
    public ICommand ConfirmSaveAsCustomCommand { get; }
    /// <summary>Hides the inline name prompt and discards the typed name.</summary>
    public ICommand CancelSaveAsCustomCommand { get; }

    /// <summary>Whether the inline "Save As Custom" name prompt is currently visible.</summary>
    public bool IsSaveAsPromptVisible
    {
        get => _isSaveAsPromptVisible;
        set => SetProperty(ref _isSaveAsPromptVisible, value);
    }

    /// <summary>Name the user is typing for the new custom diet.</summary>
    public string SaveAsName
    {
        get => _saveAsName;
        set
        {
            if (SetProperty(ref _saveAsName, value))
                (ConfirmSaveAsCustomCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// The committed working diet, populated on Save. Null when the user cancels or
    /// closes without saving. The launcher reads this after the window closes.
    /// </summary>
    public DietDto? SavedDiet { get; private set; }

    /// <summary>Raised when the VM wants the hosting window to close.</summary>
    public event EventHandler? CloseRequested;

    #region Live nutrient totals

    public double TotalCrudeProtein => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.CrudeProtein);
    public double TotalTDN => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.TotalDigestibleNutrient);
    public double TotalForage => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.Forage);
    public double TotalNDF => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.NDF);
    public double TotalADF => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.ADF);
    public double TotalFat => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.Fat);
    public double TotalAsh => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.Ash);
    public double TotalME => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.ME);
    public double PercentageSum => SelectedDietIngredients.Sum(x => x.PercentageInDiet);
    public bool IsPercentageSumValid => Math.Abs(PercentageSum - 100.0) < 0.01;

    #endregion

    private void Load()
    {
        MyDiets.Clear();

        // Custom diets first (so they appear at the top of the grid).
        // Pull from the active Farm's Diets collection. These are full Diet model objects;
        // map to DietDto via the existing DietToDietDtoMapper before adding.
        var activeFarm = _storageService?.GetActiveFarm();
        if (activeFarm?.Diets != null)
        {
            foreach (var diet in activeFarm.Diets)
            {
                if (DietBelongsToAnimalType(diet.AnimalType, _animalType))
                {
                    var dto = _dietToDtoMapper.Map(diet);
                    dto.IsDefaultDiet = false; // Mark as custom regardless of source flag
                    MyDiets.Add(dto);
                }
            }
        }

        // Then default diets from the diet service.
        var allDiets = _dietService.GetDiets();
        foreach (var diet in allDiets)
        {
            if (DietBelongsToAnimalType(diet.AnimalType, _animalType))
                MyDiets.Add(diet);
        }

        AvailableIngredients.Clear();
        IList<IFeedIngredient>? ingredients = null;
        if (_animalType.IsBeefCattleType()) ingredients = _feedIngredientProvider.GetBeefFeedIngredients();
        else if (_animalType.IsDairyCattleType()) ingredients = _feedIngredientProvider.GetDairyFeedIngredients();
        else if (_animalType.IsSwineType()) ingredients = _feedIngredientProvider.GetSwineFeedIngredients();

        if (ingredients != null)
        {
            foreach (var ingredient in ingredients)
                AvailableIngredients.Add(ingredient);
        }

        SelectedDiet = MyDiets.FirstOrDefault();
    }

    /// <summary>
    /// Deep-clones a diet (and its ingredients) into a new <see cref="DietDto"/> so the
    /// formulator can edit without mutating the source from <see cref="IDietService"/>.
    /// </summary>
    private DietDto CloneForEdit(IDietDto source)
    {
        var clone = new DietDto();

        // Copy scalars. PropertyMapper handles the concrete-to-concrete case efficiently;
        // for any future implementation of IDietDto we copy the interface surface manually.
        if (source is DietDto concrete)
        {
            PropertyMapper.CopyTo(concrete, clone);
        }
        else
        {
            clone.Name = source.Name;
            clone.DietType = source.DietType;
            clone.AnimalType = source.AnimalType;
            clone.IsDefaultDiet = source.IsDefaultDiet;
            clone.MethaneConversionFactor = source.MethaneConversionFactor;
            clone.DietaryNetEnergyConcentration = source.DietaryNetEnergyConcentration;
        }

        // Deep-clone ingredients via CopyIngredient (which uses PropertyMapper internally).
        var clonedIngredients = new List<IFeedIngredient>();
        if (source.Ingredients != null)
        {
            foreach (var ingredient in source.Ingredients)
            {
                var copy = _feedIngredientProvider.CopyIngredient(ingredient, ingredient.PercentageInDiet);
                if (copy != null) clonedIngredients.Add(copy);
            }
        }
        clone.Ingredients = clonedIngredients;
        return clone;
    }

    private void RefreshSelectedDietIngredients()
    {
        SelectedDietIngredients.Clear();
        if (_workingDiet?.Ingredients == null) return;
        foreach (var ingredient in _workingDiet.Ingredients)
            SelectedDietIngredients.Add(ingredient);
    }

    private void OnSaveExecute()
    {
        if (_workingDiet == null) return;

        // Recompute the diet-level nutrient scalars from current ingredient percentages
        // so the saved DTO reflects the user's edits.
        _workingDiet.CrudeProtein = TotalCrudeProtein;
        _workingDiet.TotalDigestibleNutrient = TotalTDN;
        _workingDiet.Forage = TotalForage;
        _workingDiet.Ash = TotalAsh;

        SavedDiet = _workingDiet;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanSave() => _workingDiet != null;

    private void OnCancelExecute()
    {
        SavedDiet = null;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void OnAddIngredientExecute()
    {
        if (_selectedAvailableIngredient == null) return;
        var copy = _feedIngredientProvider.CopyIngredient(_selectedAvailableIngredient, defaultPercentageInDiet: 0);
        if (copy != null)
        {
            // Add to both the working diet's underlying list and the UI-bound collection.
            (_workingDiet?.Ingredients as IList<IFeedIngredient>)?.Add(copy);
            SelectedDietIngredients.Add(copy);
        }
    }

    private bool CanAddIngredient()
        => _feedIngredientProvider != null
           && _selectedAvailableIngredient != null
           && _workingDiet != null;

    private void OnRemoveIngredientExecute()
    {
        if (_selectedIngredientInDiet == null) return;
        (_workingDiet?.Ingredients as IList<IFeedIngredient>)?.Remove(_selectedIngredientInDiet);
        SelectedDietIngredients.Remove(_selectedIngredientInDiet);
        SelectedIngredientInDiet = null;
    }

    private bool CanRemoveIngredient() => _selectedIngredientInDiet != null;

    private void OnBeginSaveAsCustomExecute()
    {
        // Pre-fill the name input with the working diet's current name so users can edit
        // rather than retype from scratch.
        SaveAsName = _workingDiet?.Name ?? string.Empty;
        IsSaveAsPromptVisible = true;
    }

    private bool CanBeginSaveAsCustom() => _workingDiet != null;

    private void OnConfirmSaveAsCustomExecute()
    {
        if (_workingDiet == null || string.IsNullOrWhiteSpace(_saveAsName)) return;
        var farm = _storageService?.GetActiveFarm();
        if (farm == null) return;

        // Recompute scalars on the working diet so the persisted custom diet has accurate
        // nutrient totals matching its current ingredients.
        _workingDiet.CrudeProtein = TotalCrudeProtein;
        _workingDiet.TotalDigestibleNutrient = TotalTDN;
        _workingDiet.Forage = TotalForage;
        _workingDiet.Ash = TotalAsh;

        // Convert the working DietDto to a Diet model and store on Farm.
        // DietDtoToDietMapper deep-clones ingredients into the Diet's constructor-wired
        // ObservableCollection so UpdateTotals fires correctly.
        var customDiet = new H.Core.Mappers.DietDtoToDietMapper().Map(_workingDiet);
        customDiet.Name = _saveAsName.Trim();
        customDiet.IsDefaultDiet = false;
        farm.Diets.Add(customDiet);

        // Reflect the new identity on the working DTO so the launcher's saved-diet handler
        // applies the right name and IsDefaultDiet flag to the management period.
        _workingDiet.Name = customDiet.Name;
        _workingDiet.IsDefaultDiet = false;

        SavedDiet = _workingDiet;
        IsSaveAsPromptVisible = false;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private bool CanConfirmSaveAsCustom() => !string.IsNullOrWhiteSpace(_saveAsName);

    private void OnCancelSaveAsCustomExecute()
    {
        SaveAsName = string.Empty;
        IsSaveAsPromptVisible = false;
    }

    private void OnSelectedDietIngredientsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Reset)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<IFeedIngredient>())
                {
                    if (item is INotifyPropertyChanged inpc)
                    {
                        inpc.PropertyChanged -= OnIngredientPropertyChanged;
                        inpc.PropertyChanged += OnIngredientPropertyChanged;
                    }
                }
            }
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<IFeedIngredient>())
                {
                    if (item is INotifyPropertyChanged inpc)
                        inpc.PropertyChanged -= OnIngredientPropertyChanged;
                }
            }
        }

        RaiseTotalsChanged();
    }

    private void OnIngredientPropertyChanged(object? sender, PropertyChangedEventArgs e) => RaiseTotalsChanged();

    private void RaiseTotalsChanged()
    {
        RaisePropertyChanged(nameof(TotalCrudeProtein));
        RaisePropertyChanged(nameof(TotalTDN));
        RaisePropertyChanged(nameof(TotalForage));
        RaisePropertyChanged(nameof(TotalNDF));
        RaisePropertyChanged(nameof(TotalADF));
        RaisePropertyChanged(nameof(TotalFat));
        RaisePropertyChanged(nameof(TotalAsh));
        RaisePropertyChanged(nameof(TotalME));
        RaisePropertyChanged(nameof(PercentageSum));
        RaisePropertyChanged(nameof(IsPercentageSumValid));
    }

    private static bool DietBelongsToAnimalType(AnimalType dietAnimalType, AnimalType scope)
    {
        if (scope.IsBeefCattleType()) return dietAnimalType.IsBeefCattleType();
        if (scope.IsDairyCattleType()) return dietAnimalType.IsDairyCattleType();
        if (scope.IsSwineType()) return dietAnimalType.IsSwineType();
        if (scope.IsSheepType()) return dietAnimalType.IsSheepType();
        return dietAnimalType == scope;
    }
}
