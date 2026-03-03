using H.Core.Enumerations;
using H.Core.Properties;
using H.Core.Providers.Feed;
using H.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace H.Core.Services.DietService
{
    /// <summary>
    /// Provides a factory implementation for creating diet DTO objects based on animal and diet type combinations.
    /// This factory supports caching of created diet DTOs and validates diet-animal type compatibility before creation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="DietFactory"/> class implements the factory pattern to create <see cref="IDietDto"/> instances
    /// for specific combinations of <see cref="AnimalType"/> and <see cref="DietType"/>. The factory maintains
    /// a predefined list of valid diet-animal combinations and only creates diet DTOs for supported pairings.
    /// </para>
    /// <para>
    /// Key features include:
    /// <list type="bullet">
    /// <item><description>Validation of diet-animal type combinations before creation</description></item>
    /// <item><description>Caching support to improve performance for repeated requests</description></item>
    /// <item><description>Comprehensive logging of creation and validation operations</description></item>
    /// <item><description>Fallback diet DTO creation for invalid combinations</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// The factory uses dependency injection for logging (<see cref="ILogger"/>) and caching (<see cref="ICacheService"/>)
    /// services. If these dependencies are not provided, the factory will still function but without logging or caching capabilities.
    /// </para>
    /// </remarks>
    /// <example>
    /// <para>Basic usage without dependencies:</para>
    /// <code>
    /// var factory = new DietFactory();
    /// var dietDto = factory.Create(DietType.LowEnergyAndProtein, AnimalType.BeefCow);
    /// </code>
    /// <para>Usage with dependency injection:</para>
    /// <code>
    /// var factory = new DietFactory(logger, cacheService, feedIngredientProvider);
    /// var dietDto = factory.Create(DietType.MediumEnergyAndProtein, AnimalType.BeefCow);
    /// 
    /// // Check if a combination is valid before creating
    /// if (factory.IsValidDietType(AnimalType.BeefCow, DietType.LowEnergyAndProtein))
    /// {
    ///     var validDietDto = factory.Create(DietType.LowEnergyAndProtein, AnimalType.BeefCow);
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="IDietFactory"/>
    /// <seealso cref="IDietDto"/>
    /// <seealso cref="AnimalType"/>
    /// <seealso cref="DietType"/>
    public class DietFactory : IDietFactory
    {
        #region Fields

        private readonly ILogger? _logger;
        private readonly ICacheService? _cacheService;
        private readonly IReadOnlyList<Tuple<AnimalType, DietType>>? _validDietKeys;
        private readonly IFeedIngredientProvider? _feedIngredientProvider;
        private readonly IReadOnlyCollection<IDietDto>? _dietCollection;

        #endregion

        #region Constructors

        public DietFactory()
        {
        }

        public DietFactory(ILogger logger, ICacheService cacheService, IFeedIngredientProvider feedIngredientProvider) : this()
        {
            if (feedIngredientProvider != null)
            {
                _feedIngredientProvider = feedIngredientProvider;
            }
            else
            {
                throw new ArgumentNullException(nameof(feedIngredientProvider));
            }

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

            _feedIngredientProvider = feedIngredientProvider;

            _dietCollection = this.BuildDiets();
            _validDietKeys = this.CreateDietKeys();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a default <see cref="IDietDto"/> instance without specifying diet type or animal type.
        /// </summary>
        /// <returns>
        /// An <see cref="IDietDto"/> instance representing a generic or placeholder diet. The default implementation
        /// throws a <see cref="NotImplementedException"/> unless overridden in a derived class.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This parameterless method is intended for scenarios where a specific diet type and animal type are not known
        /// at creation time. By default, it is not implemented and will throw an exception. Implementations may choose
        /// to return a generic diet, a placeholder, or use application-specific logic.
        /// </para>
        /// <para>
        /// For most use cases, prefer using <see cref="Create(DietType, AnimalType)"/> to ensure a properly configured diet.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var factory = new DietFactory();
        /// var dietDto = factory.Create(); // May throw NotImplementedException
        /// </code>
        /// </example>
        /// <seealso cref="Create(DietType, AnimalType)"/>
        public IDietDto Create()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a diet instance based on the specified diet type and animal type combination.
        /// </summary>
        /// <param name="dietType">The type of diet to create (e.g., low energy, medium energy).</param>
        /// <param name="animalType">The animal type for which the diet is intended (e.g., beef cow, dairy cow).</param>
        /// <returns>
        /// An <see cref="IDietDto"/> instance configured for the specified combination. If the combination is valid,
        /// returns a properly configured diet; otherwise, returns a fallback diet with the name "Unknown diet".
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method implements a caching strategy to improve performance for repeated requests. When a valid
        /// diet-animal combination is requested, the method first checks the cache for an existing instance before
        /// creating a new one. Newly created diets are automatically cached for future requests.
        /// </para>
        /// <para>
        /// The method validates the input combination using <see cref="IsValidDietType(AnimalType, DietType)"/>
        /// before attempting to create the diet. Invalid combinations will result in a fallback diet being returned
        /// rather than throwing an exception.
        /// </para>
        /// <para>
        /// All operations are logged through the configured <see cref="ILogger"/> instance, including:
        /// <list type="bullet">
        /// <item><description>Cache hits when returning cached diets</description></item>
        /// <item><description>New diet creation events</description></item>
        /// <item><description>Error events for invalid combinations</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>Creating a diet for a valid combination:</para>
        /// <code>
        /// var factory = new DietFactory(logger, cacheService);
        /// var dietDto = factory.Create(DietType.LowEnergyAndProtein, AnimalType.BeefCow);
        /// // Returns a properly configured diet instance
        /// </code>
        /// <para>Attempting to create a diet for an invalid combination:</para>
        /// <code>
        /// var dietDto = factory.Create(DietType.HighEnergyAndProtein, AnimalType.Sheep);
        /// // Returns a fallback diet with Name = "Unknown diet"
        /// // Logs an error message about the invalid combination
        /// </code>
        /// </example>
        /// <seealso cref="IsValidDietType(AnimalType, DietType)"/>
        /// <seealso cref="GetValidDietKeys()"/>
        /// <seealso cref="IDietDto"/>
        /// <seealso cref="DietType"/>
        /// <seealso cref="AnimalType"/>
        public IDietDto Create(DietType dietType, AnimalType animalType)
        {
            if (this.IsValidDietType(animalType, dietType))
            {
                var key = $"{nameof(DietFactory)}_{nameof(DietFactory.Create)}_{dietType}_{animalType}";
                var cachedDiet = _cacheService?.Get<IDietDto>(key);
                if (cachedDiet != null)
                {
                    _logger?.LogInformation($"Returning cached diet for {dietType} and {animalType}");
                    return cachedDiet;
                }

                _logger?.LogInformation($"Creating diet for {dietType} and {animalType}");

                var dietDto = BuildDietDto(animalType, dietType);
                _cacheService?.Set(key, dietDto);

                return dietDto;
            }
            else
            {
                _logger?.LogError($"Cannot create {dietType} for {animalType}");

                return new DietDto()
                {
                    Name = "Unknown diet",
                    IsDefaultDiet = false,
                    DietType = dietType,
                    AnimalType = animalType,
                    Ingredients = Array.Empty<IFeedIngredient>(),
                };
            }
        }

        /// <summary>
        /// Retrieves the list of valid animal type and diet type combinations supported by this factory.
        /// </summary>
        /// <returns>
        /// A read-only list of <see cref="Tuple{AnimalType, DietType}"/> representing all valid combinations
        /// for which diets can be created.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The returned list acts as the authoritative source for supported diet-animal pairings. Only combinations
        /// present in this list will be considered valid by <see cref="IsValidDietType(AnimalType, DietType)"/> and
        /// will result in proper diet creation through <see cref="Create(DietType, AnimalType)"/>.
        /// </para>
        /// <para>
        /// This method is useful for clients that need to enumerate or validate available diet options before attempting
        /// to create a diet. The list is initialized during factory construction and remains constant for the lifetime
        /// of the factory instance.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// var factory = new DietFactory();
        /// var validKeys = factory.GetValidDietKeys();
        /// foreach (var (animalType, dietType) in validKeys)
        /// {
        ///     Console.WriteLine($"Supported: {animalType} with {dietType}");
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="IsValidDietType(AnimalType, DietType)"/>
        /// <seealso cref="Create(DietType, AnimalType)"/>
        /// <seealso cref="AnimalType"/>
        /// <seealso cref="DietType"/>
        public IReadOnlyList<Tuple<AnimalType, DietType>> GetValidDietKeys()
        {
            return _validDietKeys!;
        }

        /// <summary>
        /// Determines whether the specified combination of animal type and diet type is valid and supported by this factory.
        /// </summary>
        /// <param name="animalType">The animal type to validate against the available diet combinations.</param>
        /// <param name="dietType">The diet type to validate against the available animal combinations.</param>
        /// <returns>
        /// <see langword="true"/> if the factory can create a diet for the specified combination; 
        /// otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method performs a lookup against the predefined list of valid diet-animal combinations
        /// maintained by the factory. The validation is based on a whitelist approach where only
        /// explicitly supported combinations will return <see langword="true"/>.
        /// </para>
        /// <para>
        /// The method uses <see cref="GetValidDietKeys()"/> to retrieve the current list of supported
        /// combinations and performs an exact match lookup. Both the animal type and diet type must
        /// match exactly for the combination to be considered valid.
        /// </para>
        /// <para>
        /// It is recommended to call this method before attempting to create a diet using 
        /// <see cref="Create(DietType, AnimalType)"/> to avoid receiving fallback "Unknown diet" instances.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>Validating a supported combination:</para>
        /// <code>
        /// var factory = new DietFactory();
        /// bool isValid = factory.IsValidDietType(AnimalType.BeefCow, DietType.LowEnergyAndProtein);
        /// // Returns true if this combination is supported
        /// </code>
        /// <para>Using validation before diet creation:</para>
        /// <code>
        /// if (factory.IsValidDietType(animalType, dietType))
        /// {
        ///     var dietDto = factory.Create(dietType, animalType);
        ///     // Guaranteed to receive a properly configured diet
        /// }
        /// else
        /// {
        ///     // Handle unsupported combination
        ///     Console.WriteLine($"Diet combination {dietType} for {animalType} is not supported");
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="GetValidDietKeys()"/>
        /// <seealso cref="Create(DietType, AnimalType)"/>
        /// <seealso cref="AnimalType"/>
        /// <seealso cref="DietType"/>
        public bool IsValidDietType(AnimalType animalType, DietType dietType)
        {
            return this.GetValidDietKeys().Contains(new Tuple<AnimalType, DietType>(animalType, dietType));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Creates and returns the predefined list of valid animal type and diet type combinations 
        /// supported by this factory.
        /// </summary>
        /// <returns>
        /// A read-only list of <see cref="Tuple{T1, T2}"/> where T1 is <see cref="AnimalType"/> 
        /// and T2 is <see cref="DietType"/>, representing all valid combinations for diet creation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method defines the authoritative whitelist of supported diet-animal combinations.
        /// Only combinations returned by this method will be considered valid by 
        /// <see cref="IsValidDietType(AnimalType, DietType)"/> and will result in proper diet
        /// creation through <see cref="Create(DietType, AnimalType)"/>.
        /// </para>
        /// <para>
        /// Currently supported combinations include:
        /// <list type="bullet">
        /// <item><description><see cref="AnimalType.BeefCow"/> with <see cref="DietType.LowEnergyAndProtein"/></description></item>
        /// <item><description><see cref="AnimalType.BeefCow"/> with <see cref="DietType.MediumEnergyAndProtein"/></description></item>
        /// </list>
        /// </para>
        /// <para>
        /// This method is called during factory initialization and the result is cached for the
        /// lifetime of the factory instance. To add support for new combinations, modify the
        /// list returned by this method.
        /// </para>
        /// </remarks>
        /// <example>
        /// <para>The method returns combinations that can be used like this:</para>
        /// <code>
        /// var factory = new DietFactory();
        /// var validKeys = factory.GetValidDietKeys(); // Calls this method internally
        /// 
        /// foreach (var (animalType, dietType) in validKeys)
        /// {
        ///     var dietDto = factory.Create(dietType, animalType);
        ///     Console.WriteLine($"Created diet for {animalType} with {dietType}");
        /// }
        /// </code>
        /// </example>
        /// <seealso cref="GetValidDietKeys()"/>
        /// <seealso cref="IsValidDietType(AnimalType, DietType)"/>
        /// <seealso cref="Create(DietType, AnimalType)"/>
        /// <seealso cref="AnimalType"/>
        /// <seealso cref="DietType"/>
        private IReadOnlyList<Tuple<AnimalType, DietType>> CreateDietKeys()
        {
            var result = new List<Tuple<AnimalType, DietType>>();

            var keys = _dietCollection!.Select(d => new Tuple<AnimalType, DietType>(d.AnimalType, d.DietType));
            result.AddRange(keys);

            return result;
        }

        private IReadOnlyList<IDietDto> BuildDiets()
        {
            /*
             * Set the MCF after the ingredient collection has been initialized so that it is not overwritten when the collection changed event
             * is triggered and updates the MCF based on the new ingredient list.
             *
             * Diet composition is provided in comments if ingredients cannot be used.
             *
             * For dairy diets, the ingredient provider does not provide Forage content values, and so they must be set manually.
             *
             * Any other property overrides to the composition provided by the ingredient list needs to be done after the collection of ingredients is
             * initialized (i.e. this includes setting properties for a custom forage, or crude protein value)
             *
             * Animals from the cow-calf component (bulls, cows, calves) have the same set (and composition) of diets as the stocker diets from the algorithm document, and so we do not
             * create a separate collection of diets just for stockers.
             *
             * Some diets should not be displayed in the diet formulator since not all animal types allow for custom diets. Any diet that
             * should be hidden from user in diet formulator view should have the HideFromUserInDietFormulator property set to true.
             */

            List<IDietDto> result =
            [
                #region Beef cow

                /*
                 * Implements: Table 20. Examples of NEmf content of typical diets fed to cattle for estimation of dry matter intake (IPCC, 2019, Table 10.8a).
                 *
                 * Footnote 1: The low, moderate and high quality forage diets are equivalent to the low, medium and high energy/protein diets, respectively, for beef cows in Table 21.
                 *
                 * Footnote 2: If the model user wants to formulate their own diet, the feed energy content [NEmf (MJkg-1 DM)]
                 * can be calculated from the default feed tables built into Holos. The values are included in the default
                 * table as NEma and NEga (Mcal kg-1); therefore, NEmf can be calculated as: NEmf (MJ kg-1 DM)=[NEma+NEga]*4.184 (conversion factor for Mcal to MJ).
                 */

		        new DietDto()
                {
                    /*
                     * TotalDigestibleNutrient = 47
                     * CrudeProtein = 6
                     * Forage = 100
                     * Starch = 5.5
                     * Fat = 1.4
                     * MetabolizableEnergy = 1.73
                     * Ndf = 71.4
                     */

                    Name = Resources.LowEnergyProtein,
                    DietType = DietType.LowEnergyAndProtein,
                    AnimalType = AnimalType.BeefCow,
                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider!.GetIngredientsForDiet(AnimalType.BeefCow, DietType.LowEnergyAndProtein)
                    ],

                    MethaneConversionFactor = 0.07,
                    DietaryNetEnergyConcentration = 4.5,
                },

                new DietDto()
                {
                    /*
                     * TotalDigestibleNutrient = 54.6
                     * CrudeProtein = 12.4
                     * Forage = 97
                     * Starch = 7.1
                     * Fat = 1.8
                     * MetabolizableEnergy = 1.98
                     * Ndf = 53.5
                     */

                    Name = Resources.LabelMediumEnergyProteinDiet,
                    DietType = DietType.MediumEnergyAndProtein,
                    AnimalType = AnimalType.BeefCow,
                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefCow, DietType.MediumEnergyAndProtein)
                    ],

                    MethaneConversionFactor = 0.070,
                    DietaryNetEnergyConcentration = 6,
                },

                new DietDto()
                {
                    /*
                     * TotalDigestibleNutrient = 62.8
                     * CrudeProtein = 17.7
                     * Forage = 85
                     * Starch = 9.9
                     * Fat = 2.2
                     * MetabolizableEnergy = 2.14
                     * Ndf = 45.1
                     */

                    Name = Resources.LabelHighEnergyProteinDiet,
                    DietType = DietType.HighEnergyAndProtein,
                    AnimalType = AnimalType.BeefCow,
                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefCow, DietType.HighEnergyAndProtein)
                    ],

                    MethaneConversionFactor = 0.070,
                    DietaryNetEnergyConcentration = 7.0,
                },

                #endregion

                #region Beef backgrounding

                new DietDto()
                {
                    /*
                     * TotalDigestibleNutrient = 69.4
                     * CrudeProtein = 11.6
                     * Forage = 65
                     * Starch = 28.5
                     * Fat = 3.3
                     * MetabolizableEnergy = 2.51
                     * Ndf = 40.5
                     */

                    Name = Resources.LabelSlowGrowthDiet,
                    DietType = DietType.SlowGrowth,
                    AnimalType = AnimalType.BeefBackgrounder,
                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefBackgrounder, DietType.SlowGrowth)
                    ],

                    MethaneConversionFactor = 0.063,
                },

                new DietDto()
                {
                    /*
                     * TotalDigestibleNutrient = 70.7
                     * CrudeProtein = 12.6
                     * Forage = 60
                     * Starch = 31.3
                     * Fat = 3.3
                     * MetabolizableEnergy = 2.56
                     * Ndf = 38.5
                     */

                    Name = Resources.LabelMediumGrowthDiet,
                    DietType = DietType.MediumGrowth,
                    AnimalType = AnimalType.BeefBackgrounder,
                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefBackgrounder, DietType.MediumGrowth)
                    ],

                    MethaneConversionFactor = 0.063,
                },

                #endregion

                #region Beef finishing

                new DietDto()
                {
                    /*
                     * TotalDigestibleNutrient = 80.4
                     * CrudeProtein = 12.6
                     * Forage = 7.5
                     * Starch = 52.1
                     * Fat = 2.3
                     * MetabolizableEnergy = 2.91
                     * Ndf = 21
                     */

                    Name = Resources.LabelBarleyGrainBasedDiet,
                    DietType = DietType.BarleyGrainBased,
                    AnimalType = AnimalType.BeefFinisher,
                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefFinisher, DietType.BarleyGrainBased)
                    ],

                    MethaneConversionFactor = 0.035,
                },

                new DietDto()
                {
                    /*
                     * TotalDigestibleNutrient = 83.9
                     * CrudeProtein = 12.6
                     * Forage = 7.5
                     * Starch = 65.9
                     * Fat = 3.7
                     * MetabolizableEnergy = 3.03
                     * Ndf = 13.5
                     */

                    Name = Resources.LabelCornGrainBasedDiet,
                    DietType = DietType.CornGrainBased,
                    AnimalType = AnimalType.BeefFinisher,
                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefFinisher, DietType.BarleyGrainBased)
                    ],

                    MethaneConversionFactor = 0.03,
                },

                #endregion

                #region Dairy lactating cow

                new DietDto()
                {
                    Name = Properties.Resources.LabelLegumeForageBasedDiet,
                    DietType = DietType.LegumeForageBased,
                    AnimalType = AnimalType.DairyLactatingCow,
                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.DairyLactatingCow, DietType.LegumeForageBased)
                    ],

                    Forage = 77.8,
                    MethaneConversionFactor = 0.056,
                },

                new DietDto()
                {
                    Name = Properties.Resources.LabelBarleySilageBasedDiet,
                    DietType = DietType.BarleySilageBased,
                    AnimalType = AnimalType.DairyLactatingCow,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.DairyLactatingCow, DietType.BarleySilageBased)
                    ],

                    Forage = 60.5,
                    MethaneConversionFactor = 0.056,
                },

                #endregion

                #region Dairy heifer

                new DietDto()
                {
                    Name = Properties.Resources.LabelHighFiberDiet,
                    DietType = DietType.HighFiber,
                    AnimalType = AnimalType.DairyHeifers,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.DairyHeifers, DietType.HighFiber)
                    ],

                    Forage = 87.6,
                    MethaneConversionFactor = 0.07,
                },

                new DietDto()
                {
                    Name = Properties.Resources.LabelLowFiberDiet,
                    DietType = DietType.LowFiber,
                    AnimalType = AnimalType.DairyHeifers,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.DairyHeifers, DietType.LowFiber)
                    ],

                    Forage = 63,
                    MethaneConversionFactor = 0.063,
                },

                #endregion

                #region Swine

                // Diet A - Gestation (114 days)
                new DietDto()
                {
                    Name = Resources.LabelGestationDiet,
                    DietType = DietType.Gestation,
                    AnimalType = AnimalType.Swine,
                    Comments = Resources.LabelDietA,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.Gestation)
                    ],

                    DailyDryMatterFeedIntakeOfFeed = 2.49,
                    CrudeProtein = 14.28,
                },

                // Diet B - Lactation (21 days)
                new DietDto()
                {
                    Name = Resources.LabelLactationDiet,
                    DietType = DietType.Lactation,
                    AnimalType = AnimalType.Swine,
                    Comments = Resources.LabelDietB,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.Lactation)
                    ],

                    DailyDryMatterFeedIntakeOfFeed = 6.59,
                    CrudeProtein = 19.07,
                },

                // Diet C1 - Nursery weaners (starter diet 1)
                new DietDto()
                {
                    Name = Resources.LabelNurseryWeanersStarterDiet1,
                    DietType = DietType.NurseryWeanersStarter1,
                    AnimalType = AnimalType.Swine,
                    Comments = Resources.LabelDietC1,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.NurseryWeanersStarter1)
                    ],

                    DailyDryMatterFeedIntakeOfFeed = 0.80,
                    CrudeProtein = 23.88,
                },

                // Diet C2 - Nursery weaners (starter diet 2)
                new DietDto()
                {
                    Name = Resources.LabelNurseryWeanersStarterDiet2,
                    DietType = DietType.NurseryWeanersStarter2,
                    AnimalType = AnimalType.Swine,
                    Comments = Resources.LabelDietC2,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.NurseryWeanersStarter2)
                    ],

                    DailyDryMatterFeedIntakeOfFeed = 1.17,
                    CrudeProtein = 21.45,
                },

                // Diet D1 - Grower/Finisher diet 1
                new DietDto()
                {
                    Name = Resources.LabelGrowerFinisherDiet1,
                    DietType = DietType.GrowerFinisherDiet1,
                    AnimalType = AnimalType.Swine,
                    Comments = Resources.LabelDietD1,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.GrowerFinisherDiet1)
                    ],

                    DailyDryMatterFeedIntakeOfFeed = 1.68,
                    CrudeProtein = 20.27,
                },

                // Diet D2 - Grower/Finisher diet 2
                new DietDto()
                {
                    Name = Resources.LabelGrowerFinisherDiet2,
                    DietType = DietType.GrowerFinisherDiet2,
                    AnimalType = AnimalType.Swine,
                    Comments = Resources.LabelDietD2,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.GrowerFinisherDiet2)
                    ],

                    DailyDryMatterFeedIntakeOfFeed = 2.13,
                    CrudeProtein = 19.89,
                },

                // Diet D3 - Grower/Finisher diet 3
                new DietDto()
                {
                    Name = Resources.LabelGrowerFinisherDiet3,
                    DietType = DietType.GrowerFinisherDiet3,
                    AnimalType = AnimalType.Swine,
                    Comments = Resources.LabelDietD3,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.GrowerFinisherDiet3)
                    ],

                    DailyDryMatterFeedIntakeOfFeed = 2.55,
                    CrudeProtein = 19.92,
                },

                // Diet D4 - Grower/Finisher diet 4
                new DietDto()
                {
                    Name = Resources.LabelGrowerFinisherDiet4,
                    DietType = DietType.GrowerFinisherDiet4,
                    AnimalType = AnimalType.Swine,
                    Comments = Resources.LabelDietD4,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.GrowerFinisherDiet4)
                    ],

                    DailyDryMatterFeedIntakeOfFeed = 2.93,
                    CrudeProtein = 19.66,
                },

                // Diet E - Gilt developer diet
                new DietDto()
                {
                    Name = Resources.LabelGiltDeveloperDiet,
                    DietType = DietType.GiltDeveloperDiet,
                    AnimalType = AnimalType.Swine,
                    Comments = Resources.LabelGiltDeveloperDiet,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.GiltDeveloperDiet)
                    ],

                    DailyDryMatterFeedIntakeOfFeed = 2.80,
                    CrudeProtein = 17.95,
                },

                // Diet F - Boar diet
                new DietDto()
                {
                    Name = Resources.LabelBoarDiet,
                    DietType = DietType.Boars,
                    AnimalType = AnimalType.Swine,
                    Comments = Resources.LabelBoarDiet,

                    Ingredients = (List<IFeedIngredient>)
                    [
                        .._feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.Boars)
                    ],

                    DailyDryMatterFeedIntakeOfFeed = 3,
                    CrudeProtein = 20.1,
                },

                #endregion

                #region Sheep

                new DietDto()
                {
                    Name = Core.Properties.Resources.GoodQualityForage,
                    DietType = DietType.GoodQualityForage,
                    AnimalType = AnimalType.Sheep,
                    TotalDigestibleNutrient = 60.0,
                    CrudeProtein = 17.7, 
                    Ash = 8.0,
                    MethaneConversionFactor = 0.067,
                },

                new DietDto()
                {
                    Name = Core.Properties.Resources.AverageQualityForage,
                    AnimalType = AnimalType.Sheep,
                    DietType = DietType.AverageQualityForage,
                    TotalDigestibleNutrient = 55,
                    CrudeProtein = 12.4,
                    Ash = 8,
                    MethaneConversionFactor = 0.067,
                },

                new DietDto()
                {
                    IsDefaultDiet = true,
                    Name = Core.Properties.Resources.PoorQualityForage,
                    DietType = DietType.PoorQualityForage,
                    AnimalType = AnimalType.Sheep,
                    TotalDigestibleNutrient = 48,
                    CrudeProtein = 5.7,
                    Ash = 8,
                    MethaneConversionFactor = 0.067,
                },

                #endregion

                /*
                 * Some animal groups will not have a diet (poultry, other livestock, suckling pigs, etc.). In these cases, a non-null diet
                 * must still be set.
                 */

                new DietDto()
                {
                    Name = Resources.None,
                    IsCustomPlaceholderDiet = true,
                    DietType = DietType.None,
                    AnimalType = AnimalType.NotSelected,
                }
            ];

            // These are all system/default diets and the ingredient composition is not meant to be modified
            foreach (var dietDto in result)
            {
                dietDto.IsDefaultDiet = true;

                foreach (var feedIngredient in dietDto.Ingredients)
                {
                    feedIngredient.IsReadonly = true;
                }
            }

            return result;
        }

        private IDietDto BuildDietDto(AnimalType animalType, DietType dietType)
        {
            if (this.IsValidDietType(animalType, dietType) == false)
            {
                _logger?.LogError($"Cannot build diet for {animalType} and {dietType}");

                return new DietDto();
            }

            return _dietCollection!.Single(x => x.AnimalType == animalType && x.DietType == dietType);
        }

        #endregion
    }
}