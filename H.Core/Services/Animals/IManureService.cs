using H.Core.Emissions.Results;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Providers.Animals;

namespace H.Core.Services.Animals
{
    /// <summary>
    /// Single source of truth for manure C, N, and TAN (total ammoniacal nitrogen) accounting
    /// across the farm. Tracks how manure flows through the system:
    /// <list type="number">
    ///   <item><b>Creation</b> — each animal component produces manure (volume / N / C / TAN per year).</item>
    ///   <item><b>Storage</b> — accumulated in per-(animalType, year, manureStateType) "tanks".</item>
    ///   <item><b>Use</b> — drawn down by field applications, imports/exports, and grazing deposits.</item>
    /// </list>
    ///
    /// <para><b>Where the carbon pipeline uses it:</b></para>
    /// <see cref="H.Core.Calculators.Nitrogen.N2OEmissionFactorCalculator"/> calls into this
    /// service for per-field manure N totals (direct + indirect N₂O) and per-field manure C
    /// totals (feeds the ICBM young-manure pool / Tier 2 active pool).
    ///
    /// <para><b>Lifecycle:</b></para>
    /// Must be <see cref="Initialize"/>-d with the farm + the precomputed animal results
    /// before any of the per-tank lookup methods will return real values. The orchestrator in
    /// <c>FieldResultsService.CalculateFinalResultsForField</c> handles this priming for both
    /// strategies.
    /// </summary>
    public interface IManureService
    {
        /// <summary>
        /// Prime the service with the farm context and the precomputed per-component animal
        /// results. Builds the per-(animalType, year, state) tanks that subsequent lookup
        /// methods read. Call this once per analysis pass; calling repeatedly with stale data
        /// will produce stale totals.
        /// </summary>
        void Initialize(Farm farm, List<AnimalComponentEmissionsResults> animalComponentEmissionsResults);
        ManureTank GetTank(AnimalType animalType, int year, Farm farm, ManureStateType manureStateType);
        List<AnimalType> GetValidManureTypes();
        List<AnimalType> GetManureCategoriesProducedOnFarm(Farm farm);
        double GetVolumeAvailableForExport(int year);
        double GetVolumeAvailableForExport(int year, Farm farm, AnimalType animalType);
        List<ManureApplicationTypes> GetValidManureApplicationTypes();
        List<ManureLocationSourceType> GetValidManureLocationSourceTypes();
        List<ManureStateType> GetValidManureStateTypes(Farm? farm, ManureLocationSourceType locationSourceType, AnimalType animalType);
        void SetValidManureStateTypes(ManureItemBase manureItemBase, Farm farm);

        /// <summary>
        /// (kg)
        /// </summary>
        double GetTotalVolumeOfManureExported(int year, Farm farm);

        /// <summary>
        /// (kg)
        /// </summary>
        double GetTotalVolumeOfManureExported(int year, Farm farm, AnimalType animalType);
        int GetYearHighestVolumeRemaining(AnimalType animalType);
        DefaultManureCompositionData GetManureCompositionData(ManureItemBase? manureItemBase, Farm? farm);

        /// <summary>
        /// Returns total TAN created by all animals on farm in specified year
        /// 
        /// (kg)
        /// </summary>
        double GetTotalTANCreated(int year);

        /// <summary>
        /// (kg)
        /// </summary>
        double GetTotalTANCreated(int year, AnimalType animalType);

        /// <summary>
        /// (kg)
        /// </summary>
        double GetTotalNitrogenCreated(int year);

        

        /// <summary>
        /// (kg)
        /// </summary>
        double GetTotalVolumeCreated(int year);

        /// <summary>
        /// (kg)
        /// </summary>
        double GetTotalVolumeCreated(int year, AnimalType animalType);

        /// <summary>
        /// Equation 2.7.2-12
        /// Equation 4.6.1-8
        /// Equation 4.6.2-17
        /// Equation 4.6.2-18
        ///
        /// (kg N)
        /// </summary>
        double GetTotalNitrogenFromExportedManure(int year, Farm farm);

        double GetTotalNitrogenFromExportedManure(int year, Farm farm, AnimalType animalType);
        double GetTotalNitrogenFromManureImports(int year, Farm farm, AnimalType animalType);

        /// <summary>
        /// Equation 4.6.2-13
        /// 
        /// Nitrogen sum of all manure applications made to all fields from manure produced on the farm (does not include imported manure)
        ///
        /// (kg N)
        /// </summary>
        double GetTotalNitrogenAppliedToAllFields(int year);

        /// <summary>
        /// Equation 4.6.2-14
        /// 
        /// Stored nitrogen available for application to land minus manure applied to fields or exported
        ///
        /// (kg N)
        /// </summary>
        double GetTotalManureNitrogenRemainingForFarmAndYear(int year, Farm farm);



        /// <summary>
        /// (kg)
        /// </summary>
        double GetTotalNitrogenCreated(int year, AnimalType animalType);

        List<AnimalType> GetManureTypesExported(Farm farm, int year);
        List<AnimalType> GetManureTypesImported(Farm farm, int year);
        double GetFractionOfTotalManureUsedFromLandApplication(CropViewItem viewItem, ManureApplicationViewItem manureApplicationViewItem);
        double GetAmountOfTanUsedDuringLandApplication(CropViewItem cropViewItem, ManureApplicationViewItem manureApplicationViewItem);
        double GetAmountOfTanUsedDuringLandApplications(CropViewItem cropViewItem);
        double GetAmountOfTanExported(ManureExportViewItem manureExportViewItem, int year);
        List<Tuple<double, AnimalType>> GetTANExportedForFarm(Farm farm, int year);
        List<int> GetYearsWithManureApplied(Farm farm);

        /// <summary>
        /// Returns the total amount of TAN used (by animal/manure type) from all field applications on the farm
        /// </summary>
        List<Tuple<double, AnimalType>> GetTotalTanAppliedToAllFields(int year, List<CropViewItem> viewItems);

        /// <summary>
        /// Returns the total amount of TAN used (by animal/manure type) from all field applications on the field
        /// </summary>
        List<Tuple<double, AnimalType>> GetTotalTanAppliedToField(int year, CropViewItem cropViewItem);

        double GetTotalTANExportedByAnimalType(
            AnimalType animalType,
            Farm farm,
            int year);

        /// <summary>
        /// Equation 4.1.3-16
        ///
        /// Total_C_storage
        ///  
        /// (kg C)
        /// </summary>
        double GetTotalCarbonCreated(int year);

        /// <summary>
        /// (kg C) 
        /// </summary>
        double GetTotalCarbonFromImportedManure(Farm farm, int year);

        /// <summary>
        /// Equation 4.7.1-3
        ///
        /// (kg C)
        /// </summary>
        double GetTotalCarbonRemainingForFarm(Farm farm, int year, CropViewItem viewItem);

        /// <summary>
        /// Equation 4.7.1-4
        ///
        /// (kg C)
        /// </summary>
        double GetTotalCarbonRemainingForField(Farm farm, int year, CropViewItem viewItem);

        /// <summary>
        /// Equation 4.7.1-5
        /// </summary>
        double GetTotalCarbonFromExportedManure(int year, Farm farm);

        

        /// <summary>
        /// Equation 4.7.2-1
        /// </summary>
        double GetTotalManureCarbonInputsForField(Farm farm, int year, CropViewItem viewItem);

        List<ManureStateType> GetManureStatesProducedOnFarm(Farm farm);
        double GetTotalVolumeRemainingForFarmAndYear(int year, Farm farm);

        /// <summary>
        /// Returns a list of items that represent a month and the volume of manure that was applied to the field in that month.
        /// </summary>
        List<MonthlyManureSpreadingData> GetMonthlyManureSpreadingData(
            CropViewItem viewItem,
            Farm farm);
    }
}