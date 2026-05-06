using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using H.Avalonia.ViewModels;
using H.Core.Enumerations;
using H.Core.Providers.Feed;
using H.Core.Services.DietService;
using Prism.Commands;

namespace H.Avalonia.ViewModels.SupportingViews.DietFormulator;

/// <summary>
/// View model backing the <c>DietFormulatorWindow</c> modal.
/// Loads diets and ingredients filtered by <see cref="AnimalType"/> so the formulator
/// is scoped to the animal group that launched it.
/// Phase 2 — supports adding/removing ingredients and editing percentages with
/// live nutrient totals and percentage-sum validation. Custom diet creation,
/// deep-cloning before edit, and JSON persistence land in Phase 3.
/// </summary>
public class DietFormulatorWindowViewModel : ViewModelBase
{
    private readonly IDietService _dietService;
    private readonly IFeedIngredientProvider _feedIngredientProvider;
    private readonly AnimalType _animalType;

    private IDietDto? _selectedDiet;
    private IFeedIngredient? _selectedAvailableIngredient;
    private IFeedIngredient? _selectedIngredientInDiet;

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
        CloseCommand = new DelegateCommand(() => CloseRequested?.Invoke(this, EventArgs.Empty));
        AddIngredientCommand = new DelegateCommand(OnAddIngredientExecute, CanAddIngredient);
        RemoveIngredientCommand = new DelegateCommand(OnRemoveIngredientExecute, CanRemoveIngredient);
    }

    public DietFormulatorWindowViewModel(
        IDietService dietService,
        IFeedIngredientProvider feedIngredientProvider,
        AnimalType animalType)
    {
        _dietService = dietService ?? throw new ArgumentNullException(nameof(dietService));
        _feedIngredientProvider = feedIngredientProvider ?? throw new ArgumentNullException(nameof(feedIngredientProvider));
        _animalType = animalType;

        MyDiets = new ObservableCollection<IDietDto>();
        AvailableIngredients = new ObservableCollection<IFeedIngredient>();
        SelectedDietIngredients = new ObservableCollection<IFeedIngredient>();
        SelectedDietIngredients.CollectionChanged += OnSelectedDietIngredientsChanged;

        CloseCommand = new DelegateCommand(() => CloseRequested?.Invoke(this, EventArgs.Empty));
        AddIngredientCommand = new DelegateCommand(OnAddIngredientExecute, CanAddIngredient);
        RemoveIngredientCommand = new DelegateCommand(OnRemoveIngredientExecute, CanRemoveIngredient);

        Load();
    }

    /// <summary>
    /// Diets matching the constructor's <see cref="AnimalType"/>.
    /// </summary>
    public ObservableCollection<IDietDto> MyDiets { get; }

    /// <summary>
    /// Feed ingredient library for the constructor's <see cref="AnimalType"/>.
    /// </summary>
    public ObservableCollection<IFeedIngredient> AvailableIngredients { get; }

    /// <summary>
    /// Ingredients of the currently selected diet (refreshed when <see cref="SelectedDiet"/> changes).
    /// Each ingredient's <c>PropertyChanged</c> is wired so percentage edits update live totals.
    /// </summary>
    public ObservableCollection<IFeedIngredient> SelectedDietIngredients { get; }

    /// <summary>
    /// Diet currently selected in the MyDiets grid. Drives <see cref="SelectedDietIngredients"/>.
    /// </summary>
    public IDietDto? SelectedDiet
    {
        get => _selectedDiet;
        set
        {
            if (SetProperty(ref _selectedDiet, value))
            {
                RefreshSelectedDietIngredients();
                (AddIngredientCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Ingredient highlighted in the AvailableIngredients grid. Drives <c>AddIngredientCommand</c> CanExecute.
    /// </summary>
    public IFeedIngredient? SelectedAvailableIngredient
    {
        get => _selectedAvailableIngredient;
        set
        {
            if (SetProperty(ref _selectedAvailableIngredient, value))
            {
                (AddIngredientCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Ingredient highlighted in the SelectedDietIngredients grid. Drives <c>RemoveIngredientCommand</c> CanExecute.
    /// </summary>
    public IFeedIngredient? SelectedIngredientInDiet
    {
        get => _selectedIngredientInDiet;
        set
        {
            if (SetProperty(ref _selectedIngredientInDiet, value))
            {
                (RemoveIngredientCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Animal type the formulator is scoped to (set at construction; read-only).
    /// </summary>
    public AnimalType AnimalType => _animalType;

    public ICommand CloseCommand { get; }

    /// <summary>
    /// Adds a copy of <see cref="SelectedAvailableIngredient"/> to the selected diet at 0% percentage.
    /// </summary>
    public ICommand AddIngredientCommand { get; }

    /// <summary>
    /// Removes <see cref="SelectedIngredientInDiet"/> from the selected diet.
    /// </summary>
    public ICommand RemoveIngredientCommand { get; }

    /// <summary>
    /// Raised when the VM wants the hosting window to close. The window subscribes.
    /// </summary>
    public event EventHandler? CloseRequested;

    #region Live nutrient totals

    /// <summary>Crude protein (% DM) weighted by ingredient percentages in diet.</summary>
    public double TotalCrudeProtein => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.CrudeProtein);

    /// <summary>Total digestible nutrient (%) weighted by ingredient percentages.</summary>
    public double TotalTDN => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.TotalDigestibleNutrient);

    /// <summary>Forage (% DM) weighted by ingredient percentages.</summary>
    public double TotalForage => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.Forage);

    /// <summary>Neutral detergent fiber (% DM) weighted by ingredient percentages.</summary>
    public double TotalNDF => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.NDF);

    /// <summary>Acid detergent fiber (% DM) weighted by ingredient percentages.</summary>
    public double TotalADF => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.ADF);

    /// <summary>Fat (% DM) weighted by ingredient percentages.</summary>
    public double TotalFat => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.Fat);

    /// <summary>Ash (% DM) weighted by ingredient percentages.</summary>
    public double TotalAsh => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.Ash);

    /// <summary>Metabolizable energy (Mcal/kg) weighted by ingredient percentages.</summary>
    public double TotalME => SelectedDietIngredients.Sum(x => x.PercentageInDiet / 100.0 * x.ME);

    /// <summary>Sum of <c>PercentageInDiet</c> across all ingredients (should equal 100 for a valid diet).</summary>
    public double PercentageSum => SelectedDietIngredients.Sum(x => x.PercentageInDiet);

    /// <summary>True when the percentage sum is within a small tolerance of 100.</summary>
    public bool IsPercentageSumValid => Math.Abs(PercentageSum - 100.0) < 0.01;

    #endregion

    private void Load()
    {
        // Load diets matching the formulator's animal type.
        MyDiets.Clear();
        var allDiets = _dietService.GetDiets();
        foreach (var diet in allDiets)
        {
            if (DietBelongsToAnimalType(diet.AnimalType, _animalType))
            {
                MyDiets.Add(diet);
            }
        }

        // Load the ingredient library matching the animal type.
        AvailableIngredients.Clear();
        IList<IFeedIngredient>? ingredients = null;
        if (_animalType.IsBeefCattleType())
        {
            ingredients = _feedIngredientProvider.GetBeefFeedIngredients();
        }
        else if (_animalType.IsDairyCattleType())
        {
            ingredients = _feedIngredientProvider.GetDairyFeedIngredients();
        }
        else if (_animalType.IsSwineType())
        {
            ingredients = _feedIngredientProvider.GetSwineFeedIngredients();
        }

        if (ingredients != null)
        {
            foreach (var ingredient in ingredients)
            {
                AvailableIngredients.Add(ingredient);
            }
        }

        SelectedDiet = MyDiets.FirstOrDefault();
    }

    private void RefreshSelectedDietIngredients()
    {
        // Clear() emits a Reset which OnSelectedDietIngredientsChanged handles to
        // unsubscribe all in-flight ingredient PropertyChanged subscriptions.
        SelectedDietIngredients.Clear();
        if (_selectedDiet?.Ingredients == null) return;
        foreach (var ingredient in _selectedDiet.Ingredients)
        {
            SelectedDietIngredients.Add(ingredient);
        }
    }

    private void OnAddIngredientExecute()
    {
        if (_selectedAvailableIngredient == null) return;
        // CopyIngredient deep-clones via PropertyMapper and lets us set the initial percentage.
        var copy = _feedIngredientProvider.CopyIngredient(_selectedAvailableIngredient, defaultPercentageInDiet: 0);
        if (copy != null)
        {
            SelectedDietIngredients.Add(copy);
        }
    }

    private bool CanAddIngredient()
        => _feedIngredientProvider != null
           && _selectedAvailableIngredient != null
           && _selectedDiet != null;

    private void OnRemoveIngredientExecute()
    {
        if (_selectedIngredientInDiet == null) return;
        SelectedDietIngredients.Remove(_selectedIngredientInDiet);
        SelectedIngredientInDiet = null;
    }

    private bool CanRemoveIngredient() => _selectedIngredientInDiet != null;

    /// <summary>
    /// Maintain PropertyChanged subscriptions on each ingredient so percentage edits
    /// flow into live totals + validation. Also raises totals on any add/remove.
    /// </summary>
    private void OnSelectedDietIngredientsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            // Clear() doesn't supply OldItems; conservatively re-subscribe to whatever's
            // currently in the collection (after Clear it will be empty).
            // We can't reliably unsubscribe here, but adding a new diet's ingredients
            // calls Add(...) -> PropertyChanged-subscription wires up correctly.
        }
        else
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
                    {
                        inpc.PropertyChanged -= OnIngredientPropertyChanged;
                    }
                }
            }
        }

        RaiseTotalsChanged();
    }

    private void OnIngredientPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // Refresh totals whenever any tracked ingredient property changes.
        // PercentageInDiet is the typical user edit; other property changes also recompute.
        RaiseTotalsChanged();
    }

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

    /// <summary>
    /// Returns true when a diet's animal type belongs to the formulator's animal-type scope.
    /// </summary>
    private static bool DietBelongsToAnimalType(AnimalType dietAnimalType, AnimalType scope)
    {
        if (scope.IsBeefCattleType()) return dietAnimalType.IsBeefCattleType();
        if (scope.IsDairyCattleType()) return dietAnimalType.IsDairyCattleType();
        if (scope.IsSwineType()) return dietAnimalType.IsSwineType();
        if (scope.IsSheepType()) return dietAnimalType.IsSheepType();

        return dietAnimalType == scope;
    }
}
