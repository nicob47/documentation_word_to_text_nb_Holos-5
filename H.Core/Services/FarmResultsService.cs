using System.Collections.ObjectModel;
using System.Diagnostics;
using H.Core.Calculators.Economics;
using H.Core.Calculators.Infrastructure;
using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Mappers;
using H.Core.Emissions.Results;
using H.Core.Events;
using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Models.LandManagement.Fields;
using H.Core.Providers;
using H.Core.Providers.Animals;
using H.Core.Providers.Climate;
using H.Core.Providers.Evapotranspiration;
using H.Core.Providers.Feed;
using H.Core.Providers.Precipitation;
using H.Core.Providers.Soil;
using H.Core.Providers.Temperature;
using H.Core.Services.Animals;
using H.Core.Services.LandManagement;
using Prism.Events;
using NLog;

namespace H.Core.Services
{
    /// <summary>
    /// Legacy v4-era farm orchestration service. Predates the
    /// <see cref="H.Core.Services.Analysis.FarmAnalysisService"/> façade that the Avalonia GUI
    /// now uses — this class wired the carbon / N / animal / economics / AD pipelines together
    /// the v4 way (one big composite service injected via Prism).
    ///
    /// <para><b>Current role:</b></para>
    /// Kept in v5 because the CLI and a number of bulk-import / reporting paths still depend
    /// on it. The Avalonia GUI runs through the slimmer
    /// <see cref="H.Core.Services.Analysis.FarmAnalysisService"/> instead. Both ultimately
    /// drive the same <see cref="IFieldResultsService"/> + animal-pipeline stack.
    ///
    /// <para>
    /// New GUI features should depend on <see cref="H.Core.Services.Analysis.IFarmAnalysisService"/>
    /// rather than this type — the DTO façade is the testable seam.
    /// </para>
    /// </summary>
    public class FarmResultsService : IFarmResultsService
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Fields

        private readonly  IInitializationService _initializationService;

        private readonly IManureService _manureService;

        private readonly IFieldComponentHelper _fieldComponentHelper = new FieldComponentHelper();
        private readonly IAnimalComponentHelper _animalComponentHelper = new AnimalComponentHelper();

        private readonly IFieldResultsService _fieldResultsService;
        private readonly IAnimalService _animalResultsService;
        private readonly IADCalculator _adCalculator;

        private readonly IDietProvider _dietProvider = new DietProvider();
        private readonly Table_6_Manure_Types_Default_Composition_Provider _defaultManureCompositionProvider = new Table_6_Manure_Types_Default_Composition_Provider();
        private readonly Table_30_Default_Bedding_Material_Composition_Provider _defaultBeddingMaterialCompositionProvider = new Table_30_Default_Bedding_Material_Composition_Provider();


        private readonly IEventAggregator _eventAggregator;

        private readonly EconomicsCalculator _economicsCalculator;
        private readonly UnitsOfMeasurementCalculator _unitsCalculator = new UnitsOfMeasurementCalculator();

        #endregion

        #region Constructors
        public FarmResultsService(IEventAggregator eventAggregator, IFieldResultsService fieldResultsService, IADCalculator adCalculator, IManureService manureService, IAnimalService animalService)
        {
            if (animalService != null)
            {
                _animalResultsService = animalService;
            }
            else
            {
                throw new ArgumentNullException(nameof(animalService));
            }

            if (manureService != null)
            {
                _manureService = manureService;
            }
            else
            {
                throw new ArgumentNullException(nameof(manureService));
            }

            if (adCalculator != null)
            {
                _adCalculator = adCalculator;
            }
            else
            {
                throw new ArgumentNullException(nameof(adCalculator));
            }

            if (fieldResultsService != null)
            {
                _fieldResultsService = fieldResultsService;
                _economicsCalculator = new EconomicsCalculator(_fieldResultsService);
            }
            else
            {
                throw new ArgumentNullException(nameof(fieldResultsService));
            }

            if (eventAggregator != null)
            {
                _eventAggregator = eventAggregator;
            }
            else
            {
                throw new ArgumentNullException(nameof(eventAggregator));
            }

            _initializationService = new InitializationService();
        }

        #endregion

        #region Properties

        public bool CropEconomicDataApplied { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates final results for a farm.
        /// </summary>
        public FarmEmissionResults CalculateFarmEmissionResults(Farm farm)
        {
            var farmResults = new FarmEmissionResults();
            farmResults.Farm = farm;

            if (farm.PolygonId == 0)
            {
                // Error
                return farmResults;
            }

            _log.Info($"{nameof(FarmResultsService)}.{nameof(CalculateFarmEmissionResults)}: calculating results for farm: '{farm.Name}'");

            if (farm.Components.Any() == false)
            {
                _log.Info($"{nameof(FarmResultsService)}.{nameof(CalculateFarmEmissionResults)}: no components for farm: '{farm.Name}' found.");
            }

            _initializationService.CheckInitialization(farm);

            // Field results will use animal results to calculate indirect emissions from land applied manure. We will need to reset the animal component calculation state here.
            farm.ResetAnimalResults();

            var animalResults = _animalResultsService.GetAnimalResults(farm);

            farmResults.AnimalComponentEmissionsResults.AddRange(animalResults);
            _fieldResultsService.AnimalResults = animalResults;

            farmResults.AnaerobicDigestorResults.AddRange(this.CalculateAdResults(farm, animalResults.ToList()));

            farmResults.FinalFieldResultViewItems.AddRange(this.CalculateFieldResults(farm));

            // Manure calculations - must be calculated after both field and animal results have been calculated.
            _manureService.Initialize(farm, animalResults);

            // Economics
            farmResults.EconomicResultsViewItems.AddRange(_economicsCalculator.CalculateCropResults(farmResults));
            farmResults.EconomicsProfit = _economicsCalculator.GetTotalProfit(farmResults.EconomicResultsViewItems);

            _eventAggregator.GetEvent<FarmResultsCalculatedEvent>().Publish(new FarmResultsCalculatedEventArgs() { FarmEmissionResults = farmResults });

            _log.Info($"{nameof(FarmResultsService)}.{nameof(CalculateFarmEmissionResults)}: results for farm: '{farm.Name}' calculated. {farmResults.ToString()}");

            return farmResults;
        }

        public List<CropViewItem> CalculateFieldResults(Farm farm)
        {
            // Field results
            var finalFieldResults = _fieldResultsService.CalculateFinalResults(farm);

            return finalFieldResults;
        }

        public List<DigestorDailyOutput> CalculateAdResults(Farm farm, List<AnimalComponentEmissionsResults> animalComponentEmissionsResults)
        {
            return _adCalculator.CalculateResults(farm, animalComponentEmissionsResults);
        }

        /// <summary>
        /// Calculates final results for a collection of farms.
        /// </summary>
        public List<FarmEmissionResults> CalculateFarmEmissionResults(IEnumerable<Farm> farms)
        {
            var result = new List<FarmEmissionResults>();

            foreach (var farm in farms)
            {
                result.Add(this.CalculateFarmEmissionResults(farm));
            }

            return result;
        }

        public List<Farm> ReplicateFarms(IEnumerable<Farm> farms)
        {
            var result = new List<Farm>();

            foreach (var farm in farms)
            {
                result.Add(this.ReplicateFarm(farm));
            }

            return result;
        }

        public Farm ReplicateFarm(Farm farm)
        {
            var replicatedFarm = new Farm
            {
                GeographicData = new GeographicData()
            };

            PropertyMapper.CopyTo(farm, replicatedFarm);

            // PropertyMapper copies all matching properties including Guid and reference-type sub-objects.
            // Reset Guid so the replicated farm has its own unique identity.
            replicatedFarm.Guid = Guid.NewGuid();

            // Reset reference-type properties to new instances so they are not shared with the source farm.
            // PropertyMapper.CopyTo above copies these references, so we must create fresh objects before copying into them.
            replicatedFarm.Defaults = new Defaults();
            replicatedFarm.ClimateData = new ClimateData();
            replicatedFarm.GeographicData = new GeographicData();

            PropertyMapper.CopyTo(farm.Defaults, replicatedFarm.Defaults);
            PropertyMapper.CopyTo(farm.ClimateData, replicatedFarm.ClimateData);
            PropertyMapper.CopyTo(farm.GeographicData, replicatedFarm.GeographicData);

            // Reset DefaultSoilData so it is not shared with the source GeographicData (CopyTo copied the reference).
            replicatedFarm.GeographicData.DefaultSoilData = new SoilData();
            PropertyMapper.CopyTo(farm.GeographicData.DefaultSoilData, replicatedFarm.GeographicData.DefaultSoilData);

            replicatedFarm.Name = farm.Name;

            #region Animal Components
            foreach (var animalComponent in farm.AnimalComponents.Cast<AnimalComponentBase>())
            {
                var replicatedAnimalComponent = _animalComponentHelper.ReplicateAnimalComponent(animalComponent);

                replicatedFarm.Components.Add(replicatedAnimalComponent);
            }
            #endregion

            #region FieldSystemComponents

            foreach (var fieldSystemComponent in farm.FieldSystemComponents)
            {
                var replicatedFieldSystemComponent = _fieldComponentHelper.Replicate(fieldSystemComponent);

                replicatedFarm.Components.Add(replicatedFieldSystemComponent);
            }

            #endregion

            #region StageStates
            foreach (var fieldSystemDetailsStageState in farm.StageStates.OfType<FieldSystemDetailsStageState>().ToList())
            {
                var stageState = new FieldSystemDetailsStageState();
                replicatedFarm.StageStates.Add(stageState);

                foreach (var detailsScreenViewCropViewItem in fieldSystemDetailsStageState.DetailsScreenViewCropViewItems)
                {
                    var viewItem = new CropViewItem();

                    PropertyMapper.CopyTo(detailsScreenViewCropViewItem, viewItem);

                    stageState.DetailsScreenViewCropViewItems.Add(viewItem);
                }
            }

            #endregion

            #region GeographicData
            foreach (var soilData in farm.GeographicData.SoilDataForAllComponentsWithinPolygon)
            {
                var replicatedSoilData = new SoilData();
                PropertyMapper.CopyTo(soilData, replicatedSoilData);
                replicatedFarm.GeographicData.SoilDataForAllComponentsWithinPolygon.Add(replicatedSoilData);
            }

            foreach (var customYieldData in farm.GeographicData.CustomYieldData)
            {
                var replicatedCustomYieldData = new CustomUserYieldData();
                PropertyMapper.CopyTo(customYieldData, replicatedCustomYieldData);
                replicatedFarm.GeographicData.CustomYieldData.Add(replicatedCustomYieldData);
            }
            #endregion

            #region ClimateData
            foreach (var dailyClimateData in farm.ClimateData.DailyClimateData)
            {
                var replicatedDailyClimateData = new DailyClimateData();
                PropertyMapper.CopyTo(dailyClimateData, replicatedDailyClimateData);
                replicatedFarm.ClimateData.DailyClimateData.Add(dailyClimateData);
            }
            #endregion

            return replicatedFarm;
        }

        #endregion
    }
}