using H.Core.Factories.Animals;
using H.Core.Factories.Animals.Dairy;
using H.Core.Models.Animals.Dairy;
using H.Core.Services.Animals.Dairy;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Prism.Events;
using Prism.Regions;
using System.Linq;
using System;
using H.Core.Enumerations;

namespace H.Avalonia.ViewModels.ComponentViews.Dairy;

/// <summary>
/// Design-time ViewModel for the Dairy Component view, providing sample data for the Avalonia designer.
/// This allows designers to see realistic data while building the UI without running the full application.
/// </summary>
public class DairyComponentViewModelDesign : DairyComponentViewModel
{
    /// <summary>
    /// Parameterless constructor for design-time support.
    /// Initializes the ViewModel with sample dairy herd data for visualization in the designer.
    /// </summary>
    public DairyComponentViewModelDesign()
    {
        // Initialize ManureStateTypes collection (filtering out obsolete values)
        ManureStateTypes = Enum.GetValues<ManureStateType>()
            .Where(x => !x.GetType().GetMember(x.ToString())[0]
                .GetCustomAttributes(typeof(ObsoleteAttribute), false).Any())
            .ToList();

        // Initialize HousingTypes collection (filtering out obsolete values)
        HousingTypes = Enum.GetValues<HousingType>()
            .Where(x => !x.GetType().GetMember(x.ToString())[0]
                .GetCustomAttributes(typeof(ObsoleteAttribute), false).Any())
            .ToList();

        // Create a sample DairyComponentDto with realistic values
        var dairyDto = new DairyComponentDto
        {
            Name = "Sample Dairy Herd",

            // Input parameters with typical dairy operation values
            TotalMilkingCows = 150,
            ReplacementRate = 28.5,
            CalvingIntervalMonths = 13,
            DryPeriodDays = 60,
            CalfMortalityRate = 4.5,
            FemaleCalfRatio = 48.0,

            // Lifecycle stage durations (used in steady-state calculations)
            CalfStageDurationDays = 120,      // 4 months
            HeiferStageDurationDays = 608,    // ~20 months
            LactationDurationDays = 305,      // ~10 months (standard lactation)

            // Steady-state populations (for Simple mode display)
            SteadyStateCalves = 33,
            SteadyStateHeifers = 50,
            SteadyStateLactating = 84,
            SteadyStateDry = 16,

            // Flow rates - these will be converted to steady-state populations for display
            // User will enter steady-state values, system calculates these flow rates
            CalvesEnteringPerYear = 100,      // Results in ~33 calves present (100 * 120/365)
            HeifersEnteringPerYear = 30,      // Results in ~50 heifers present (30 * 608/365)
            LactatingCowsEnteringPerYear = 100, // Results in ~84 lactating cows (100 * 305/365)
            DryCowsEnteringPerYear = 100,     // Results in ~16 dry cows (100 * 60/365)

            // Advanced mode - using multiple population groups for design-time visualization
            UseAdvancedPopulationMode = true,

            // Production defaults - typical Holstein values
            DefaultMilkProduction = 28.0,
            DefaultMilkFatContent = 3.7,
            DefaultMilkProteinContent = 3.1,

            // Manure handling system defaults for heifer phases
            HeiferPhase1ManureHandlingSystem = ManureStateType.LiquidNoCrust,
            HeiferPhase2ManureHandlingSystem = ManureStateType.LiquidNoCrust,

            // Calf phases - typically solid storage for individual pens
            CalfPhase1ManureHandlingSystem = ManureStateType.SolidStorage,
            CalfPhase2ManureHandlingSystem = ManureStateType.SolidStorage,

            // Lactating phases - typically liquid slurry systems
            LactatingPhase1ManureHandlingSystem = ManureStateType.LiquidNoCrust,
            LactatingPhase2ManureHandlingSystem = ManureStateType.LiquidNoCrust,
            LactatingPhase3ManureHandlingSystem = ManureStateType.LiquidNoCrust,
            LactatingPhase4ManureHandlingSystem = ManureStateType.LiquidNoCrust,

            // Dry phases - typically deep bedding
            DryPhase1ManureHandlingSystem = ManureStateType.DeepBedding,
            DryPhase2ManureHandlingSystem = ManureStateType.DeepBedding,

            // Housing type defaults for heifer phases
            HeiferPhase1HousingType = HousingType.FreeStallBarnSlurryScraping,
            HeiferPhase2HousingType = HousingType.FreeStallBarnSlurryScraping,

            // Calf phases - typically housed in barn with solid litter
            CalfPhase1HousingType = HousingType.HousedInBarnSolid,
            CalfPhase2HousingType = HousingType.HousedInBarnSolid,

            // Lactating phases - typically free stall barn
            LactatingPhase1HousingType = HousingType.FreeStallBarnSlurryScraping,
            LactatingPhase2HousingType = HousingType.FreeStallBarnSlurryScraping,
            LactatingPhase3HousingType = HousingType.FreeStallBarnSlurryScraping,
            LactatingPhase4HousingType = HousingType.FreeStallBarnSlurryScraping,

            // Dry phases - typically free stall with solid litter
            DryPhase1HousingType = HousingType.FreeStallBarnSolidLitter,
            DryPhase2HousingType = HousingType.FreeStallBarnSolidLitter
        };

        // Assign the sample DTO to the bound property
        base.SelectedDairyComponentDto = dairyDto;

        // Add sample population groups for design-time visualization
        // Calf groups - representing different pens or management groups
        dairyDto.CalfPopulationGroups.Add(new DairyPopulationGroup("Calf Pen 1", 15));
        dairyDto.CalfPopulationGroups.Add(new DairyPopulationGroup("Calf Pen 2", 12));
        dairyDto.CalfPopulationGroups.Add(new DairyPopulationGroup("Hutch Group", 8));

        // Heifer groups - representing different age cohorts
        dairyDto.HeiferPopulationGroups.Add(new DairyPopulationGroup("Young Heifers", 20));
        dairyDto.HeiferPopulationGroups.Add(new DairyPopulationGroup("Breeding Heifers", 18));
        dairyDto.HeiferPopulationGroups.Add(new DairyPopulationGroup("Pregnant Heifers", 15));

        // Lactating groups - representing different production levels
        dairyDto.LactatingPopulationGroups.Add(new DairyPopulationGroup("High Producers", 35));
        dairyDto.LactatingPopulationGroups.Add(new DairyPopulationGroup("Mid Producers", 30));
        dairyDto.LactatingPopulationGroups.Add(new DairyPopulationGroup("Fresh Cows", 12));
        dairyDto.LactatingPopulationGroups.Add(new DairyPopulationGroup("Late Lactation", 10));

        // Dry groups - representing different management approaches
        dairyDto.DryPopulationGroups.Add(new DairyPopulationGroup("Far-off Dry", 8));
        dairyDto.DryPopulationGroups.Add(new DairyPopulationGroup("Close-up Dry", 10));

        // Management practices are auto-initialized in the DairyComponentDto constructor
        // with sensible defaults for each stage. Add an extra custom practice to demonstrate
        // the dynamic add/remove capability in the designer.
        dairyDto.CalfManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 3: Transition Period",
            ManureStateType = ManureStateType.SolidStorage,
            HousingType = HousingType.HousedInBarnSolid,
        });

        dairyDto.LactatingManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 5: Extended Lactation",
            ManureStateType = ManureStateType.LiquidNoCrust,
            HousingType = HousingType.FreeStallBarnSlurryScraping,
        });

        // Set Calf stage as selected for design-time visualization of Step 2 and Step 3
        IsCalfSelected = true;
    }

    /// <summary>
    /// Constructor with dependency injection for runtime use.
    /// This constructor is never called in design mode but is required for the inheritance hierarchy.
    /// </summary>
    /// <param name="regionManager">Prism region manager for navigation</param>
    /// <param name="eventAggregator">Event aggregator for pub/sub messaging</param>
    /// <param name="storageService">Storage service for farm data</param>
    /// <param name="dairyComponentService">Service for dairy component operations</param>
    /// <param name="logger">Logger instance</param>
    public DairyComponentViewModelDesign(
        IRegionManager regionManager,
        IEventAggregator eventAggregator,
        IStorageService storageService,
        IDairyComponentService dairyComponentService,
        ILogger logger)
        : base(regionManager, eventAggregator, storageService, dairyComponentService, logger)
    {
    }
}
