using System.Diagnostics;
using H.Core.Calculators.Carbon;
using H.Core.Mappers;
using H.Core.Calculators.Climate;
using H.Core.Calculators.Economics;
using H.Core.Calculators.Nitrogen;
using H.Core.Calculators.Tillage;
using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Emissions.Results;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Providers.Carbon;
using H.Core.Providers.Economics;
using H.Core.Providers.Energy;
using H.Core.Providers.Fertilizer;
using H.Core.Providers.Nitrogen;
using H.Core.Providers.Plants;
using H.Core.Providers.Soil;
using H.Core.Services.Animals;
using NLog;

namespace H.Core.Services.LandManagement
{
    public partial class FieldResultsService : IFieldResultsService
    {
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        #region Fields

        private const int DefaultNumberOfDecimalPlaces = 3;

        private readonly IrrigationService _irrigationService = new IrrigationService();

        private readonly IClimateParameterCalculator _climateParameterCalculator = new ClimateParameterCalculator();
        private readonly ICBMSoilCarbonCalculator _icbmSoilCarbonCalculator;
        private readonly IPCCTier2SoilCarbonCalculator _tier2SoilCarbonCalculator;
        private readonly ITillageFactorCalculator _tillageFactorCalculator = new TillageFactorCalculator();
        private readonly UnitsOfMeasurementCalculator _unitsCalculator = new UnitsOfMeasurementCalculator();
        private readonly N2OEmissionFactorCalculator _n2OEmissionFactorCalculator;
        private readonly DigestateService _digestateService = new DigestateService();
        private readonly IManureService _manureService = new ManureService();

        private readonly LandManagementChangeHelper _landManagementChangeHelper = new LandManagementChangeHelper();
        private readonly EconomicsHelper _economicsHelper = new EconomicsHelper();
        private readonly FieldComponentHelper _fieldComponentHelper = new FieldComponentHelper();



        private readonly Table_48_Carbon_Footprint_For_Fertilizer_Blends_Provider _carbonFootprintForFertilizerBlendsProvider = new Table_48_Carbon_Footprint_For_Fertilizer_Blends_Provider();
        private readonly Table_9_Nitrogen_Lignin_Content_In_Crops_Provider _slopeProviderTable = new Table_9_Nitrogen_Lignin_Content_In_Crops_Provider();
        private readonly LumCMax_KValues_Perennial_Cropping_Change_Provider _lumCMaxKValuesPerennialCroppingChangeProvider = new LumCMax_KValues_Perennial_Cropping_Change_Provider();
        private readonly LumCMax_KValues_Fallow_Practice_Change_Provider _lumCMaxKValuesFallowPracticeChangeProvider = new LumCMax_KValues_Fallow_Practice_Change_Provider();
        private readonly SmallAreaYieldProvider _smallAreaYieldProvider = new SmallAreaYieldProvider();
        private readonly Table50FuelEnergyEstimatesProvider _fuelEnergyEstimatesProvider = new Table50FuelEnergyEstimatesProvider();
        private readonly Table_51_Herbicide_Energy_Estimates_Provider _herbicideEnergyEstimatesProvider = new Table_51_Herbicide_Energy_Estimates_Provider();
        private readonly EcodistrictDefaultsProvider _ecodistrictDefaultsProvider = new EcodistrictDefaultsProvider();
        private readonly NitogenFixationProvider _nitrogenFixationProvider = new NitogenFixationProvider();
        private readonly Table_60_Utilization_Rates_For_Livestock_Grazing_Provider _utilizationRatesForLivestockGrazingProvider = new Table_60_Utilization_Rates_For_Livestock_Grazing_Provider();
        private readonly ICustomFileYieldProvider _customFileYieldProvider = new CustomFileYieldProvider();
        private readonly Table_7_Relative_Biomass_Information_Provider _relativeBiomassInformationProvider = new Table_7_Relative_Biomass_Information_Provider();
        private readonly CropEconomicsProvider _economicsProvider = new CropEconomicsProvider();

        // Memoization cache for CalculateClimateParameter(viewItem, farm).
        //
        // Why this is safe to cache:
        //   CalculateClimateParameter is a pure function of (viewItem.Year, viewItem.Yield,
        //   viewItem.CropType.IsPerennial(), viewItem.IrrigationType, viewItem.AmountOfIrrigation,
        //   farm.GetPreferredSoilData(viewItem), farm.Defaults, farm.ClimateData). The inner
        //   365-day loop resets soilTemperaturePrevious/waterTemperaturePrevious to 0 each call,
        //   so there is no cross-year state — two calls with identical inputs always produce the
        //   same scalar. Downstream "previous year" references in the carbon calculators read
        //   CropViewItem.ClimateParameter, which we still assign per year, so caching does not
        //   change any inter-year linkage.
        //
        // Lifecycle:
        //   The cache is cleared at the start of every InitializeStageState(farm) call. The
        //   stage state is already invalidated on Recalculate / navigation, so user edits cannot
        //   silently reuse a stale climate value.
        //
        // Why reference identity is sufficient for SoilData / Defaults / ClimateData:
        //   These are large composite objects shared across all view items in a single analysis
        //   run. Within one InitializeStageState pass the user cannot mutate them. Identity
        //   comparison avoids both the cost and the bug surface of a deep equality check.
        private readonly Dictionary<ClimateParameterCacheKey, double> _climateParameterCache
            = new Dictionary<ClimateParameterCacheKey, double>();

        private readonly record struct ClimateParameterCacheKey(
            int Year,
            double Yield,
            bool IsPerennial,
            Enumerations.IrrigationType IrrigationType,
            double AmountOfIrrigation,
            Providers.Soil.SoilData SoilData,
            Models.Defaults Defaults,
            Providers.Climate.ClimateData ClimateData);

        #endregion

        #region Constructors

        public FieldResultsService(
            ICBMSoilCarbonCalculator icbmSoilCarbonCalculator, 
            IPCCTier2SoilCarbonCalculator ipccTier2SoilCarbonCalculator, 
            N2OEmissionFactorCalculator n2OEmissionFactorCalculator)
        {
            if (icbmSoilCarbonCalculator != null)
            {
                _icbmSoilCarbonCalculator = icbmSoilCarbonCalculator;
            }
            else
            {
                throw new ArgumentNullException(nameof(icbmSoilCarbonCalculator));
            }

            if (ipccTier2SoilCarbonCalculator != null)
            {
                _tier2SoilCarbonCalculator = ipccTier2SoilCarbonCalculator;
            }
            else
            {
                throw new ArgumentNullException(nameof(ipccTier2SoilCarbonCalculator));
            }

            if (n2OEmissionFactorCalculator != null)
            {
                _n2OEmissionFactorCalculator = n2OEmissionFactorCalculator;
            }
            else
            {
                throw new ArgumentNullException(nameof(n2OEmissionFactorCalculator));
            }
            _smallAreaYieldProvider.Initialize();

            this.AnimalResults = new List<AnimalComponentEmissionsResults>();
            this.AnimalResultsService = new AnimalResultsService();
        }

        #endregion

        #region Properties

        public List<AnimalComponentEmissionsResults> AnimalResults { get; set; }

        public IAnimalService AnimalResultsService { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Calculates final multiyear C and N2O results for a collection of farms
        /// </summary>
        public List<CropViewItem> CalculateFinalResults(IEnumerable<Farm> farms)
        {
            var results = new List<CropViewItem>();

            foreach (var farm in farms)
            {
                var result = this.CalculateFinalResults(farm);
                results.AddRange(result);
            }

            return results;
        }

        /// <summary>
        /// Calculates final multiyear C and N2O results for a farm
        /// </summary>
        public List<CropViewItem> CalculateFinalResults(Farm farm)
        {
            var totalSw = System.Diagnostics.Stopwatch.StartNew();
            long groupMs = 0, combineMs = 0, mergeMs = 0, fieldCalcMs = 0, avgSocMs = 0;
            int fieldCount = 0;
            var result = new List<CropViewItem>();

            // Get all of the detail view items for all fields for this farm
             var detailsStageState = this.GetStageState(farm);
            if (detailsStageState != null)
            {
                /*
                 * Group all detail view items by field GUID, then create a result view item for each. This is required since the stage state will hold
                 * detail view items for all fields on a farm
                 */

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var viewItemsGroupedByField =  detailsStageState.DetailsScreenViewCropViewItems.GroupBy(viewItem => viewItem.FieldSystemComponentGuid);
                groupMs = sw.ElapsedMilliseconds;
                foreach (var groupingByFieldSystem in viewItemsGroupedByField)
                {
                    var fieldGuid = groupingByFieldSystem.Key;
                    var fieldSystemComponent = farm.GetFieldSystemComponent(fieldGuid);
                    if (fieldSystemComponent is null)
                    {
                        continue;
                    }
                    fieldCount++;

                    var detailViewItems = groupingByFieldSystem.ToList();

                    /*
                     * At this point there could be multiple items for one year (e.g. a main crop and a cover crop or an undersown crop), here we combine
                     * multiple inputs from same year into the main crop
                     */
                    sw.Restart();
                    this.CombineInputsForAllCropsInSameYear(detailViewItems, fieldSystemComponent);
                    combineMs += sw.ElapsedMilliseconds;

                    // Merge multiple items with the same year into a single year view items so that no two view items have the same year when calculating ICBM results (ICBM calculations
                    // require exactly one item per year (with combined inputs when there is a secondary crop grown)
                    sw.Restart();
                    var mergedItems = this.MergeDetailViewItems(detailViewItems, fieldSystemComponent);
                    mergeMs += sw.ElapsedMilliseconds;

                    sw.Restart();
                    this.CalculateFinalResultsForField(
                        viewItemsForField: mergedItems, 
                        farm: farm, 
                        fieldSystemGuid: groupingByFieldSystem.Key);
                    fieldCalcMs += sw.ElapsedMilliseconds;

                    result.AddRange(mergedItems);
                }

                var sw2 = System.Diagnostics.Stopwatch.StartNew();
                this.CalculateAverageSoilOrganicCarbonForFields(result);
                avgSocMs = sw2.ElapsedMilliseconds;
            }

            totalSw.Stop();
            _log.Info(
                $"[GHGAnalysis.Field] total={totalSw.ElapsedMilliseconds}ms group={groupMs}ms combine={combineMs}ms " +
                $"merge={mergeMs}ms fieldCalc={fieldCalcMs}ms avgSoc={avgSocMs}ms fields={fieldCount} rows={result.Count}");

            return result;
        }

        /// <summary>
        /// Calculate climate parameter. Will use custom climate data if it exists for the farm, otherwise will use SLC normals
        /// for climate data.
        /// </summary>
        public double CalculateClimateParameter(CropViewItem viewItem, Farm farm)
        {
            // Memoization: the 365-day climate-parameter loop dominates AssignCarbonInputs
            // (~60-237ms per field in profiling). Within a single InitializeStageState pass
            // the inputs that drive this calculation rarely change across the 30 view items
            // for a field, so most calls are cache hits after the first year of each crop
            // configuration. See _climateParameterCache for the safety rationale.
            var soilDataForKey = farm.GetPreferredSoilData(viewItem);
            var key = new ClimateParameterCacheKey(
                Year: viewItem.Year,
                Yield: viewItem.Yield,
                IsPerennial: viewItem.CropType.IsPerennial(),
                IrrigationType: viewItem.IrrigationType,
                AmountOfIrrigation: viewItem.AmountOfIrrigation,
                SoilData: soilDataForKey,
                Defaults: farm.Defaults,
                ClimateData: farm.ClimateData);

            if (_climateParameterCache.TryGetValue(key, out var cached))
            {
                return cached;
            }

            var computed = this.CalculateClimateParameterCore(viewItem, farm);
            _climateParameterCache[key] = computed;
            return computed;
        }

        /// <summary>
        /// Original climate-parameter computation. Split out from <see cref="CalculateClimateParameter"/>
        /// so the cache wrapper above stays small and the underlying math is unchanged.
        /// </summary>
        private double CalculateClimateParameterCore(CropViewItem viewItem, Farm farm)
        {
            if (farm.ClimateData.DailyClimateData.Any())
            {
                // O(1) dictionary lookup into the per-year cache maintained on ClimateData,
                // plus a single OrderBy over just that one year's entries (~365). Replaces the
                // earlier per-call Where over the full multi-year collection.
                var orderedForYear = farm.ClimateData.GetDailyClimateDataForYearSortedByJulianDay(viewItem.Year);

                var climateParameter = 0d;

                if (orderedForYear.Count > 0)
                {
                    // Use daily climate data — three projections share the same already-sorted list.
                    var precipitationList = orderedForYear.Select(d => d.MeanDailyPrecipitation).ToList();
                    var totalPrecipitationList = _irrigationService.AddIrrigationToDailyPrecipitations(precipitationList, farm, viewItem);
                    var temperatureList = orderedForYear.Select(d => d.MeanDailyAirTemperature).ToList();
                    var evapotranspirationList = orderedForYear.Select(d => d.MeanDailyPET).ToList();

                    climateParameter = _climateParameterCalculator.CalculateClimateParameterForYear(
                        farm: farm,
                        cropViewItem: viewItem,
                        evapotranspirations: evapotranspirationList,
                        precipitations: totalPrecipitationList,
                        temperatures: temperatureList);
                }
                else
                {
                    // If user has entered custom climate data but their input file has no data for a particular year, then use normals for that particular year
                    
                    // Add irrigation amounts to daily precipitations
                    var totalPrecipitationList = _irrigationService.AddIrrigationToDailyPrecipitations(farm.ClimateData.PrecipitationData.GetAveragedYearlyValues(), farm, viewItem);

                    climateParameter = _climateParameterCalculator.CalculateClimateParameterForYear(
                        farm: farm,
                        cropViewItem: viewItem,
                        evapotranspirations: farm.ClimateData.EvapotranspirationData.GetAveragedYearlyValues(),
                        precipitations: totalPrecipitationList,
                        temperatures: farm.ClimateData.TemperatureData.GetAveragedYearlyValues());
                }

                return Math.Round(climateParameter, DefaultNumberOfDecimalPlaces);
            }
            else
            {
                // Add irrigation amounts to daily precipitations
                var totalPrecipitationList = _irrigationService.AddIrrigationToDailyPrecipitations(farm.ClimateData.PrecipitationData.GetAveragedYearlyValues(), farm, viewItem);

                // Use SLC normals when there is no custom user climate data
                _log.Warn($"{nameof(FieldResultsService)}: No custom daily climate data exists for this farm. Defaulting to SLC climate normals (and averaged daily values)");

                var result = _climateParameterCalculator.CalculateClimateParameterForYear(
                    farm: farm,
                    cropViewItem: viewItem,
                    evapotranspirations: farm.ClimateData.EvapotranspirationData.GetAveragedYearlyValues(),
                    precipitations: totalPrecipitationList,
                    temperatures: farm.ClimateData.TemperatureData.GetAveragedYearlyValues());

                return Math.Round(result, DefaultNumberOfDecimalPlaces);
            }
        }

        public double CalculateTillageFactor(CropViewItem viewItem, Farm farm)
        {
            var soilData = farm.GetPreferredSoilData(viewItem);

            var result = _tillageFactorCalculator.CalculateTillageFactor(
                province: soilData.Province,
                soilFunctionalCategory: farm.GeographicData.DefaultSoilData.SoilFunctionalCategory,
                tillageType: viewItem.TillageType,
                cropType: viewItem.CropType,
                perennialYear: viewItem.YearInPerennialStand);

            return Math.Round(result, DefaultNumberOfDecimalPlaces);
        }

        public double CalculateManagementFactor(double climateParameter, double tillageFactor)
        {
            var result = _climateParameterCalculator.CalculateClimateManagementFactor(climateParameter, tillageFactor);

            return Math.Round(result, DefaultNumberOfDecimalPlaces);
        }

        public Table_7_Relative_Biomass_Information_Data? GetResidueData(CropViewItem cropViewItem, Farm farm)
        {
            var soilData = farm.GetPreferredSoilData(cropViewItem);

            var province = soilData.Province;
            var residueData = _relativeBiomassInformationProvider.GetResidueData(
                irrigationType: cropViewItem.IrrigationType,
                irrigationAmount: cropViewItem.AmountOfIrrigation,
                cropType: cropViewItem.CropType,
                soilFunctionalCategory: soilData.SoilFunctionalCategory,
                province: province);

            return residueData;
        }

        public List<GroupEmissionsByMonth> GetGroupEmissionsFromGrazingAnimals(
            List<AnimalComponentEmissionsResults> results,
            GrazingViewItem grazingViewItem)
        {
            var result = new List<GroupEmissionsByMonth>();

            // Get all animal components that have been placed on this field for grazing.
            var animalComponentEmissionsResults = results.SingleOrDefault(x => x.Component.Guid == grazingViewItem.AnimalComponentGuid);
            if (animalComponentEmissionsResults != null)
            {
                //Get all animal groups that have been placed on this field for grazing.
                var groupEmissionResults = animalComponentEmissionsResults.EmissionResultsForAllAnimalGroupsInComponent.SingleOrDefault(x => x.AnimalGroup.Guid == grazingViewItem.AnimalGroupGuid);
                if (groupEmissionResults != null)
                {
                    // Get emissions from the group when they are placed on pasture (housing type is pasture)
                    foreach (var groupEmissionsByMonth in groupEmissionResults.GroupEmissionsByMonths)
                    {
                        if (groupEmissionsByMonth.MonthsAndDaysData.ManagementPeriod.HousingDetails.HousingType.IsPasture())
                        {
                            var start = groupEmissionsByMonth.MonthsAndDaysData.ManagementPeriod.Start;
                            var end = groupEmissionsByMonth.MonthsAndDaysData.ManagementPeriod.End;

                            if (start >= grazingViewItem.Start && end <= grazingViewItem.End)
                            {
                                result.Add(groupEmissionsByMonth);
                            }
                        }
                    }
                }
            }

            return result;
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
