#region Imports

using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Properties;

#endregion

namespace H.Core.Providers.Feed
{
    /// <summary>
    /// </summary>
    public class DietProvider : IDietProvider
    {
        #region Fields

        private readonly IFeedIngredientProvider _feedIngredientProvider;

        #endregion

        #region Constructors

        public DietProvider()  
        {
            _feedIngredientProvider = new FeedIngredientProvider();
        }

        #endregion

        #region Public Methods

        public List<Diet> GetDiets()
        {
            var diets = new List<Diet>();

            /*
             * Beef cattle diets
             *
             * Animals from the cow-calf component (bulls, cows, calves) have the same set (and composition) of diets as the stocker diets from the algorithm document and so we do not
             * create a separate collection of diets just for stockers.
             */
            diets.AddRange(this.CreateBeefCowDiets());
            diets.AddRange(this.CreateBeefBackgroundingDiets());
            diets.AddRange(this.CreateBeefFinishingDiets());

            /*
             * Dairy cattle diets
             */

            diets.AddRange(this.CreateDairyLactatingCowDiets());
            diets.AddRange(this.CreateDairyDryCowDiets());
            diets.AddRange(this.CreateDairyHeiferDiets());

            /*
             * Swine diets
             */
            diets.AddRange(this.CreateSwineDiets());

            /*
             * Sheep diets
             */
            diets.AddRange(this.GetSheepDiets());

            // For animals that don't have default diets, we still need to set the diet to a placeholder (non-null) value.
            diets.Add(this.GetNoDiet());

            // This needs to be set so readonly style can be used in diet creator view
            foreach (var diet in diets)
            {
                foreach (var ingredient in diet.Ingredients)
                {
                    ingredient.IsReadonly = true;
                }
            }

            return diets;
        }

        #endregion

        #region Private Methods

        private IEnumerable<Diet> CreateDairyHeiferDiets()
        {
            var dairyIngredients = _feedIngredientProvider.GetDairyFeedIngredients()!.ToList();

            var diets = new List<Diet>();

            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Properties.Resources.LabelHighFiberDiet,
                DietType = DietType.HighFiber,
                AnimalType = AnimalType.DairyHeifers,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.DairyHeifers, DietType.HighFiber).Select(x => (FeedIngredient)x)),

                Forage = 87.6,
                MethaneConversionFactor = 0.07,
            });

            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Properties.Resources.LabelLowFiberDiet,
                DietType = DietType.LowFiber,
                AnimalType = AnimalType.DairyHeifers,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.DairyHeifers, DietType.LowFiber).Select(x => (FeedIngredient)x)),

                Forage = 63,
                MethaneConversionFactor = 0.063,
            });

            return diets;
        }

        private IEnumerable<Diet> CreateDairyDryCowDiets()
        {
            var dairyIngredients = _feedIngredientProvider.GetDairyFeedIngredients()!.ToList();

            var diets = new List<Diet>();

            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Properties.Resources.LabelCloseUpDiet,
                DietType = DietType.CloseUp,
                AnimalType = AnimalType.DairyDryCow,

                Ingredients = new ObservableCollection<FeedIngredient>()
                {
                    _feedIngredientProvider.CopyIngredient((FeedIngredient)dairyIngredients.Single(x => x.IngredientType == IngredientType.CornYellowSilageNormal), 48)!,
                    _feedIngredientProvider.CopyIngredient((FeedIngredient)dairyIngredients.Single(x => x.IngredientType == IngredientType.GrassLegumeMixturesPredomLegumesSilageMidMaturity), 23)!,
                    _feedIngredientProvider.CopyIngredient((FeedIngredient)dairyIngredients.Single(x => x.IngredientType == IngredientType.CornYellowGrainCrackedDry), 12)!,
                    _feedIngredientProvider.CopyIngredient((FeedIngredient)dairyIngredients.Single(x => x.IngredientType == IngredientType.CanolaMealMechExtracted), 9)!,
                    _feedIngredientProvider.CopyIngredient((FeedIngredient)dairyIngredients.Single(x => x.IngredientType == IngredientType.CornYellowGlutenMealDried), 8)!,
                },

                Forage = 52.4,
                MethaneConversionFactor = 0.063,
            });

            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Properties.Resources.LabelFarOffDiet,
                DietType = DietType.FarOff,
                AnimalType = AnimalType.DairyDryCow,

                Ingredients = new ObservableCollection<FeedIngredient>()
                {
                    _feedIngredientProvider.CopyIngredient((FeedIngredient)dairyIngredients.Single(x => x.IngredientType == IngredientType.GrassLegumeMixturesPredomLegumesHayMidMaturity), 57)!,
                    _feedIngredientProvider.CopyIngredient((FeedIngredient)dairyIngredients.Single(x => x.IngredientType == IngredientType.BarleyGrainRolled), 38)!,
                    _feedIngredientProvider.CopyIngredient((FeedIngredient)dairyIngredients.Single(x => x.IngredientType == IngredientType.SoybeanMealExpellers), 5)!,
                },

                Forage = 90.2,
                MethaneConversionFactor = 0.070,
            });

            return diets;
        }

        private IEnumerable<Diet> CreateDairyLactatingCowDiets()
        {
            var dairyIngredients = _feedIngredientProvider.GetDairyFeedIngredients()!.ToList();

            var diets = new List<Diet>();

            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Properties.Resources.LabelLegumeForageBasedDiet,
                DietType = DietType.LegumeForageBased,
                AnimalType = AnimalType.DairyLactatingCow,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.DairyLactatingCow, DietType.LegumeForageBased).Select(x => (FeedIngredient)x)),

                Forage = 77.8,
                MethaneConversionFactor = 0.056,
            });

            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Properties.Resources.LabelBarleySilageBasedDiet,
                DietType = DietType.BarleySilageBased,
                AnimalType = AnimalType.DairyLactatingCow,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.DairyLactatingCow, DietType.BarleySilageBased).Select(x => (FeedIngredient)x)),

                Forage = 60.5,
                MethaneConversionFactor = 0.056,
            });

            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Properties.Resources.LabelCornSilageBasedDiet,
                DietType = DietType.CornSilageBased,
                AnimalType = AnimalType.DairyLactatingCow,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.DairyLactatingCow, DietType.CornSilageBased).Select(x => (FeedIngredient)x)),

                Forage = 59.1,
                MethaneConversionFactor = 0.056,
            });

            return diets;
        }

        private IEnumerable<Diet> CreateBeefBackgroundingDiets()
        {
            var beefIngredients = _feedIngredientProvider.GetBeefFeedIngredients()!.ToList();

            var diets = new List<Diet>();

            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelSlowGrowthDiet,
                DietType = DietType.SlowGrowth,
                AnimalType = AnimalType.BeefBackgrounder,

                // This is the breakdown of the diet if ingredients are not added.
                //TotalDigestibleNutrient = 69.4,
                //CrudeProtein = 11.6,
                //Forage = 65,
                //Starch = 28.5,
                //Fat = 3.3,
                //MetabolizableEnergy = 2.51,
                //Ndf = 40.5,                

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefBackgrounder, DietType.SlowGrowth).Select(x => (FeedIngredient)x)),

                MethaneConversionFactor = 0.063,
            });

            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelMediumGrowthDiet,
                DietType = DietType.MediumGrowth,
                AnimalType = AnimalType.BeefBackgrounder,

                // This is the breakdown of the diet if ingredients are not added.
                //TotalDigestibleNutrient = 70.7,
                //CrudeProtein = 12.6,
                //Forage = 60,
                //Starch = 31.3,
                //Fat = 3.3,
                //MetabolizableEnergy = 2.56,
                //Ndf = 38.5,                

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefBackgrounder, DietType.MediumGrowth).Select(x => (FeedIngredient)x)),

                MethaneConversionFactor = 0.063,
            });

            return diets;
        }

        private IEnumerable<Diet> CreateBeefFinishingDiets()
        {
            var diets = new List<Diet>();

            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelBarleyGrainBasedDiet,
                DietType = DietType.BarleyGrainBased,
                AnimalType = AnimalType.BeefFinisher,

                // This is the breakdown of the diet if ingredients are not added.
                //TotalDigestibleNutrient = 80.4,
                //CrudeProtein = 12.6,
                //Forage = 7.5,
                //Starch = 52.1,
                //Fat = 2.3,
                //MetabolizableEnergy = 2.91,
                //Ndf = 21,                

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefFinisher, DietType.BarleyGrainBased).Select(x => (FeedIngredient)x)),

                MethaneConversionFactor = 0.035,
            });

            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelCornGrainBasedDiet,
                DietType = DietType.CornGrainBased,
                AnimalType = AnimalType.BeefFinisher,

                // This is the breakdown of the diet if ingredients are not added.
                //TotalDigestibleNutrient = 83.9,
                //CrudeProtein = 12.6,
                //Forage = 7.5,
                //Starch = 65.9,
                //Fat = 3.7,
                //MetabolizableEnergy = 3.03,
                //Ndf = 13.5,                

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefFinisher, DietType.CornGrainBased).Select(x => (FeedIngredient)x)),

                MethaneConversionFactor = 0.03,
            });

            return diets;
        }

        /// <summary>
        /// Implements: Table 20. Examples of NEmf content of typical diets fed to cattle for estimation of dry matter intake (IPCC, 2019, Table 10.8a).
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Diet> CreateBeefCowDiets()
        {
            /*
             * Footnote 1: The low, moderate and high quality forage diets are equivalent to the low, medium and high energy/protein diets, respectively, for beef cows in Table 21.
               
               Footnote 2: If the model user wants to formulate their own diet, the feed energy content {NEmf (MJkg-1 DM)] 
               can be calculated from the default feed tables built into Holos. The values are included in the default 
               table as NEma and NEga (Mcal kg-1); therefore, NEmf can be calculated as: NEmf (MJ kg-1 DM)=[NEma+NEga]*4.184 (conversion factor for Mcal to MJ).
             */

            var diets = new List<Diet>();

            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LowEnergyProtein,
                DietType = DietType.LowEnergyAndProtein,
                AnimalType = AnimalType.BeefCow,

                // This is the breakdown of the diet if ingredients are not added.
                //TotalDigestibleNutrient = 47,
                //CrudeProtein = 6,
                //Forage = 100,
                //Starch = 5.5,
                //Fat = 1.4,
                //MetabolizableEnergy = 1.73,
                //Ndf = 71.4,                

                
                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefCow, DietType.LowEnergyAndProtein).Select(x => (FeedIngredient)x)),

                MethaneConversionFactor = 0.07,
                DietaryNetEnergyConcentration = 4.5,
            });

            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelMediumEnergyProteinDiet,
                DietType = DietType.MediumEnergyAndProtein,
                AnimalType = AnimalType.BeefCow,

                // This is the breakdown of the diet if ingredients are not added.
                //TotalDigestibleNutrient = 54.6,
                //CrudeProtein = 12.4,
                //Forage = 97,
                //Starch = 7.1,
                //Fat = 1.8,
                //MetabolizableEnergy = 1.98,
                //Ndf = 53.5,                

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefCow, DietType.MediumEnergyAndProtein).Select(x => (FeedIngredient)x)),

                MethaneConversionFactor = 0.070,
                DietaryNetEnergyConcentration = 6,
            });

            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelHighEnergyProteinDiet,
                DietType = DietType.HighEnergyAndProtein,
                AnimalType = AnimalType.BeefCow,

                // This is the breakdown of the diet if ingredients are not added.
                //TotalDigestibleNutrient = 62.8,
                //CrudeProtein = 17.7,
                //Forage = 85,
                //Starch = 9.9,
                //Fat = 2.2,
                //MetabolizableEnergy = 2.14,
                //Ndf = 45.1,                

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.BeefCow, DietType.HighEnergyAndProtein).Select(x => (FeedIngredient)x)),

                MethaneConversionFactor = 0.070,
                DietaryNetEnergyConcentration = 7.0,
            });

            return diets;
        }

        /// <summary>
        /// Swine diets (all diets) are defined in the diet provider. Some diets should not be displayed in the diet formulator since not all animal types allow for
        /// custom diets. Any diet that should be hidden from user in diet formulator view should have the HideFromUserInDietFormulator property set to true.
        /// </summary>
        private IEnumerable<Diet> CreateSwineDiets()
        {
            var swineIngredients = _feedIngredientProvider.GetSwineFeedIngredients();
            
            var diets = new List<Diet>();

            // Diet A - Gestation (114 days)
            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelGestationDiet,
                DietType = DietType.Gestation,
                AnimalType = AnimalType.Swine,
                Comments = Resources.LabelDietA,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.Gestation).Select(x => (FeedIngredient)x)),

                DailyDryMatterFeedIntakeOfFeed = 2.49,
                CrudeProtein = 14.28,
            });

            // Diet B - Lactation (21 days)
            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelLactationDiet,
                DietType = DietType.Lactation,
                AnimalType = AnimalType.Swine,
                Comments = Resources.LabelDietB,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.Lactation).Select(x => (FeedIngredient)x)),

                DailyDryMatterFeedIntakeOfFeed = 6.59,
                CrudeProtein = 19.07,
            });

            // Diet C1 - Nursery weaners (starter diet 1)
            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelNurseryWeanersStarterDiet1,
                DietType = DietType.NurseryWeanersStarter1,
                AnimalType = AnimalType.Swine,
                Comments = Resources.LabelDietC1,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.NurseryWeanersStarter1).Select(x => (FeedIngredient)x)),

                DailyDryMatterFeedIntakeOfFeed = 0.80,
                CrudeProtein = 23.88,
            });

            // Diet C2 - Nursery weaners (starter diet 2)
            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelNurseryWeanersStarterDiet2,
                DietType = DietType.NurseryWeanersStarter2,
                AnimalType = AnimalType.Swine,
                Comments = Resources.LabelDietC2,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.NurseryWeanersStarter2).Select(x => (FeedIngredient)x)),

                DailyDryMatterFeedIntakeOfFeed = 1.17,
                CrudeProtein = 21.45,
            });

            // Diet D1 - Grower/Finisher diet 1
            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelGrowerFinisherDiet1,
                DietType = DietType.GrowerFinisherDiet1,
                AnimalType = AnimalType.Swine,
                Comments = Resources.LabelDietD1,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.GrowerFinisherDiet1).Select(x => (FeedIngredient)x)),

                DailyDryMatterFeedIntakeOfFeed = 1.68,
                CrudeProtein = 20.27,
            });

            // Diet D2 - Grower/Finisher diet 2
            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelGrowerFinisherDiet2,
                DietType = DietType.GrowerFinisherDiet2,
                AnimalType = AnimalType.Swine,
                Comments = Resources.LabelDietD2,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.GrowerFinisherDiet2).Select(x => (FeedIngredient)x)),

                DailyDryMatterFeedIntakeOfFeed = 2.13,
                CrudeProtein = 19.89,
            });

            // Diet D3 - Grower/Finisher diet 3
            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelGrowerFinisherDiet3,
                DietType = DietType.GrowerFinisherDiet3,
                AnimalType = AnimalType.Swine,
                Comments = Resources.LabelDietD3,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.GrowerFinisherDiet3).Select(x => (FeedIngredient)x)),

                DailyDryMatterFeedIntakeOfFeed = 2.55,
                CrudeProtein = 19.92,
            });

            // Diet D4 - Grower/Finisher diet 4
            diets.Add(new Diet
            {
                IsDefaultDiet = true,
                Name = Resources.LabelGrowerFinisherDiet4,
                DietType = DietType.GrowerFinisherDiet4,
                AnimalType = AnimalType.Swine,
                Comments = Resources.LabelDietD4,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.GrowerFinisherDiet3).Select(x => (FeedIngredient)x)),

                DailyDryMatterFeedIntakeOfFeed = 2.93,
                CrudeProtein = 19.66,
            });

            // Diet E - Gilt developer diet
            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Resources.LabelGiltDeveloperDiet,
                DietType = DietType.GiltDeveloperDiet,
                AnimalType = AnimalType.Swine,
                Comments = Resources.LabelGiltDeveloperDiet,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.GiltDeveloperDiet).Select(x => (FeedIngredient)x)),

                DailyDryMatterFeedIntakeOfFeed = 2.80,
                CrudeProtein = 17.95,
            });

            // Diet F - Boar diet
            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Resources.LabelBoarDiet,
                DietType = DietType.Boars,
                AnimalType = AnimalType.Swine,
                Comments = Resources.LabelBoarDiet,

                Ingredients = new ObservableCollection<FeedIngredient>(_feedIngredientProvider.GetIngredientsForDiet(AnimalType.Swine, DietType.Boars).Select(x => (FeedIngredient)x)),

                DailyDryMatterFeedIntakeOfFeed = 3,
                CrudeProtein = 20.1,
            });

            return diets;
        }

        private IEnumerable<Diet> GetSheepDiets()
        {
            var diets = new List<Diet>();

            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Core.Properties.Resources.GoodQualityForage,
                DietType = DietType.GoodQualityForage,
                AnimalType = AnimalType.Sheep,
                TotalDigestibleNutrient = 60,
                CrudeProtein = 17.7,
                Ash = 8,
                MethaneConversionFactor = 0.067,
            });

            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Core.Properties.Resources.AverageQualityForage,
                AnimalType = AnimalType.Sheep,
                DietType = DietType.AverageQualityForage,
                TotalDigestibleNutrient = 55,
                CrudeProtein = 12.4,
                Ash = 8,
                MethaneConversionFactor = 0.067,
            });

            diets.Add(new Diet()
            {
                IsDefaultDiet = true,
                Name = Core.Properties.Resources.PoorQualityForage,
                DietType = DietType.PoorQualityForage,
                AnimalType = AnimalType.Sheep,
                TotalDigestibleNutrient = 48,
                CrudeProtein = 5.7,
                Ash = 8,
                MethaneConversionFactor = 0.067,
            });

            return diets;
        }

        /// <summary>
        /// Some animal groups will not have a diet (poultry, other livestock, suckling pigs, etc.). In these cases, a non-null diet must still be set.
        /// </summary>
        public Diet GetNoDiet()
        {
            return new Diet()
            {
                Name = Resources.None,
                IsCustomPlaceholderDiet = true,
                DietType = DietType.None,
                AnimalType = AnimalType.NotSelected,
            };
        }

        #endregion
    }
}