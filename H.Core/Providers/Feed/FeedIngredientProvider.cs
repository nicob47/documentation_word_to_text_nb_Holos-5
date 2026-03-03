#region Imports

using System.Globalization;
using H.Content;
using H.Core.Converters;
using H.Infrastructure;
using AutoMapper;
using H.Core.Enumerations;
using H.Core.Mappers;
using H.Core.Models;
using H.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Prism.Ioc;

#endregion

namespace H.Core.Providers.Feed
{
    /// <summary>
    /// Provides feed ingredient data and diet recipes for various animal types and diet types.
    /// This class loads, manages, and caches feed ingredient information from data sources,
    /// and supplies ingredient breakdowns for specific animal and diet combinations.
    /// It supports efficient retrieval of feed ingredient lists, leveraging caching to improve performance,
    /// and is used as a core service for ration formulation and feed analysis in livestock management applications.
    /// </summary>
    public class FeedIngredientProvider : IFeedIngredientProvider
    {
        #region Private Classes

        /// <summary>
        /// Represents a single ingredient and its percentage in a diet formulation.
        /// Used to describe the breakdown of a diet recipe by ingredient type and proportion.
        /// </summary>
        internal class IngredientBreakdown(IngredientType ingredientType, double percentageInDiet)
        {
            public IngredientType IngredientType { get; set; } = ingredientType;
            public double PercentageInDiet { get; set; } = percentageInDiet;
        }

        /// <summary>
        /// Represents a list of ingredient breakdowns for a specific animal and diet type combination.
        /// Used to define a complete diet recipe for a given animal and diet.
        /// </summary>
        internal class IngredientList(DietType dietType, AnimalType animalType)
        {
            public DietType DietType { get; set; } = dietType;
            public AnimalType AnimalType { get; set; } = animalType;
            public List<IngredientBreakdown> Ingredients { get; set; } = new();
        }

        #endregion

        #region Fields

        private readonly IMapper _feedIngredientMapper;
        private readonly ILogger _logger = null!;
        private readonly ICacheService _cacheService = null!;

        private readonly Dictionary<ComponentCategory, IReadOnlyList<IFeedIngredient>> _ingredientsByAnimalCategory;
        private readonly Dictionary<Tuple<ComponentCategory, IngredientType>, IFeedIngredient> _ingredientDictionary;
        private readonly IReadOnlyCollection<IngredientList> _ingredientLists;

        #endregion

        #region Constructors

        public FeedIngredientProvider()
        {
            _ingredientsByAnimalCategory = new Dictionary<ComponentCategory, IReadOnlyList<IFeedIngredient>>();
            _ingredientDictionary = new Dictionary<Tuple<ComponentCategory, IngredientType>, IFeedIngredient>();
            _ingredientLists = this.CreateIngredientLists();

            this.BuildDictionary();
            this.CreateIngredientLists();

            _feedIngredientMapper = new Mapper(new MapperConfiguration(expression =>
            {
                expression.CreateMap<FeedIngredient, FeedIngredient>();
            }));
        }

        public FeedIngredientProvider(ILogger logger, IContainerProvider containerProvider, ICacheService cacheService) : this()
        {
            if (cacheService != null)
            {
                _cacheService = cacheService;
            }
            else
            {
                throw new ArgumentNullException(nameof(cacheService));
            }

            if (logger != null)
            {
                _logger = logger;
            }
            else
            {
                throw new ArgumentNullException(nameof(logger));
            }

            _feedIngredientMapper = containerProvider.Resolve<IMapper>(nameof(FeedIngredientToFeedIngredientMapper));
        }

        #endregion

        #region Properties


        #endregion

        #region Public Methods

        public IReadOnlyCollection<IFeedIngredient> GetIngredientsForDiet(AnimalType animalType, DietType dietType)
        {
            // Build a unique cache key for this animal and diet type combination
            var cacheKey = $"{nameof(FeedIngredientProvider)}_{nameof(GetIngredientsForDiet)}_" +
                            $"{animalType}_{dietType}";

            // Try to retrieve the result from cache
            IReadOnlyCollection<IFeedIngredient>? cachedResult = null;
            if (_cacheService != null)
            {
                cachedResult = _cacheService.Get<IReadOnlyCollection<IFeedIngredient>>(cacheKey);
                if (cachedResult != null)
                {
                    // Return cached result if available
                    return cachedResult;
                }
            }

            // Find the recipe for the given animal and diet type
            var result = _ingredientLists.SingleOrDefault(x => x.DietType == dietType && x.AnimalType == animalType);
            IReadOnlyCollection<IFeedIngredient> collection;
            if (result != null)
            {
                var list = new List<IFeedIngredient>();
                // For each ingredient in the recipe, get the ingredient and set its percentage
                foreach (var ingredientBreakdown in result.Ingredients)
                {
                    var category = animalType.GetComponentCategoryFromAnimalType();
                    list.Add(this.GetIngredient(ingredientBreakdown.IngredientType, ingredientBreakdown.PercentageInDiet, category));
                }
                collection = list;
            }
            else
            {
                // Log and return an empty list if no recipe is found
                _logger.LogError($"No ingredients for {animalType} and {dietType}");
                collection = new List<IFeedIngredient>();
            }

            // Store the result in cache for future requests
            _cacheService?.Set<IReadOnlyCollection<IFeedIngredient>>(key: cacheKey, value: collection, options: null);

            return collection;
        }

        public FeedIngredient? CopyIngredient(IFeedIngredient ingredient, double defaultPercentageInDiet)
        {
            if (ingredient != null)
            {
                var copiedIngredient = new FeedIngredient();

                _feedIngredientMapper.Map(ingredient, copiedIngredient);

                copiedIngredient.PercentageInDiet = defaultPercentageInDiet;

                return copiedIngredient;
            }
            else
            {
                _logger.LogError("Ingredient is null, cannot perform copy");

                return null;
            }
        }

        /// <summary>
        /// Gets the list of beef feed ingredients available for beef production.
        /// </summary>
        public IList<IFeedIngredient>? GetBeefFeedIngredients()
        {
            return _ingredientsByAnimalCategory[ComponentCategory.BeefProduction] as IList<IFeedIngredient>;
        }

        /// <summary>
        /// Gets the list of dairy feed ingredients available for dairy production.
        /// </summary>
        public IList<IFeedIngredient>? GetDairyFeedIngredients()
        {
            return _ingredientsByAnimalCategory[ComponentCategory.Dairy] as IList<IFeedIngredient>;
        }

        /// <summary>
        /// Gets the list of swine feed ingredients available for swine production.
        /// </summary>
        public IList<IFeedIngredient>? GetSwineFeedIngredients()
        {
            return _ingredientsByAnimalCategory[ComponentCategory.Swine] as IList<IFeedIngredient>;
        }

        /// <summary>
        /// Gets all feed ingredients available for a given animal type.
        /// Maps the animal type to its corresponding component category and returns the appropriate ingredients.
        /// </summary>
        /// <param name="animalType">The animal type to get ingredients for</param>
        /// <returns>A read-only collection of feed ingredients for the specified animal type</returns>
        public IReadOnlyCollection<IFeedIngredient> GetAllIngredientsForAnimalType(AnimalType animalType)
        {
            var componentCategory = animalType.GetComponentCategoryFromAnimalType();
            
            if (_ingredientsByAnimalCategory.TryGetValue(componentCategory, out var ingredients))
            {
                return ingredients;
            }
            
            _logger?.LogWarning($"No ingredients found for animal type {animalType} (component category: {componentCategory})");
            return new List<IFeedIngredient>();
        }

        #endregion

        #region Private Methods      

        /// <summary>
        /// Retrieves a feed ingredient for the specified component category and ingredient type.
        /// Looks up the ingredient in the internal dictionary and returns it if found; otherwise, logs an error and returns null.
        /// </summary>
        /// <param name="componentCategory">The component category (e.g., BeefProduction, Dairy, Swine).</param>
        /// <param name="ingredientType">The type of ingredient to retrieve.</param>
        /// <returns>The <see cref="FeedIngredient"/> if found; otherwise, null.</returns>
        private FeedIngredient? Get(ComponentCategory componentCategory, IngredientType ingredientType)
        {
            var tuple = new Tuple<ComponentCategory, IngredientType>(componentCategory, ingredientType);
            bool exists = _ingredientDictionary.TryGetValue(tuple, out IFeedIngredient? ingredient);
            if (exists)
            {
                return (FeedIngredient?)ingredient;
            }
            else
            {
                _logger.LogError($"The ingredient type '{ingredientType}' does not exist in the category '{componentCategory}'.");

                return null;
            }
        }

        private IFeedIngredient GetIngredient(IngredientType ingredientType, double percentageInDiet, ComponentCategory componentCategory)
        {
            var ingredient = this.Get(componentCategory, ingredientType);
            var copy = this.CopyIngredient(ingredient!, percentageInDiet);

            return copy!;
        }

        /// <summary>
        /// Creates and returns a collection of predefined ingredient lists for various animal and diet type combinations.
        /// Each ingredient list represents a diet recipe, specifying the ingredients and their percentages for a specific animal and diet.
        /// </summary>
        /// <returns>A read-only collection of <see cref="IngredientList"/> objects representing diet recipes.</returns>
        private IReadOnlyCollection<IngredientList> CreateIngredientLists()
        {
            return new List<IngredientList>()
            {
                #region Beef cow

                new(DietType.LowEnergyAndProtein, AnimalType.BeefCow)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.NativePrairieHay, 100)
                    ]
                },

                new(DietType.MediumEnergyAndProtein, AnimalType.BeefCow)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.AlfalfaHay, 32),
                        new IngredientBreakdown(IngredientType.MeadowHay, 65),
                        new IngredientBreakdown(IngredientType.BarleyGrain, 3),
                    ]
                },

                new(DietType.HighEnergyAndProtein, AnimalType.BeefCow)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.OrchardgrassHay, 60),
                        new IngredientBreakdown(IngredientType.AlfalfaHay, 20),
                        new IngredientBreakdown(IngredientType.BarleyGrain, 20),
                    ]
                },

                #endregion

                #region Beef finisher

                new(DietType.BarleyGrainBased, AnimalType.BeefFinisher)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.BarleySilage, 10),
                        new IngredientBreakdown(IngredientType.BarleyGrain, 90),
                    ]
                },

                new(DietType.CornGrainBased, AnimalType.BeefFinisher)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.BarleySilage, 10),
                        new IngredientBreakdown(IngredientType.CornGrain, 88.7),
                        new IngredientBreakdown(IngredientType.Urea, 1.3),
                    ]
                },

                #endregion

                #region Beef backgrounding

                new(DietType.SlowGrowth, AnimalType.BeefBackgrounder)
                {
                    Ingredients = 
                    [
                        new IngredientBreakdown(IngredientType.BarleySilage, 65),
                        new IngredientBreakdown(IngredientType.CornGrain, 35),
                    ]
                },

                new(DietType.MediumGrowth, AnimalType.BeefBackgrounder)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.BarleySilage, 65),
                        new IngredientBreakdown(IngredientType.BarleyGrain, 35),
                    ]
                },

                #endregion

                #region Dairy lactating cow

                new(DietType.LegumeForageBased, AnimalType.DairyLactatingCow)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.LegumesForageHayMature, 22),
                        new IngredientBreakdown(IngredientType.LegumesForageSilageAllSamples, 22),
                        new IngredientBreakdown(IngredientType.BarleyGrainRolled, 45),
                        new IngredientBreakdown(IngredientType.CanolaMealMechExtracted, 2),
                        new IngredientBreakdown(IngredientType.SoybeanHulls, 5),
                        new IngredientBreakdown(IngredientType.MolassesBeetSugar, 1),
                        new IngredientBreakdown(IngredientType.CornYellowGlutenFeedDried, 3),
                    ]
                },

                new(DietType.BarleySilageBased, AnimalType.DairyLactatingCow)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.BarleySilageHeaded, 49),
                        new IngredientBreakdown(IngredientType.LegumesForageHayMidMaturity, 5),
                        new IngredientBreakdown(IngredientType.BarleyGrainRolled, 17),
                        new IngredientBreakdown(IngredientType.CornYellowGrainRolledHighMoisture, 13),
                        new IngredientBreakdown(IngredientType.CanolaMealMechExtracted, 5),
                        new IngredientBreakdown(IngredientType.SoybeanMealExpellers, 4),
                        new IngredientBreakdown(IngredientType.BeetSugarPulpDried, 2),
                        new IngredientBreakdown(IngredientType.MolassesSugarCane, 1),
                    ]
                },

                new(DietType.CornSilageBased, AnimalType.DairyLactatingCow)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.CornYellowSilageNormal, 55),
                        new IngredientBreakdown(IngredientType.GrassesCoolHayMidMaturity, 6),
                        new IngredientBreakdown(IngredientType.CornYellowGrainGraundDry, 11),
                        new IngredientBreakdown(IngredientType.SoybeanMealSolvent48, 12),
                        new IngredientBreakdown(IngredientType.SoybeanHulls, 5),
                        new IngredientBreakdown(IngredientType.CornYellowGlutenFeedDried, 11),
                    ]
                },

                #endregion

                #region Dairy dry cow

                new(DietType.CloseUp, AnimalType.DairyDryCow)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.CornYellowSilageNormal, 48),
                        new IngredientBreakdown(IngredientType.GrassLegumeMixturesPredomLegumesSilageMidMaturity, 23),
                        new IngredientBreakdown(IngredientType.CornYellowGrainCrackedDry, 12),
                        new IngredientBreakdown(IngredientType.CanolaMealMechExtracted, 9),
                        new IngredientBreakdown(IngredientType.CornYellowGlutenMealDried, 8),
                    ]
                },

                new(DietType.FarOff, AnimalType.DairyDryCow)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.GrassLegumeMixturesPredomLegumesHayMidMaturity, 57),
                        new IngredientBreakdown(IngredientType.BarleyGrainRolled, 38),
                        new IngredientBreakdown(IngredientType.SoybeanMealExpellers, 5),
                    ]
                },

                #endregion

                #region Dairy heifer

                new(DietType.HighFiber, AnimalType.DairyHeifers)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.GrassesCoolHayMature, 50),
                        new IngredientBreakdown(IngredientType.BarleyGrainRolled, 45),
                        new IngredientBreakdown(IngredientType.SoybeanMealExpellers, 5),
                    ]
                },

                new(DietType.LowFiber, AnimalType.DairyHeifers)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.CornYellowSilageNormal, 50),
                        new IngredientBreakdown(IngredientType.BarleyGrainRolled, 42),
                        new IngredientBreakdown(IngredientType.CanolaMealMechExtracted, 8),
                    ]
                },

                #endregion

                #region Swine

                new(DietType.Gestation, AnimalType.Swine)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.WheatBran, 14),
                        new IngredientBreakdown(IngredientType.WheatShorts, 3),
                        new IngredientBreakdown(IngredientType.Barley, 62.2),
                        new IngredientBreakdown(IngredientType.SoybeanMealDehulledExpelled, 4),
                        new IngredientBreakdown(IngredientType.CanolaMealExpelled, 3),
                        new IngredientBreakdown(IngredientType.FieldPeas, 6),
                        new IngredientBreakdown(IngredientType.SugarBeetPulp, 5.6),
                        new IngredientBreakdown(IngredientType.CanolaFullFat, 0.4),
                    ]
                },

                new(DietType.Lactation, AnimalType.Swine)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.WheatBran, 41),
                        new IngredientBreakdown(IngredientType.Barley, 21.8),
                        new IngredientBreakdown(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, 9),
                        new IngredientBreakdown(IngredientType.SoybeanMealDehulledExpelled, 9),
                        new IngredientBreakdown(IngredientType.CanolaMealExpelled, 5),
                        new IngredientBreakdown(IngredientType.FieldPeas, 10),
                        new IngredientBreakdown(IngredientType.CanolaFullFat, 1.8),
                    ]
                },

                new(DietType.NurseryWeanersStarter1, AnimalType.Swine)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.WheatBran, 39),
                        new IngredientBreakdown(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, 11.38),
                        new IngredientBreakdown(IngredientType.SoybeanMealDehulledExpelled, 20),
                        new IngredientBreakdown(IngredientType.FieldPeas, 11),
                        new IngredientBreakdown(IngredientType.CanolaFullFat, 1.2),
                        new IngredientBreakdown(IngredientType.WheyPermeateLactose80, 10),
                        new IngredientBreakdown(IngredientType.FishMealCombined, 5),
                    ]
                },

                new(DietType.NurseryWeanersStarter2, AnimalType.Swine)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.WheatBran, 33.76),
                        new IngredientBreakdown(IngredientType.Barley, 20),
                        new IngredientBreakdown(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, 10),
                        new IngredientBreakdown(IngredientType.SoybeanMealDehulledExpelled, 14),
                        new IngredientBreakdown(IngredientType.CanolaMealExpelled, 7),
                        new IngredientBreakdown(IngredientType.FieldPeas, 10),
                        new IngredientBreakdown(IngredientType.CanolaFullFat, 2.5),
                    ]
                },

                new(DietType.GrowerFinisherDiet1, AnimalType.Swine)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.WheatBran, 32.53),
                        new IngredientBreakdown(IngredientType.Barley, 24),
                        new IngredientBreakdown(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, 12),
                        new IngredientBreakdown(IngredientType.SoybeanMealDehulledExpelled, 10),
                        new IngredientBreakdown(IngredientType.CanolaMealExpelled, 6),
                        new IngredientBreakdown(IngredientType.FieldPeas, 12),
                        new IngredientBreakdown(IngredientType.CanolaFullFat, 1.2),
                    ]
                },

                new(DietType.GrowerFinisherDiet2, AnimalType.Swine)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.WheatBran, 32.2),
                        new IngredientBreakdown(IngredientType.Barley, 26.79),
                        new IngredientBreakdown(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, 12),
                        new IngredientBreakdown(IngredientType.SoybeanMealDehulledExpelled, 8),
                        new IngredientBreakdown(IngredientType.CanolaMealExpelled, 8),
                        new IngredientBreakdown(IngredientType.FieldPeas, 10),
                        new IngredientBreakdown(IngredientType.CanolaFullFat, 1.1),
                    ]
                },

                new(DietType.GrowerFinisherDiet3, AnimalType.Swine)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.WheatBran, 25.2),
                        new IngredientBreakdown(IngredientType.WheatShorts, 6.3),
                        new IngredientBreakdown(IngredientType.Barley, 28),
                        new IngredientBreakdown(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, 12),
                        new IngredientBreakdown(IngredientType.SoybeanMealDehulledExpelled, 8),
                        new IngredientBreakdown(IngredientType.CanolaMealExpelled, 8),
                        new IngredientBreakdown(IngredientType.FieldPeas, 10),
                        new IngredientBreakdown(IngredientType.CanolaFullFat, 0.8),
                    ]
                },

                new(DietType.GrowerFinisherDiet4, AnimalType.Swine)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.WheatBran, 19.1),
                        new IngredientBreakdown(IngredientType.WheatShorts, 11.8),
                        new IngredientBreakdown(IngredientType.Barley, 30),
                        new IngredientBreakdown(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, 15),
                        new IngredientBreakdown(IngredientType.SoybeanMealDehulledExpelled, 6),
                        new IngredientBreakdown(IngredientType.CanolaMealExpelled, 8),
                        new IngredientBreakdown(IngredientType.FieldPeas, 8),
                        new IngredientBreakdown(IngredientType.CanolaFullFat, 0.8),
                    ]
                },

                new(DietType.GiltDeveloperDiet, AnimalType.Swine)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.WheatBran, 35),
                        new IngredientBreakdown(IngredientType.WheatShorts, 10.1),
                        new IngredientBreakdown(IngredientType.Barley, 20),
                        new IngredientBreakdown(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, 12.1),
                        new IngredientBreakdown(IngredientType.CanolaMealExpelled, 15),
                        new IngredientBreakdown(IngredientType.FieldPeas, 5),
                        new IngredientBreakdown(IngredientType.CanolaFullFat, 0.5),
                    ]
                },

                new(DietType.Boars, AnimalType.Swine)
                {
                    Ingredients =
                    [
                        new IngredientBreakdown(IngredientType.WheatBran, 38),
                        new IngredientBreakdown(IngredientType.WheatShorts, 2),
                        new IngredientBreakdown(IngredientType.Barley, 20),
                        new IngredientBreakdown(IngredientType.CornDistillersDriedGrainsSolublesGreaterThanSixAndLessThanNinePercentOil, 10.6),
                        new IngredientBreakdown(IngredientType.SoybeanMealDehulledExpelled, 7.1),
                        new IngredientBreakdown(IngredientType.CanolaMealExpelled, 15),
                        new IngredientBreakdown(IngredientType.FieldPeas, 5),
                        new IngredientBreakdown(IngredientType.CanolaFullFat, 0.5),
                    ]
                },

                #endregion
            };
        }

        /// <summary>
        /// Populates the internal dictionaries that map component categories and ingredient types to feed ingredients.
        /// Loads feed ingredient data for beef, dairy, and swine from their respective sources,
        /// and organizes them for efficient lookup by category and ingredient type.
        /// </summary>
        private void BuildDictionary()
        {
            var beeFeedIngredients = this.ReadBeefFile().ToList();
            _ingredientsByAnimalCategory.Add(ComponentCategory.BeefProduction, new List<IFeedIngredient>(beeFeedIngredients));
            foreach (var beeFeedIngredient in beeFeedIngredients)
            {
                var tuple = new Tuple<ComponentCategory, IngredientType>(ComponentCategory.BeefProduction, beeFeedIngredient.IngredientType);
                _ingredientDictionary.Add(tuple, beeFeedIngredient);
            }

            var dairyIngredients = this.ReadDairyFile().ToList();
            _ingredientsByAnimalCategory.Add(ComponentCategory.Dairy, new List<IFeedIngredient>(dairyIngredients));
            foreach (var dairyIngredient in dairyIngredients)
            {
                var tuple = new Tuple<ComponentCategory, IngredientType>(ComponentCategory.Dairy, dairyIngredient.IngredientType);
                _ingredientDictionary.Add(tuple, dairyIngredient);
            }

            var swineFeedIngredients = this.ReadSwineFile().ToList();
            _ingredientsByAnimalCategory.Add(ComponentCategory.Swine, new List<IFeedIngredient>(swineFeedIngredients));
            foreach (var swineFeedIngredient in swineFeedIngredients)
            {
                var tuple = new Tuple<ComponentCategory, IngredientType>(ComponentCategory.Swine, swineFeedIngredient.IngredientType);
                _ingredientDictionary.Add(tuple, swineFeedIngredient);
            }
        }

        /// <summary>
        /// Reads swine feed ingredient data from a CSV resource file and parses each line into a <see cref="FeedIngredient"/> object.
        /// Converts string values to their appropriate types and populates all relevant properties for each ingredient.
        /// Adds a constant Urea ingredient at the end of the list.
        /// </summary>
        /// <returns>A collection of <see cref="FeedIngredient"/> objects representing swine feed ingredients.</returns>
        private IEnumerable<FeedIngredient> ReadSwineFile()
        {
            var cultureInfo = InfrastructureConstants.EnglishCultureInfo;
            var fileLines = CsvResourceReader.GetFileLines(CsvResourceNames.SwineFeedIngredientList)!;
            var feedNameStringConverter = new FeedIngredientStringConverter();
            double parseResult = 0;

            var result = new List<FeedIngredient>();

            foreach (var line in fileLines.Skip(1))
            {
                var feedData = new FeedIngredient
                {
                    IngredientType = feedNameStringConverter.Convert(line[1]),
                    AAFCO = line[2],
                    AAFCO2010 = line[3],
                    IFN = line[4],
                    DryMatter = double.TryParse(line[5], NumberStyles.Any, cultureInfo, out parseResult)
                        ? parseResult
                        : 0,
                    CrudeProtein = double.TryParse(line[6], NumberStyles.Any, cultureInfo, out parseResult)
                        ? parseResult
                        : 0,
                    CrudeFiber = double.TryParse(line[7], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    EE = double.TryParse(line[8], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    AcidEtherExtract = double.TryParse(line[9], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Ash = double.TryParse(line[10], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    GrossEnergy = double.TryParse(line[11], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    DeSwine = double.TryParse(line[12], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ME =
                        double.TryParse(line[13], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    NE = double.TryParse(line[14], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Lactose =
                        double.TryParse(line[15], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Sucrose = double.TryParse(line[16], NumberStyles.Any, cultureInfo, out parseResult)
                        ? parseResult
                        : 0,
                    Raffinose = double.TryParse(line[17], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Stachyose = double.TryParse(line[18], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Verbascose = double.TryParse(line[19], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Oligosaccharides = double.TryParse(line[20], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Starch = double.TryParse(line[21], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    NDF = double.TryParse(line[22], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ADF = double.TryParse(line[23], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Hemicellulose = double.TryParse(line[24], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ADL = double.TryParse(line[25], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    TDF = double.TryParse(line[26], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    IDF = double.TryParse(line[27], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    SDF = double.TryParse(line[28], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ARG = double.TryParse(line[30], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    HIS = double.TryParse(line[31], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ILE = double.TryParse(line[32], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    LEU = double.TryParse(line[33], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    LYS = double.TryParse(line[34], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    MET = double.TryParse(line[35], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    PHE = double.TryParse(line[36], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    THR = double.TryParse(line[37], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    TRP = double.TryParse(line[38], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    VAL = double.TryParse(line[39], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ALA = double.TryParse(line[40], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ASP = double.TryParse(line[41], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    CYS = double.TryParse(line[42], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    GLU = double.TryParse(line[43], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    GLY = double.TryParse(line[44], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    PRO = double.TryParse(line[45], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    SER = double.TryParse(line[46], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    TYR = double.TryParse(line[47], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    CpDigestAID = double.TryParse(line[48], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ArgDigestAID = double.TryParse(line[49], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    HisDigestAID = double.TryParse(line[50], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    IleDigestAID = double.TryParse(line[51], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    LeuDigestAID = double.TryParse(line[52], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    LysDigestAID = double.TryParse(line[53], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    MetDigestAID = double.TryParse(line[54], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    PheDigestAID = double.TryParse(line[55], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ThrDigestAID = double.TryParse(line[56], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    TrpDigestAID = double.TryParse(line[57], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ValDigestAID = double.TryParse(line[58], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    AlaDigestAID = double.TryParse(line[59], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    AspDigestAID = double.TryParse(line[60], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    CysDigestAID = double.TryParse(line[61], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    GluDigestAID = double.TryParse(line[62], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    GlyDigestAID = double.TryParse(line[63], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ProDigestAID = double.TryParse(line[64], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    SerDigestAID = double.TryParse(line[65], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    TyrDigestAID = double.TryParse(line[66], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    CpDigestSID = double.TryParse(line[67], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ArgDigestSID = double.TryParse(line[68], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    HisDigestSID = double.TryParse(line[69], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    IleDigestSID = double.TryParse(line[70], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    LeuDigestSID = double.TryParse(line[71], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    LysDigestSID = double.TryParse(line[72], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    MetDigestSID = double.TryParse(line[73], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    PheDigestSID = double.TryParse(line[74], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ThrDigestSID = double.TryParse(line[75], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    TrpDigestSID = double.TryParse(line[76], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ValDigestSID = double.TryParse(line[77], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    AlaDigestSID = double.TryParse(line[78], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    AspDigestSID = double.TryParse(line[79], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    CysDigestSID = double.TryParse(line[80], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    GluDigestSID = double.TryParse(line[81], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    GlyDigestSID = double.TryParse(line[82], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ProDigestSID = double.TryParse(line[83], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    SerDigestSID = double.TryParse(line[84], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    TyrDigestSID = double.TryParse(line[85], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Ca = double.TryParse(line[86], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Cl = double.TryParse(line[87], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    K = double.TryParse(line[88], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Mg = double.TryParse(line[89], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Na = double.TryParse(line[90], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    P = double.TryParse(line[91], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    S = double.TryParse(line[92], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    CR = double.TryParse(line[93], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Cu = double.TryParse(line[94], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Fe = double.TryParse(line[95], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    I = double.TryParse(line[96], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Mn = double.TryParse(line[97], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Se = double.TryParse(line[98], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Zn = double.TryParse(line[99], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    PhytatePhosphorus = double.TryParse(line[100], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ATTDPhosphorus = double.TryParse(line[101], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    STTDPhosphorus = double.TryParse(line[102], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    BetaCarotene = double.TryParse(line[103], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    VitE = double.TryParse(line[104], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    VitB6 = double.TryParse(line[105], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    VitB12 = double.TryParse(line[106], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Biotin = double.TryParse(line[107], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Folacin = double.TryParse(line[108], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Niacin = double.TryParse(line[109], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    PantothenicAcid = double.TryParse(line[110], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Riboflavin = double.TryParse(line[111], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Thiamin = double.TryParse(line[112], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Choline = double.TryParse(line[113], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    EtherExtractDup = double.TryParse(line[114], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C120 = double.TryParse(line[115], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C140 = double.TryParse(line[116], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C160 = double.TryParse(line[117], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C161 = double.TryParse(line[118], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C180 = double.TryParse(line[119], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C181 = double.TryParse(line[120], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C182 = double.TryParse(line[121], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C183 = double.TryParse(line[122], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C184 = double.TryParse(line[123], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C200 = double.TryParse(line[124], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C201 = double.TryParse(line[125], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C204 = double.TryParse(line[126], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C205 = double.TryParse(line[127], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C220 = double.TryParse(line[128], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C221 = double.TryParse(line[129], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C225 = double.TryParse(line[130], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C226 = double.TryParse(line[131], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    C240 = double.TryParse(line[132], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    SFA = double.TryParse(line[133], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    MUFA = double.TryParse(line[134], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    PUFA = double.TryParse(line[135], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    IV = double.TryParse(line[136], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    IVP = double.TryParse(line[137], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                };
                feedData.IngredientTypeString = feedData.IngredientType.GetDescription();

                result.Add(feedData);
            }

            // Need to add Urea.
            result.Add(new FeedIngredient()
            {
                // This value is constant.
                IngredientType = IngredientType.Urea,
                IngredientTypeString = IngredientType.Urea.GetDescription(),
                CrudeProtein = 291,
            });

            return result;
        }

        /// <summary>
        /// Reads beef feed ingredient data from a CSV resource file and parses each line into a <see cref="FeedIngredient"/> object.
        /// Converts string values to their appropriate types and populates all relevant properties for each ingredient.
        /// Adds a constant Urea ingredient at the end of the list.
        /// </summary>
        /// <returns>A collection of <see cref="FeedIngredient"/> objects representing beef feed ingredients.</returns>
        private IEnumerable<FeedIngredient> ReadBeefFile()
        {
            var cultureInfo = InfrastructureConstants.EnglishCultureInfo;
            var fileLines = CsvResourceReader.GetFileLines(CsvResourceNames.FeedNames)!;
            var feedNameStringConverter = new FeedIngredientStringConverter();
            double parseResult = 0;

            var result = new List<FeedIngredient>();

            foreach (var line in fileLines)
            {
                var feedData = new FeedIngredient
                {
                    IngredientType = feedNameStringConverter.Convert(line[1]),
                    IFN = line[2],
                    Cost = double.TryParse(line[3], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Forage = double.TryParse(line[4], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    DryMatter = double.TryParse(line[5], NumberStyles.Any, cultureInfo, out parseResult)
                        ? parseResult
                        : 0,
                    CrudeProtein = double.TryParse(line[6], NumberStyles.Any, cultureInfo, out parseResult)
                        ? parseResult
                        : 0,
                    SP = double.TryParse(line[7], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ADICP = double.TryParse(line[8], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Sugars = double.TryParse(line[9], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    OA = double.TryParse(line[10], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Fat = double.TryParse(line[11], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Ash = double.TryParse(line[12], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Starch =
                        double.TryParse(line[13], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    NDF = double.TryParse(line[14], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Lignin =
                        double.TryParse(line[15], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    TotalDigestibleNutrient = double.TryParse(line[16], NumberStyles.Any, cultureInfo, out parseResult)
                        ? parseResult
                        : 0,
                    ME = double.TryParse(line[17], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    NEma = double.TryParse(line[18], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    NEga = double.TryParse(line[19], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    RUP = double.TryParse(line[20], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    kdPB = double.TryParse(line[21], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    KdCB1 = double.TryParse(line[22], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    KdCB2 = double.TryParse(line[23], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    KdCB3 = double.TryParse(line[24], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    PBID = double.TryParse(line[25], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    CB1ID = double.TryParse(line[26], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    CB2ID = double.TryParse(line[27], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Pef = double.TryParse(line[28], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ARG = double.TryParse(line[29], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    HIS = double.TryParse(line[30], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ILE = double.TryParse(line[31], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    LEU = double.TryParse(line[32], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    LYS = double.TryParse(line[33], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    MET = double.TryParse(line[34], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    CYS = double.TryParse(line[35], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    PHE = double.TryParse(line[36], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    TYR = double.TryParse(line[37], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    THR = double.TryParse(line[38], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    TRP = double.TryParse(line[39], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    VAL = double.TryParse(line[40], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Ca = double.TryParse(line[41], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    P = double.TryParse(line[42], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Mg = double.TryParse(line[43], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Cl = double.TryParse(line[44], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    K = double.TryParse(line[45], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Na = double.TryParse(line[46], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    S = double.TryParse(line[47], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Co = double.TryParse(line[48], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Cu = double.TryParse(line[49], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    I = double.TryParse(line[50], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Fe = double.TryParse(line[51], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Mn = double.TryParse(line[52], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Se = double.TryParse(line[53], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Zn = double.TryParse(line[54], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    VitA = double.TryParse(line[55], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    VitD = double.TryParse(line[56], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    VitE = double.TryParse(line[57], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0
                };

                feedData.IngredientTypeString = feedData.IngredientType.GetDescription();

                result.Add(feedData);
            }

            // Need to add Urea.
            result.Add(new FeedIngredient()
            {
                // This value is constant.
                IngredientType = IngredientType.Urea,
                IngredientTypeString = IngredientType.Urea.GetDescription(),
                CrudeProtein = 291,
            });

            return result;
        }

        /// <summary>
        /// Reads dairy feed ingredient data from a CSV resource file and parses each line into a <see cref="FeedIngredient"/> object.
        /// Converts string values to their appropriate types and populates all relevant properties for each ingredient.
        /// Skips lines without a line number and uses converters for ingredient and dairy feed class types.
        /// </summary>
        /// <returns>A collection of <see cref="FeedIngredient"/> objects representing dairy feed ingredients.</returns>
        private IEnumerable<FeedIngredient> ReadDairyFile()
        {
            var cultureInfo = InfrastructureConstants.EnglishCultureInfo;
            var fileLines = CsvResourceReader.GetFileLines(CsvResourceNames.DairyFeedComposition)!.ToList();
            var feedNameStringConverter = new FeedIngredientStringConverter();
            var dairyClassTypeConverter = new DairyFeedClassTypeConverter();
            double parseResult = 0;

            var result = new List<FeedIngredient>();

            foreach (var line in fileLines.Skip(2))
            {
                //line[0] is the line number starting from 1, if it has no number, go to the next line
                if (String.IsNullOrEmpty(line[0]))
                {
                    continue;
                }

                var feedData = new FeedIngredient()
                {
                    IngredientType = feedNameStringConverter.Convert(line[1]),
                    //FeedNumber = line[2],
                    DairyFeedClass = dairyClassTypeConverter.Convert(line[2]),
                    TotalDigestibleNutrient = double.TryParse(line[3], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    DryMatter = double.TryParse(line[4], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    CrudeProtein = double.TryParse(line[5], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    NDICP = double.TryParse(line[6], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ADICP = double.TryParse(line[7], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    EE = double.TryParse(line[8], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    NDF = double.TryParse(line[9], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ADF = double.TryParse(line[10], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Lignin = double.TryParse(line[11], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    Ash = double.TryParse(line[12], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    DE = double.TryParse(line[13], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    ME = double.TryParse(line[14], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    NEL_ThreeX = double.TryParse(line[15], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    NEL_FourX = double.TryParse(line[16], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    NEM = double.TryParse(line[17], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                    NEG = double.TryParse(line[18], NumberStyles.Any, cultureInfo, out parseResult) ? parseResult : 0,
                };

                feedData.IngredientTypeString = feedData.IngredientType.GetDescription();

                result.Add(feedData);
            }

            return result;
        }

        #endregion
    }
}