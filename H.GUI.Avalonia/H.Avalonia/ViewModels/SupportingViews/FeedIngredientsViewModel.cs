using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using H.Core.Enumerations;
using H.Core.Providers.Feed;
using H.Core.Services.StorageService;
using Prism.Regions;

namespace H.Avalonia.ViewModels.SupportingViews
{
    public class FeedIngredientsViewModel : ViewModelBase
    {
        #region Fields

        private readonly IFeedIngredientProvider _feedIngredientProvider = null!;
        private ObservableCollection<IFeedIngredient> _ingredients = null!;
        private IFeedIngredient _selectedIngredient = null!;
        private string _searchText = string.Empty;
        private AnimalType _selectedAnimalType;

        #endregion

        #region Constructors

        public FeedIngredientsViewModel()
        {
            // Design-time constructor
            Ingredients = new ObservableCollection<IFeedIngredient>();
            SelectedAnimalType = AnimalType.Beef;
        }

        public FeedIngredientsViewModel(
            IRegionManager regionManager, 
            IStorageService storageService,
            IFeedIngredientProvider feedIngredientProvider) : base(regionManager, storageService)
        {
            _feedIngredientProvider = feedIngredientProvider ?? throw new System.ArgumentNullException(nameof(feedIngredientProvider));
            Ingredients = new ObservableCollection<IFeedIngredient>();
            SelectedAnimalType = AnimalType.Beef;
            
            LoadIngredients();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Collection of all available feed ingredients to display in the grid
        /// </summary>
        public ObservableCollection<IFeedIngredient> Ingredients
        {
            get => _ingredients;
            set => SetProperty(ref _ingredients, value);
        }

        /// <summary>
        /// Currently selected ingredient in the grid
        /// </summary>
        public IFeedIngredient SelectedIngredient
        {
            get => _selectedIngredient;
            set => SetProperty(ref _selectedIngredient, value);
        }

        /// <summary>
        /// Search text for filtering ingredients
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set 
            {
                if (SetProperty(ref _searchText, value))
                {
                    FilterIngredients();
                }
            }
        }

        /// <summary>
        /// Currently selected animal type to show ingredients for
        /// </summary>
        public AnimalType SelectedAnimalType
        {
            get => _selectedAnimalType;
            set 
            {
                if (SetProperty(ref _selectedAnimalType, value))
                {
                    LoadIngredients();
                }
            }
        }

        /// <summary>
        /// List of animal types that have feed ingredient data available
        /// </summary>
        public IList<AnimalType> AvailableAnimalTypes
        {
            get
            {
                return new List<AnimalType>
                {
                    AnimalType.Beef,      // Maps to ComponentCategory.BeefProduction
                    AnimalType.Dairy,     // Maps to ComponentCategory.Dairy
                    AnimalType.Swine      // Maps to ComponentCategory.Swine
                    // Note: Other animal types (Sheep, Poultry, etc.) don't have feed ingredient data yet
                };
            }
        }

        #endregion

        #region Public Methods

        public override void InitializeViewModel()
        {
            base.InitializeViewModel();
            LoadIngredients();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads all available ingredients for the selected animal type
        /// </summary>
        private void LoadIngredients()
        {
            if (_feedIngredientProvider == null) return;

            var allIngredients = _feedIngredientProvider.GetAllIngredientsForAnimalType(SelectedAnimalType);
            
            Ingredients.Clear();
            foreach (var ingredient in allIngredients)
            {
                Ingredients.Add(ingredient);
            }
        }

        /// <summary>
        /// Filters ingredients based on search text
        /// </summary>
        private void FilterIngredients()
        {
            if (_feedIngredientProvider == null) return;

            var allIngredients = _feedIngredientProvider.GetAllIngredientsForAnimalType(SelectedAnimalType);
            
            Ingredients.Clear();

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                // Show all ingredients if no search text
                foreach (var ingredient in allIngredients)
                {
                    Ingredients.Add(ingredient);
                }
            }
            else
            {
                // Filter ingredients based on ingredient type name or other properties
                var filteredIngredients = allIngredients.Where(i => 
                    (i.IngredientType.ToString().Contains(SearchText, System.StringComparison.OrdinalIgnoreCase)) ||
                    (i.IngredientTypeString?.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) == true) ||
                    (i.IFN?.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) == true)).ToList();

                foreach (var ingredient in filteredIngredients)
                {
                    Ingredients.Add(ingredient);
                }
            }
        }

        #endregion
    }
}