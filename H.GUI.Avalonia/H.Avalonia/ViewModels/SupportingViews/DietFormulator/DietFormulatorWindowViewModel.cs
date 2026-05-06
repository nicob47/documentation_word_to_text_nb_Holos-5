using System.Collections.Generic;
using System.Collections.ObjectModel;
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
/// is scoped to the animal group that launched it (beef-only initially; same code
/// path supports dairy and swine when those launchers are added).
/// Phase 1.3 — read-only browsing only. Editing, custom diets, and persistence
/// land in later phases.
/// </summary>
public class DietFormulatorWindowViewModel : ViewModelBase
{
    private readonly IDietService _dietService;
    private readonly IFeedIngredientProvider _feedIngredientProvider;
    private readonly AnimalType _animalType;

    private IDietDto? _selectedDiet;

    public DietFormulatorWindowViewModel()
    {
        // Design-time constructor.
        _dietService = null!;
        _feedIngredientProvider = null!;
        _animalType = AnimalType.NotSelected;
        MyDiets = new ObservableCollection<IDietDto>();
        AvailableIngredients = new ObservableCollection<IFeedIngredient>();
        SelectedDietIngredients = new ObservableCollection<IFeedIngredient>();
        CloseCommand = new DelegateCommand(() => CloseRequested?.Invoke(this, System.EventArgs.Empty));
    }

    public DietFormulatorWindowViewModel(
        IDietService dietService,
        IFeedIngredientProvider feedIngredientProvider,
        AnimalType animalType)
    {
        _dietService = dietService ?? throw new System.ArgumentNullException(nameof(dietService));
        _feedIngredientProvider = feedIngredientProvider ?? throw new System.ArgumentNullException(nameof(feedIngredientProvider));
        _animalType = animalType;

        MyDiets = new ObservableCollection<IDietDto>();
        AvailableIngredients = new ObservableCollection<IFeedIngredient>();
        SelectedDietIngredients = new ObservableCollection<IFeedIngredient>();

        CloseCommand = new DelegateCommand(() => CloseRequested?.Invoke(this, System.EventArgs.Empty));

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
            }
        }
    }

    /// <summary>
    /// Animal type the formulator is scoped to (set at construction; read-only).
    /// </summary>
    public AnimalType AnimalType => _animalType;

    /// <summary>
    /// Closes the formulator window. The window's code-behind subscribes to
    /// <see cref="CloseRequested"/> so the VM can stay UI-agnostic.
    /// </summary>
    public ICommand CloseCommand { get; }

    /// <summary>
    /// Raised when the VM wants the hosting window to close. The window subscribes.
    /// </summary>
    public event System.EventHandler? CloseRequested;

    private void Load()
    {
        // Load diets matching the formulator's animal type.
        // IDietService.GetDiets returns the full set; we filter client-side.
        // Ranges (e.g., AnimalType.Beef) match the same parent category as
        // specific subtypes (BeefCow, BeefBackgrounder, etc.) via IsBeefCattleType / IsDairyCattleType / IsSwineType extensions.
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

        // Auto-select the first diet so SelectedDietIngredients is populated.
        SelectedDiet = MyDiets.FirstOrDefault();
    }

    private void RefreshSelectedDietIngredients()
    {
        SelectedDietIngredients.Clear();
        if (_selectedDiet?.Ingredients == null) return;
        foreach (var ingredient in _selectedDiet.Ingredients)
        {
            SelectedDietIngredients.Add(ingredient);
        }
    }

    /// <summary>
    /// Returns true when a diet's animal type belongs to the formulator's animal-type scope.
    /// Beef diets target subtypes like BeefCow / BeefBackgrounder / BeefFinisher; the
    /// formulator launched from a beef component should show all beef-family diets.
    /// </summary>
    private static bool DietBelongsToAnimalType(AnimalType dietAnimalType, AnimalType scope)
    {
        if (scope.IsBeefCattleType()) return dietAnimalType.IsBeefCattleType();
        if (scope.IsDairyCattleType()) return dietAnimalType.IsDairyCattleType();
        if (scope.IsSwineType()) return dietAnimalType.IsSwineType();
        if (scope.IsSheepType()) return dietAnimalType.IsSheepType();

        // Fallback: exact match for any other category (poultry, other animals, etc.)
        return dietAnimalType == scope;
    }
}
