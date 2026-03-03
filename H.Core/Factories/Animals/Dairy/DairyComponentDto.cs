using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Linq;
using H.Core.CustomAttributes;
using H.Core.Enumerations;
using H.Core.Factories.Animals;
using H.Core.Models.Animals.Dairy;

namespace H.Core.Factories.Animals.Dairy;

/// <summary>
/// A class used to validate input as it relates to a <see cref="H.Core.Models.Animals.Dairy.DairyComponent"/>. 
/// This class is used to validate input before any input is transferred to the <see cref="H.Core.Models.Animals.Dairy.DairyComponent"/>
/// 
/// This DTO contains the herd composition input parameters that drive the lifecycle-based dairy herd calculations.
/// </summary>
public class DairyComponentDto : AnimalComponentDto, IDairyComponentDto
{
    #region Fields

    // Herd Overview - Input Parameters
    private int _totalMilkingCows = 100;
    private double _replacementRate = 30.0;
    private int _calvingIntervalMonths = 14;
    private int _dryPeriodDays = 60;
    private double _calfMortalityRate = 5.0;
    private double _femaleCalfRatio = 50.0;
    
    // Lifecycle Stage Durations
    private int _calfStageDurationDays = 120;
    private int _heiferStageDurationDays = 608;
    private int _lactationDurationDays = 305;
    
    // Calculated Herd Composition - Read-only outputs
    private int _calculatedCalves;
    private int _calculatedHeifers;
    private int _calculatedLactating;
    private int _calculatedDry;
    
    // Herd Production Defaults - Used to populate management periods
    // NOTE: These are separate from ManagementPeriodDto properties
    // WHY DUPLICATE PROPERTIES?
    // - Component level: Herd-level defaults for simplified user input
    // - ManagementPeriod level: Actual values used in emissions calculations
    // - When auto-generating animal groups, these defaults populate the management period values
    // - Advanced users can then override individual management period values if needed
    private double _defaultMilkProduction = 25.0;
    private double _defaultMilkFatContent = 3.9;
    private double _defaultMilkProteinContent = 3.2;
    
    // Staggered Progression - Flow Rate Inputs
    private int _calvesEnteringPerYear = 100;
    private int _heifersEnteringPerYear = 30;
    private int _lactatingCowsEnteringPerYear = 30;
    private int _dryCowsEnteringPerYear = 100;
    
    // Manure Handling Systems for each phase
    private ManureStateType _heiferPhase1ManureHandlingSystem = ManureStateType.LiquidNoCrust;
    private ManureStateType _heiferPhase2ManureHandlingSystem = ManureStateType.LiquidNoCrust;
    
    // Calf stage phases (2 phases)
    private ManureStateType _calfPhase1ManureHandlingSystem = ManureStateType.SolidStorage;
    private ManureStateType _calfPhase2ManureHandlingSystem = ManureStateType.SolidStorage;
    
    // Lactating stage phases (4 phases)
    private ManureStateType _lactatingPhase1ManureHandlingSystem = ManureStateType.LiquidNoCrust;
    private ManureStateType _lactatingPhase2ManureHandlingSystem = ManureStateType.LiquidNoCrust;
    private ManureStateType _lactatingPhase3ManureHandlingSystem = ManureStateType.LiquidNoCrust;
    private ManureStateType _lactatingPhase4ManureHandlingSystem = ManureStateType.LiquidNoCrust;
    
    // Dry stage phases (2 phases)
    private ManureStateType _dryPhase1ManureHandlingSystem = ManureStateType.DeepBedding;
    private ManureStateType _dryPhase2ManureHandlingSystem = ManureStateType.DeepBedding;
    
    // Housing Types for each phase
    private HousingType _heiferPhase1HousingType = HousingType.FreeStallBarnSlurryScraping;
    private HousingType _heiferPhase2HousingType = HousingType.FreeStallBarnSlurryScraping;
    
    // Calf stage housing types (2 phases)
    private HousingType _calfPhase1HousingType = HousingType.HousedInBarnSolid;
    private HousingType _calfPhase2HousingType = HousingType.HousedInBarnSolid;
    
    // Lactating stage housing types (4 phases)
    private HousingType _lactatingPhase1HousingType = HousingType.FreeStallBarnSlurryScraping;
    private HousingType _lactatingPhase2HousingType = HousingType.FreeStallBarnSlurryScraping;
    private HousingType _lactatingPhase3HousingType = HousingType.FreeStallBarnSlurryScraping;
    private HousingType _lactatingPhase4HousingType = HousingType.FreeStallBarnSlurryScraping;
    
    // Dry stage housing types (2 phases)
    private HousingType _dryPhase1HousingType = HousingType.FreeStallBarnSolidLitter;
    private HousingType _dryPhase2HousingType = HousingType.FreeStallBarnSolidLitter;
    
    // Population Entry Mode - Simple vs Advanced
    private bool _useAdvancedPopulationMode;
    
    #endregion

    #region Constructors

    public DairyComponentDto()
    {
        this.PropertyChanged += OnPropertyChanged;

        // Initialize population group collections
        CalfPopulationGroups = new ObservableCollection<DairyPopulationGroup>();
        HeiferPopulationGroups = new ObservableCollection<DairyPopulationGroup>();
        LactatingPopulationGroups = new ObservableCollection<DairyPopulationGroup>();
        DryPopulationGroups = new ObservableCollection<DairyPopulationGroup>();

        // Initialize dynamic management practice collections
        CalfManagementPractices = new ObservableCollection<ManagementPeriodDto>();
        HeiferManagementPractices = new ObservableCollection<ManagementPeriodDto>();
        LactatingManagementPractices = new ObservableCollection<ManagementPeriodDto>();
        DryManagementPractices = new ObservableCollection<ManagementPeriodDto>();

        // Populate default management practices for each stage
        InitializeDefaultManagementPractices();

        // Subscribe to collection changes
        CalfPopulationGroups.CollectionChanged += OnCalfGroupsCollectionChanged;
        HeiferPopulationGroups.CollectionChanged += OnHeiferGroupsCollectionChanged;
        LactatingPopulationGroups.CollectionChanged += OnLactatingGroupsCollectionChanged;
        DryPopulationGroups.CollectionChanged += OnDryGroupsCollectionChanged;

        // Calculate initial values
        CalculateHerdComposition();
    }

    #endregion

    #region Properties - Input Parameters

    /// <summary>
    /// The total number of cows in the milking herd
    /// 
    /// (number of animals)
    /// </summary>
    public int TotalMilkingCows
    {
        get => _totalMilkingCows;
        set
        {
            if (SetProperty(ref _totalMilkingCows, value))
            {
                CalculateHerdComposition();
            }
        }
    }

    /// <summary>
    /// The percentage of the herd that is replaced annually
    /// 
    /// (%)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.Percentage)]
    public double ReplacementRate
    {
        get => _replacementRate;
        set
        {
            if (SetProperty(ref _replacementRate, value))
            {
                CalculateHerdComposition();
            }
        }
    }

    /// <summary>
    /// The average number of months between calvings
    /// 
    /// (months)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.Months)]
    public int CalvingIntervalMonths
    {
        get => _calvingIntervalMonths;
        set
        {
            if (SetProperty(ref _calvingIntervalMonths, value))
            {
                CalculateHerdComposition();
            }
        }
    }

    /// <summary>
    /// The number of days before calving that a cow is not milked (dry period)
    /// 
    /// (days)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.Days)]
    public int DryPeriodDays
    {
        get => _dryPeriodDays;
        set
        {
            if (SetProperty(ref _dryPeriodDays, value))
            {
                CalculateHerdComposition();
                RaisePropertyChanged(nameof(SteadyStateDry));
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }

    /// <summary>
    /// The percentage of calves that die before reaching 4 months of age
    /// 
    /// (%)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.Percentage)]
    public double CalfMortalityRate
    {
        get => _calfMortalityRate;
        set
        {
            if (SetProperty(ref _calfMortalityRate, value))
            {
                CalculateHerdComposition();
            }
        }
    }

    /// <summary>
    /// The expected percentage of female calves born
    /// 
    /// (%)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.Percentage)]
    public double FemaleCalfRatio
    {
        get => _femaleCalfRatio;
        set
        {
            if (SetProperty(ref _femaleCalfRatio, value))
            {
                CalculateHerdComposition();
            }
        }
    }
    
    /// <summary>
    /// Duration of the calf stage (birth to 4 months)
    /// This is used to calculate steady-state calf populations
    /// 
    /// Default: 120 days (4 months)
    /// 
    /// (days)
    /// </summary>
    public int CalfStageDurationDays
    {
        get => _calfStageDurationDays;
        set
        {
            if (SetProperty(ref _calfStageDurationDays, value))
            {
                RaisePropertyChanged(nameof(SteadyStateCalves));
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }
    
    /// <summary>
    /// Duration of the heifer stage (4 months to first calving at ~24 months)
    /// This is used to calculate steady-state heifer populations
    /// 
    /// Default: 608 days (~20 months)
    /// 
    /// (days)
    /// </summary>
    public int HeiferStageDurationDays
    {
        get => _heiferStageDurationDays;
        set
        {
            if (SetProperty(ref _heiferStageDurationDays, value))
            {
                RaisePropertyChanged(nameof(SteadyStateHeifers));
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }
    
    /// <summary>
    /// Duration of the lactation period (active milk production)
    /// This is used to calculate steady-state lactating cow populations
    /// 
    /// Default: 305 days (standard 10-month lactation)
    /// 
    /// (days)
    /// </summary>
    public int LactationDurationDays
    {
        get => _lactationDurationDays;
        set
        {
            if (SetProperty(ref _lactationDurationDays, value))
            {
                RaisePropertyChanged(nameof(SteadyStateLactating));
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }

    #endregion

    #region Properties - Calculated Outputs

    /// <summary>
    /// The calculated number of calves in the herd (birth to 4 months)
    /// 
    /// (number of animals)
    /// </summary>
    public int CalculatedCalves
    {
        get => _calculatedCalves;
        private set => SetProperty(ref _calculatedCalves, value);
    }

    /// <summary>
    /// The calculated number of heifers (replacement stock)
    /// 
    /// (number of animals)
    /// </summary>
    public int CalculatedHeifers
    {
        get => _calculatedHeifers;
        private set => SetProperty(ref _calculatedHeifers, value);
    }

    /// <summary>
    /// The calculated number of lactating cows (producing milk)
    /// 
    /// (number of animals)
    /// </summary>
    public int CalculatedLactating
    {
        get => _calculatedLactating;
        private set => SetProperty(ref _calculatedLactating, value);
    }

    /// <summary>
    /// The calculated number of dry cows (not producing milk, pre-calving)
    /// 
    /// (number of animals)
    /// </summary>
    public int CalculatedDry
    {
        get => _calculatedDry;
        private set => SetProperty(ref _calculatedDry, value);
    }

    #endregion

    #region Properties - Herd Production Defaults

    /// <summary>
    /// Default milk production for the herd (used when creating management periods)
    /// 
    /// ARCHITECTURE NOTE: This is a herd-level default that will be used to populate
    /// ManagementPeriod.MilkProduction when auto-generating animal groups.
    /// 
    /// TWO-LEVEL PATTERN:
    /// 1. User enters this value once (simplified input)
    /// 2. System uses it to populate all lactating cow management periods
    /// 3. Advanced users can override individual management period values later
    /// 
    /// (kg head?� day?�)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.KilogramPerHeadPerDay)]
    public double DefaultMilkProduction
    {
        get => _defaultMilkProduction;
        set => SetProperty(ref _defaultMilkProduction, value);
    }

    /// <summary>
    /// Default milk fat content for the herd (used when creating management periods)
    /// 
    /// ARCHITECTURE NOTE: This is a herd-level default that will be used to populate
    /// ManagementPeriod.MilkFatContent when auto-generating animal groups.
    /// 
    /// Typical values by breed:
    /// - Holstein: 3.5-3.9%
    /// - Jersey: 4.5-5.0%
    /// - Guernsey: 4.2-4.7%
    /// 
    /// (%)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.Percentage)]
    public double DefaultMilkFatContent
    {
        get => _defaultMilkFatContent;
        set => SetProperty(ref _defaultMilkFatContent, value);
    }

    /// <summary>
    /// Default milk protein content for the herd (used when creating management periods)
    /// 
    /// ARCHITECTURE NOTE: This is a herd-level default that will be used to populate
    /// ManagementPeriod.MilkProteinContentAsPercentage when auto-generating animal groups.
    /// 
    /// Typical values by breed:
    /// - Holstein: 3.0-3.2%
    /// - Jersey: 3.6-3.9%
    /// - Guernsey: 3.3-3.6%
    /// 
    /// (%)
    /// </summary>
    [Units(MetricUnitsOfMeasurement.Percentage)]
    public double DefaultMilkProteinContent
    {
        get => _defaultMilkProteinContent;
        set => SetProperty(ref _defaultMilkProteinContent, value);
    }
    
    #endregion
    
    #region Properties - Staggered Progression Flow Rates

    /// <summary>
    /// Number of calves entering the calf stage (birth) per year.
    /// This represents the continuous flow of animals being born into the dairy operation.
    /// 
    /// STAGGERED PROGRESSION MODEL:
    /// Instead of modeling all animals moving through stages synchronously, this approach
    /// models a steady flow of animals entering each stage. This better represents
    /// real dairy operations where calving occurs year-round.
    /// 
    /// CALCULATION NOTE:
    /// This value can be used to calculate steady-state populations:
    /// - Steady-state calves = (CalvesEnteringPerYear) � (Duration in calf stage / 365 days)
    /// - Example: 100 calves/year � (4 months / 12 months) = ~33 calves at any given time
    /// 
    /// (number of animals per year)
    /// </summary>
    public int CalvesEnteringPerYear
    {
        get => _calvesEnteringPerYear;
        set
        {
            if (SetProperty(ref _calvesEnteringPerYear, value))
            {
                RaisePropertyChanged(nameof(SteadyStateCalves));
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }
    
    /// <summary>
    /// Number of heifers entering the heifer stage (from calf to heifer transition) per year.
    /// This represents the continuous flow of animals moving from the calf stage into the replacement heifer population.
    /// 
    /// STAGGERED PROGRESSION MODEL:
    /// This value represents heifers that have survived the calf stage and are entering the replacement
    /// stock pool. In a steady-state operation, this equals the number of calves surviving to 4 months.
    /// 
    /// CALCULATION NOTE:
    /// - Steady-state heifers = (HeifersEnteringPerYear) � (Duration in heifer stage / 365 days)
    /// - Example: 30 heifers/year � (20 months / 12 months) = ~50 heifers at any given time
    /// 
    /// (number of animals per year)
    /// </summary>
    public int HeifersEnteringPerYear
    {
        get => _heifersEnteringPerYear;
        set
        {
            if (SetProperty(ref _heifersEnteringPerYear, value))
            {
                RaisePropertyChanged(nameof(SteadyStateHeifers));
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }
    
    /// <summary>
    /// Number of cows entering lactation (calving and starting milk production) per year.
    /// This represents the continuous flow of first-calf heifers and re-calving cows entering the lactating herd.
    /// 
    /// STAGGERED PROGRESSION MODEL:
    /// This includes both first-calf heifers completing their first pregnancy and mature cows
    /// calving again after their dry period. In steady-state, this equals the culling rate
    /// plus any herd expansion.
    /// 
    /// CALCULATION NOTE:
    /// - Steady-state lactating = (LactatingCowsEnteringPerYear) � (Lactation period / 365 days)
    /// - Example: 100 cows/year � (305 days / 365 days) = ~84 lactating cows at any given time
    /// 
    /// (number of animals per year)
    /// </summary>
    public int LactatingCowsEnteringPerYear
    {
        get => _lactatingCowsEnteringPerYear;
        set
        {
            if (SetProperty(ref _lactatingCowsEnteringPerYear, value))
            {
                RaisePropertyChanged(nameof(SteadyStateLactating));
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }
    
    /// <summary>
    /// Number of cows entering the dry period (stopping milk production before calving) per year.
    /// This represents the continuous flow of lactating cows being dried off in preparation for their next calving.
    /// 
    /// STAGGERED PROGRESSION MODEL:
    /// In steady-state, this should equal the number of cows entering lactation since every cow
    /// goes through a dry period before each lactation. The dry period is a rest phase that allows
    /// the udder to regenerate.
    /// 
    /// CALCULATION NOTE:
    /// - Steady-state dry cows = (DryCowsEnteringPerYear) � (Dry period / 365 days)
    /// - Example: 100 cows/year � (60 days / 365 days) = ~16 dry cows at any given time
    /// 
    /// (number of animals per year)
    /// </summary>
    public int DryCowsEnteringPerYear
    {
        get => _dryCowsEnteringPerYear;
        set
        {
            if (SetProperty(ref _dryCowsEnteringPerYear, value))
            {
                RaisePropertyChanged(nameof(SteadyStateDry));
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }

    #endregion
    
    #region Properties - Staggered Progression Steady-State Calculations
    
    /// <summary>
    /// Steady-state population of calves in the herd.
    /// This represents how many calves are present at any given moment in a continuous-flow operation.
    /// 
    /// USER INPUT: User enters the number of calves present
    /// CALCULATION: System calculates CalvesEnteringPerYear = SteadyStateCalves / (CalfStageDurationDays / 365)
    /// Example: 33 calves / (120 days / 365) = 33 / 0.329 = 100 calves/year entering
    /// 
    /// (number of animals)
    /// </summary>
    public int SteadyStateCalves
    {
        get
        {
            return (int)Math.Round(CalvesEnteringPerYear * (CalfStageDurationDays / 365.0));
        }
        set
        {
            // Reverse calculation: Calculate flow rate from steady-state population
            if (CalfStageDurationDays > 0)
            {
                var calculatedFlowRate = (int)Math.Round(value / (CalfStageDurationDays / 365.0));
                CalvesEnteringPerYear = calculatedFlowRate;
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }
    
    /// <summary>
    /// Steady-state population of heifers in the herd.
    /// This represents how many replacement heifers are present at any given moment.
    /// 
    /// USER INPUT: User enters the number of heifers present
    /// CALCULATION: System calculates HeifersEnteringPerYear = SteadyStateHeifers / (HeiferStageDurationDays / 365)
    /// Example: 50 heifers / (608 days / 365) = 50 / 1.666 = 30 heifers/year entering
    /// 
    /// (number of animals)
    /// </summary>
    public int SteadyStateHeifers
    {
        get
        {
            return (int)Math.Round(HeifersEnteringPerYear * (HeiferStageDurationDays / 365.0));
        }
        set
        {
            // Reverse calculation: Calculate flow rate from steady-state population
            if (HeiferStageDurationDays > 0)
            {
                var calculatedFlowRate = (int)Math.Round(value / (HeiferStageDurationDays / 365.0));
                HeifersEnteringPerYear = calculatedFlowRate;
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }
    
    /// <summary>
    /// Steady-state population of lactating cows in the herd.
    /// This represents how many cows are producing milk at any given moment.
    /// 
    /// USER INPUT: User enters the number of lactating cows present
    /// CALCULATION: System calculates LactatingCowsEnteringPerYear = SteadyStateLactating / (LactationDurationDays / 365)
    /// Example: 84 cows / (305 days / 365) = 84 / 0.836 = 100 cows/year entering lactation
    /// 
    /// (number of animals)
    /// </summary>
    public int SteadyStateLactating
    {
        get
        {
            return (int)Math.Round(LactatingCowsEnteringPerYear * (LactationDurationDays / 365.0));
        }
        set
        {
            // Reverse calculation: Calculate flow rate from steady-state population
            if (LactationDurationDays > 0)
            {
                var calculatedFlowRate = (int)Math.Round(value / (LactationDurationDays / 365.0));
                LactatingCowsEnteringPerYear = calculatedFlowRate;
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }
    
    /// <summary>
    /// Steady-state population of dry cows in the herd.
    /// This represents how many non-lactating cows (preparing for calving) are present at any given moment.
    /// 
    /// USER INPUT: User enters the number of dry cows present
    /// CALCULATION: System calculates DryCowsEnteringPerYear = SteadyStateDry / (DryPeriodDays / 365)
    /// Example: 16 cows / (60 days / 365) = 16 / 0.164 = 100 cows/year entering dry period
    /// 
    /// NOTE: Uses the user-defined DryPeriodDays from Step 1
    /// 
    /// (number of animals)
    /// </summary>
    public int SteadyStateDry
    {
        get
        {
            return (int)Math.Round(DryCowsEnteringPerYear * (DryPeriodDays / 365.0));
        }
        set
        {
            // Reverse calculation: Calculate flow rate from steady-state population
            if (DryPeriodDays > 0)
            {
                var calculatedFlowRate = (int)Math.Round(value / (DryPeriodDays / 365.0));
                DryCowsEnteringPerYear = calculatedFlowRate;
                RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
            }
        }
    }
    
    /// <summary>
    /// Total steady-state herd size across all lifecycle stages.
    /// This is the sum of all animals present at any given moment in the operation.
    /// 
    /// (number of animals)
    /// </summary>
    public int TotalSteadyStateHerdSize
    {
        get
        {
            return SteadyStateCalves + SteadyStateHeifers + SteadyStateLactating + SteadyStateDry;
        }
    }
    
    #endregion
    
    #region Properties - Population Entry Mode and Groups
    
    /// <summary>
    /// Determines whether the user is in Simple mode (single entry) or Advanced mode (multiple groups).
    /// Simple mode: User enters a single population value per stage
    /// Advanced mode: User can define multiple named groups with individual populations
    /// </summary>
    public bool UseAdvancedPopulationMode
    {
        get => _useAdvancedPopulationMode;
        set
        {
            if (SetProperty(ref _useAdvancedPopulationMode, value))
            {
                // When switching modes, sync the data
                if (value)
                {
                    // Switching TO Advanced: Create single group from simple values
                    SyncSimpleToAdvancedMode();
                }
                else
                {
                    // Switching TO Simple: Calculate totals from groups
                    SyncAdvancedToSimpleMode();
                }
            }
        }
    }
    
    /// <summary>
    /// Collection of population groups for the calf stage (Advanced mode)
    /// </summary>
    public ObservableCollection<DairyPopulationGroup> CalfPopulationGroups { get; private set; }
    
    /// <summary>
    /// Collection of population groups for the heifer stage (Advanced mode)
    /// </summary>
    public ObservableCollection<DairyPopulationGroup> HeiferPopulationGroups { get; private set; }
    
    /// <summary>
    /// Collection of population groups for the lactating stage (Advanced mode)
    /// </summary>
    public ObservableCollection<DairyPopulationGroup> LactatingPopulationGroups { get; private set; }
    
    /// <summary>
    /// Collection of population groups for the dry stage (Advanced mode)
    /// </summary>
    public ObservableCollection<DairyPopulationGroup> DryPopulationGroups { get; private set; }
    
    /// <summary>
    /// Total calf population from all groups (used in Advanced mode)
    /// </summary>
    public int TotalCalfPopulation => CalfPopulationGroups?.Sum(g => g.NumberOfAnimals) ?? 0;
    
    /// <summary>
    /// Total heifer population from all groups (used in Advanced mode)
    /// </summary>
    public int TotalHeiferPopulation => HeiferPopulationGroups?.Sum(g => g.NumberOfAnimals) ?? 0;
    
    /// <summary>
    /// Total lactating population from all groups (used in Advanced mode)
    /// </summary>
    public int TotalLactatingPopulation => LactatingPopulationGroups?.Sum(g => g.NumberOfAnimals) ?? 0;
    
    /// <summary>
    /// Total dry population from all groups (used in Advanced mode)
    /// </summary>
    public int TotalDryPopulation => DryPopulationGroups?.Sum(g => g.NumberOfAnimals) ?? 0;

    #endregion

    #region Properties - Dynamic Management Practices

    /// <summary>
    /// Dynamic collection of management practices for the calf stage.
    /// Users can add/remove practices to model their specific operation.
    /// </summary>
    public ObservableCollection<ManagementPeriodDto> CalfManagementPractices { get; private set; }

    /// <summary>
    /// Dynamic collection of management practices for the heifer stage.
    /// Users can add/remove practices to model their specific operation.
    /// </summary>
    public ObservableCollection<ManagementPeriodDto> HeiferManagementPractices { get; private set; }

    /// <summary>
    /// Dynamic collection of management practices for the lactating stage.
    /// Users can add/remove practices to model their specific operation.
    /// </summary>
    public ObservableCollection<ManagementPeriodDto> LactatingManagementPractices { get; private set; }

    /// <summary>
    /// Dynamic collection of management practices for the dry stage.
    /// Users can add/remove practices to model their specific operation.
    /// </summary>
    public ObservableCollection<ManagementPeriodDto> DryManagementPractices { get; private set; }

    #endregion

    #region Properties - Manure Handling Systems
    
    /// <summary>
    /// Manure handling system for heifer Phase 1 (Growing Phase: 4-12 months)
    /// This determines how manure is stored and handled during the growing phase
    /// </summary>
    public ManureStateType HeiferPhase1ManureHandlingSystem
    {
        get => _heiferPhase1ManureHandlingSystem;
        set => SetProperty(ref _heiferPhase1ManureHandlingSystem, value);
    }
    
    /// <summary>
    /// Manure handling system for heifer Phase 2 (Breeding Phase: 12-24 months)
    /// This determines how manure is stored and handled during the breeding phase
    /// </summary>
    public ManureStateType HeiferPhase2ManureHandlingSystem
    {
        get => _heiferPhase2ManureHandlingSystem;
        set => SetProperty(ref _heiferPhase2ManureHandlingSystem, value);
    }
    
    /// <summary>
    /// Manure handling system for calf Phase 1 (Milk-Fed Period: Days 1-90)
    /// This determines how manure is stored and handled during the milk-fed period
    /// </summary>
    public ManureStateType CalfPhase1ManureHandlingSystem
    {
        get => _calfPhase1ManureHandlingSystem;
        set => SetProperty(ref _calfPhase1ManureHandlingSystem, value);
    }
    
    /// <summary>
    /// Manure handling system for calf Phase 2 (Weaning Period: Days 91-120)
    /// This determines how manure is stored and handled during the weaning period
    /// </summary>
    public ManureStateType CalfPhase2ManureHandlingSystem
    {
        get => _calfPhase2ManureHandlingSystem;
        set => SetProperty(ref _calfPhase2ManureHandlingSystem, value);
    }
    
    /// <summary>
    /// Manure handling system for lactating Phase 1 (Early Lactation: Days 1-150)
    /// This determines how manure is stored and handled during early lactation
    /// </summary>
    public ManureStateType LactatingPhase1ManureHandlingSystem
    {
        get => _lactatingPhase1ManureHandlingSystem;
        set => SetProperty(ref _lactatingPhase1ManureHandlingSystem, value);
    }
    
    /// <summary>
    /// Manure handling system for lactating Phase 2 (Mid Lactation: Days 151-240)
    /// This determines how manure is stored and handled during mid lactation
    /// </summary>
    public ManureStateType LactatingPhase2ManureHandlingSystem
    {
        get => _lactatingPhase2ManureHandlingSystem;
        set => SetProperty(ref _lactatingPhase2ManureHandlingSystem, value);
    }
    
    /// <summary>
    /// Manure handling system for lactating Phase 3 (Late Lactation: Days 241-305)
    /// This determines how manure is stored and handled during late lactation
    /// </summary>
    public ManureStateType LactatingPhase3ManureHandlingSystem
    {
        get => _lactatingPhase3ManureHandlingSystem;
        set => SetProperty(ref _lactatingPhase3ManureHandlingSystem, value);
    }
    
    /// <summary>
    /// Manure handling system for lactating Phase 4 (End Lactation: Days 306-365)
    /// This determines how manure is stored and handled during end lactation
    /// </summary>
    public ManureStateType LactatingPhase4ManureHandlingSystem
    {
        get => _lactatingPhase4ManureHandlingSystem;
        set => SetProperty(ref _lactatingPhase4ManureHandlingSystem, value);
    }
    
    /// <summary>
    /// Manure handling system for dry Phase 1 (Far-off Dry: Days 1-45)
    /// This determines how manure is stored and handled during the far-off dry period
    /// </summary>
    public ManureStateType DryPhase1ManureHandlingSystem
    {
        get => _dryPhase1ManureHandlingSystem;
        set => SetProperty(ref _dryPhase1ManureHandlingSystem, value);
    }
    
    /// <summary>
    /// Manure handling system for dry Phase 2 (Close-up Period: Days 46-60)
    /// This determines how manure is stored and handled during the close-up period
    /// </summary>
    public ManureStateType DryPhase2ManureHandlingSystem
    {
        get => _dryPhase2ManureHandlingSystem;
        set => SetProperty(ref _dryPhase2ManureHandlingSystem, value);
    }

    #endregion
    
    #region Properties - Housing Types
    
    /// <summary>
    /// Housing type for heifer Phase 1 (Growing Phase: 4-12 months)
    /// This determines the type of housing facility used during the growing phase
    /// </summary>
    public HousingType HeiferPhase1HousingType
    {
        get => _heiferPhase1HousingType;
        set => SetProperty(ref _heiferPhase1HousingType, value);
    }
    
    /// <summary>
    /// Housing type for heifer Phase 2 (Breeding Phase: 12-24 months)
    /// This determines the type of housing facility used during the breeding phase
    /// </summary>
    public HousingType HeiferPhase2HousingType
    {
        get => _heiferPhase2HousingType;
        set => SetProperty(ref _heiferPhase2HousingType, value);
    }
    
    /// <summary>
    /// Housing type for calf Phase 1 (Milk-Fed Period: Days 1-90)
    /// This determines the type of housing facility used during the milk-fed period
    /// </summary>
    public HousingType CalfPhase1HousingType
    {
        get => _calfPhase1HousingType;
        set => SetProperty(ref _calfPhase1HousingType, value);
    }
    
    /// <summary>
    /// Housing type for calf Phase 2 (Weaning Period: Days 91-120)
    /// This determines the type of housing facility used during the weaning period
    /// </summary>
    public HousingType CalfPhase2HousingType
    {
        get => _calfPhase2HousingType;
        set => SetProperty(ref _calfPhase2HousingType, value);
    }
    
    /// <summary>
    /// Housing type for lactating Phase 1 (Early Lactation: Days 1-150)
    /// This determines the type of housing facility used during early lactation
    /// </summary>
    public HousingType LactatingPhase1HousingType
    {
        get => _lactatingPhase1HousingType;
        set => SetProperty(ref _lactatingPhase1HousingType, value);
    }
    
    /// <summary>
    /// Housing type for lactating Phase 2 (Mid Lactation: Days 151-240)
    /// This determines the type of housing facility used during mid lactation
    /// </summary>
    public HousingType LactatingPhase2HousingType
    {
        get => _lactatingPhase2HousingType;
        set => SetProperty(ref _lactatingPhase2HousingType, value);
    }
    
    /// <summary>
    /// Housing type for lactating Phase 3 (Late Lactation: Days 241-305)
    /// This determines the type of housing facility used during late lactation
    /// </summary>
    public HousingType LactatingPhase3HousingType
    {
        get => _lactatingPhase3HousingType;
        set => SetProperty(ref _lactatingPhase3HousingType, value);
    }
    
    /// <summary>
    /// Housing type for lactating Phase 4 (End Lactation: Days 306-365)
    /// This determines the type of housing facility used during end lactation
    /// </summary>
    public HousingType LactatingPhase4HousingType
    {
        get => _lactatingPhase4HousingType;
        set => SetProperty(ref _lactatingPhase4HousingType, value);
    }
    
    /// <summary>
    /// Housing type for dry Phase 1 (Far-off Dry: Days 1-45)
    /// This determines the type of housing facility used during the far-off dry period
    /// </summary>
    public HousingType DryPhase1HousingType
    {
        get => _dryPhase1HousingType;
        set => SetProperty(ref _dryPhase1HousingType, value);
    }
    
    /// <summary>
    /// Housing type for dry Phase 2 (Close-up Period: Days 46-60)
    /// This determines the type of housing facility used during the close-up period
    /// </summary>
    public HousingType DryPhase2HousingType
    {
        get => _dryPhase2HousingType;
        set => SetProperty(ref _dryPhase2HousingType, value);
    }

    #endregion
    
    #region Private Methods

    /// <summary>
    /// Initializes default management practices for each lifecycle stage.
    /// These defaults match the legacy fixed-phase properties and provide
    /// a starting point that users can customize by adding or removing practices.
    /// </summary>
    private void InitializeDefaultManagementPractices()
    {
        // Calf stage defaults (2 phases)
        CalfManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 1: Milk-Fed Period",
            ManureStateType = ManureStateType.NotSelected,
            HousingType = HousingType.HousedInBarnSolid,
        });
        CalfManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 2: Weaning Period",
            ManureStateType = ManureStateType.NotSelected,
            HousingType = HousingType.HousedInBarnSolid,
        });

        // Heifer stage defaults (2 phases)
        HeiferManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 1: Growing Phase",
            ManureStateType = ManureStateType.NotSelected,
            HousingType = HousingType.FreeStallBarnSlurryScraping,
        });
        HeiferManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 2: Breeding Phase",
            ManureStateType = ManureStateType.NotSelected,
            HousingType = HousingType.FreeStallBarnSlurryScraping,
        });

        // Lactating stage defaults (4 phases)
        LactatingManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 1: Early Lactation",
            ManureStateType = ManureStateType.NotSelected,
            HousingType = HousingType.FreeStallBarnSlurryScraping,
        });
        LactatingManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 2: Mid Lactation",
            ManureStateType = ManureStateType.NotSelected,
            HousingType = HousingType.FreeStallBarnSlurryScraping,
        });
        LactatingManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 3: Late Lactation",
            ManureStateType = ManureStateType.NotSelected,
            HousingType = HousingType.FreeStallBarnSlurryScraping,
        });
        LactatingManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 4: End Lactation",
            ManureStateType = ManureStateType.NotSelected,
            HousingType = HousingType.FreeStallBarnSlurryScraping,
        });

        // Dry stage defaults (2 phases)
        DryManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 1: Far-off Dry",
            ManureStateType = ManureStateType.NotSelected,
            HousingType = HousingType.FreeStallBarnSolidLitter,
        });
        DryManagementPractices.Add(new ManagementPeriodDto
        {
            Name = "Phase 2: Close-up Period",
            ManureStateType = ManureStateType.NotSelected,
            HousingType = HousingType.FreeStallBarnSolidLitter,
        });
    }

    /// <summary>
    /// Calculates the herd composition based on the input parameters
    /// </summary>
    private void CalculateHerdComposition()
    {
        // Calculate based on replacement rate and herd size
        var totalReplacements = (int)Math.Ceiling(TotalMilkingCows * (ReplacementRate / 100.0));
        
        // Calculate dry cows based on dry period
        // Dry period fraction = days dry / days in year
        var dryPeriodFraction = DryPeriodDays / 365.0;
        CalculatedDry = (int)Math.Ceiling(TotalMilkingCows * dryPeriodFraction);
        
        // Lactating = Total milking cows - Dry cows
        CalculatedLactating = TotalMilkingCows - CalculatedDry;
        
        // Heifers = replacement stock
        CalculatedHeifers = totalReplacements;
        
        // Calculate calves (accounting for mortality and female ratio)
        // Assume one calf per cow per year
        var expectedCalves = TotalMilkingCows;
        var survivingCalves = expectedCalves * (1 - CalfMortalityRate / 100.0);
        var femaleCalves = survivingCalves * (FemaleCalfRatio / 100.0);
        CalculatedCalves = (int)Math.Ceiling(femaleCalves);
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validates that the total milking cows is a positive number
    /// </summary>
    private void ValidateTotalMilkingCows()
    {
        var key = nameof(TotalMilkingCows);
        
        if (TotalMilkingCows <= 0)
        {
            AddError(key, "Total milking cows must be greater than zero");
        }
        else if (TotalMilkingCows > 10000)
        {
            AddError(key, "Total milking cows cannot exceed 10,000");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Validates that the replacement rate is within a reasonable range
    /// </summary>
    private void ValidateReplacementRate()
    {
        var key = nameof(ReplacementRate);
        
        if (ReplacementRate < 0)
        {
            AddError(key, "Replacement rate cannot be negative");
        }
        else if (ReplacementRate > 100)
        {
            AddError(key, "Replacement rate cannot exceed 100%");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Validates that the calving interval is within a reasonable range
    /// </summary>
    private void ValidateCalvingIntervalMonths()
    {
        var key = nameof(CalvingIntervalMonths);
        
        if (CalvingIntervalMonths < 10)
        {
            AddError(key, "Calving interval must be at least 10 months");
        }
        else if (CalvingIntervalMonths > 24)
        {
            AddError(key, "Calving interval cannot exceed 24 months");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Validates that the dry period is within a reasonable range
    /// </summary>
    private void ValidateDryPeriodDays()
    {
        var key = nameof(DryPeriodDays);
        
        if (DryPeriodDays < 0)
        {
            AddError(key, "Dry period cannot be negative");
        }
        else if (DryPeriodDays > 120)
        {
            AddError(key, "Dry period cannot exceed 120 days");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Validates that the calf mortality rate is within a reasonable range
    /// </summary>
    private void ValidateCalfMortalityRate()
    {
        var key = nameof(CalfMortalityRate);
        
        if (CalfMortalityRate < 0)
        {
            AddError(key, "Calf mortality rate cannot be negative");
        }
        else if (CalfMortalityRate > 50)
        {
            AddError(key, "Calf mortality rate cannot exceed 50%");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Validates that the female calf ratio is within a reasonable range
    /// </summary>
    private void ValidateFemaleCalfRatio()
    {
        var key = nameof(FemaleCalfRatio);
        
        if (FemaleCalfRatio < 0)
        {
            AddError(key, "Female calf ratio cannot be negative");
        }
        else if (FemaleCalfRatio > 100)
        {
            AddError(key, "Female calf ratio cannot exceed 100%");
        }
        else
        {
            RemoveError(key);
        }
    }
    
    /// <summary>
    /// Validates that the calf stage duration is within a reasonable range
    /// </summary>
    private void ValidateCalfStageDurationDays()
    {
        var key = nameof(CalfStageDurationDays);
        
        if (CalfStageDurationDays < 30)
        {
            AddError(key, "Calf stage duration must be at least 30 days");
        }
        else if (CalfStageDurationDays > 365)
        {
            AddError(key, "Calf stage duration cannot exceed 365 days");
        }
        else
        {
            RemoveError(key);
        }
    }
    
    /// <summary>
    /// Validates that the heifer stage duration is within a reasonable range
    /// </summary>
    private void ValidateHeiferStageDurationDays()
    {
        var key = nameof(HeiferStageDurationDays);
        
        if (HeiferStageDurationDays < 365)
        {
            AddError(key, "Heifer stage duration must be at least 365 days (1 year)");
        }
        else if (HeiferStageDurationDays > 1095)
        {
            AddError(key, "Heifer stage duration cannot exceed 1095 days (3 years)");
        }
        else
        {
            RemoveError(key);
        }
    }
    
    /// <summary>
    /// Validates that the lactation duration is within a reasonable range
    /// </summary>
    private void ValidateLactationDurationDays()
    {
        var key = nameof(LactationDurationDays);
        
        if (LactationDurationDays < 200)
        {
            AddError(key, "Lactation duration must be at least 200 days");
        }
        else if (LactationDurationDays > 400)
        {
            AddError(key, "Lactation duration cannot exceed 400 days");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Validates that the default milk production is within a reasonable range
    /// </summary>
    private void ValidateDefaultMilkProduction()
    {
        var key = nameof(DefaultMilkProduction);
        
        if (DefaultMilkProduction < 0)
        {
            AddError(key, "Milk production cannot be negative");
        }
        else if (DefaultMilkProduction > 100)
        {
            AddError(key, "Milk production cannot exceed 100 kg/day");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Validates that the default milk fat content is within a reasonable range
    /// </summary>
    private void ValidateDefaultMilkFatContent()
    {
        var key = nameof(DefaultMilkFatContent);
        
        if (DefaultMilkFatContent < 0)
        {
            AddError(key, "Milk fat content cannot be negative");
        }
        else if (DefaultMilkFatContent > 10)
        {
            AddError(key, "Milk fat content cannot exceed 10%");
        }
        else
        {
            RemoveError(key);
        }
    }

    /// <summary>
    /// Validates that the default milk protein content is within a reasonable range
    /// </summary>
    private void ValidateDefaultMilkProteinContent()
    {
        var key = nameof(DefaultMilkProteinContent);
        
        if (DefaultMilkProteinContent < 0)
        {
            AddError(key, "Milk protein content cannot be negative");
        }
        else if (DefaultMilkProteinContent > 10)
        {
            AddError(key, "Milk protein content cannot exceed 10%");
        }
        else
        {
            RemoveError(key);
        }
    }
    
    /// <summary>
    /// Validates that the calves entering per year is within a reasonable range
    /// </summary>
    private void ValidateCalvesEnteringPerYear()
    {
        var key = nameof(CalvesEnteringPerYear);
        
        if (CalvesEnteringPerYear < 0)
        {
            AddError(key, "Calves entering per year cannot be negative");
        }
        else if (CalvesEnteringPerYear > 10000)
        {
            AddError(key, "Calves entering per year cannot exceed 10,000");
        }
        else
        {
            RemoveError(key);
        }
    }
    
    /// <summary>
    /// Validates that the heifers entering per year is within a reasonable range
    /// </summary>
    private void ValidateHeifersEnteringPerYear()
    {
        var key = nameof(HeifersEnteringPerYear);
        
        if (HeifersEnteringPerYear < 0)
        {
            AddError(key, "Heifers entering per year cannot be negative");
        }
        else if (HeifersEnteringPerYear > 10000)
        {
            AddError(key, "Heifers entering per year cannot exceed 10,000");
        }
        else
        {
            RemoveError(key);
        }
    }
    
    /// <summary>
    /// Validates that the lactating cows entering per year is within a reasonable range
    /// </summary>
    private void ValidateLactatingCowsEnteringPerYear()
    {
        var key = nameof(LactatingCowsEnteringPerYear);
        
        if (LactatingCowsEnteringPerYear < 0)
        {
            AddError(key, "Lactating cows entering per year cannot be negative");
        }
        else if (LactatingCowsEnteringPerYear > 10000)
        {
            AddError(key, "Lactating cows entering per year cannot exceed 10,000");
        }
        else
        {
            RemoveError(key);
        }
    }
    
    /// <summary>
    /// Validates that the dry cows entering per year is within a reasonable range
    /// </summary>
    private void ValidateDryCowsEnteringPerYear()
    {
        var key = nameof(DryCowsEnteringPerYear);
        
        if (DryCowsEnteringPerYear < 0)
        {
            AddError(key, "Dry cows entering per year cannot be negative");
        }
        else if (DryCowsEnteringPerYear > 10000)
        {
            AddError(key, "Dry cows entering per year cannot exceed 10,000");
        }
        else
        {
            RemoveError(key);
        }
    }

    #endregion

    #region Event Handlers

    private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == null)
            return;

        // Validate properties when they change
        switch (e.PropertyName)
        {
            case nameof(TotalMilkingCows):
                ValidateTotalMilkingCows();
                break;
                
            case nameof(ReplacementRate):
                ValidateReplacementRate();
                break;
                
            case nameof(CalvingIntervalMonths):
                ValidateCalvingIntervalMonths();
                break;
                
            case nameof(DryPeriodDays):
                ValidateDryPeriodDays();
                break;
                
            case nameof(CalfMortalityRate):
                ValidateCalfMortalityRate();
                break;
                
            case nameof(FemaleCalfRatio):
                ValidateFemaleCalfRatio();
                break;
                
            case nameof(CalfStageDurationDays):
                ValidateCalfStageDurationDays();
                break;
                
            case nameof(HeiferStageDurationDays):
                ValidateHeiferStageDurationDays();
                break;
                
            case nameof(LactationDurationDays):
                ValidateLactationDurationDays();
                break;
                
            case nameof(DefaultMilkProduction):
                ValidateDefaultMilkProduction();
                break;
                
            case nameof(DefaultMilkFatContent):
                ValidateDefaultMilkFatContent();
                break;
                
            case nameof(DefaultMilkProteinContent):
                ValidateDefaultMilkProteinContent();
                break;
                
            case nameof(CalvesEnteringPerYear):
                ValidateCalvesEnteringPerYear();
                break;
                
            case nameof(HeifersEnteringPerYear):
                ValidateHeifersEnteringPerYear();
                break;
                
            case nameof(LactatingCowsEnteringPerYear):
                ValidateLactatingCowsEnteringPerYear();
                break;
                
            case nameof(DryCowsEnteringPerYear):
                ValidateDryCowsEnteringPerYear();
                break;
        }
    }
    
    /// <summary>
    /// Called when population groups change in Advanced mode.
    /// Updates the steady-state populations based on group totals.
    /// </summary>
    private void OnPopulationGroupsChanged()
    {
        if (UseAdvancedPopulationMode)
        {
            // Update steady-state values from group totals
            SteadyStateCalves = TotalCalfPopulation;
            SteadyStateHeifers = TotalHeiferPopulation;
            SteadyStateLactating = TotalLactatingPopulation;
            SteadyStateDry = TotalDryPopulation;
        }
        
        // Notify UI of changes
        RaisePropertyChanged(nameof(TotalCalfPopulation));
        RaisePropertyChanged(nameof(TotalHeiferPopulation));
        RaisePropertyChanged(nameof(TotalLactatingPopulation));
        RaisePropertyChanged(nameof(TotalDryPopulation));
        RaisePropertyChanged(nameof(TotalSteadyStateHerdSize));
    }
    
    /// <summary>
    /// Handles collection changes for calf groups
    /// </summary>
    private void OnCalfGroupsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Unsubscribe from removed items
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is DairyPopulationGroup group)
                {
                    group.PropertyChanged -= OnGroupPropertyChanged;
                }
            }
        }
        
        // Subscribe to new items
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is DairyPopulationGroup group)
                {
                    group.PropertyChanged += OnGroupPropertyChanged;
                }
            }
        }
        
        OnPopulationGroupsChanged();
    }
    
    /// <summary>
    /// Handles collection changes for heifer groups
    /// </summary>
    private void OnHeiferGroupsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Unsubscribe from removed items
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is DairyPopulationGroup group)
                {
                    group.PropertyChanged -= OnGroupPropertyChanged;
                }
            }
        }
        
        // Subscribe to new items
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is DairyPopulationGroup group)
                {
                    group.PropertyChanged += OnGroupPropertyChanged;
                }
            }
        }
        
        OnPopulationGroupsChanged();
    }
    
    /// <summary>
    /// Handles collection changes for lactating groups
    /// </summary>
    private void OnLactatingGroupsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Unsubscribe from removed items
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is DairyPopulationGroup group)
                {
                    group.PropertyChanged -= OnGroupPropertyChanged;
                }
            }
        }
        
        // Subscribe to new items
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is DairyPopulationGroup group)
                {
                    group.PropertyChanged += OnGroupPropertyChanged;
                }
            }
        }
        
        OnPopulationGroupsChanged();
    }
    
    /// <summary>
    /// Handles collection changes for dry groups
    /// </summary>
    private void OnDryGroupsCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        // Unsubscribe from removed items
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is DairyPopulationGroup group)
                {
                    group.PropertyChanged -= OnGroupPropertyChanged;
                }
            }
        }
        
        // Subscribe to new items
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is DairyPopulationGroup group)
                {
                    group.PropertyChanged += OnGroupPropertyChanged;
                }
            }
        }
        
        OnPopulationGroupsChanged();
    }
    
    /// <summary>
    /// Handles property changes on individual population groups
    /// </summary>
    private void OnGroupPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // When a group's NumberOfAnimals changes, recalculate totals
        if (e.PropertyName == nameof(DairyPopulationGroup.NumberOfAnimals))
        {
            OnPopulationGroupsChanged();
        }
    }
    
    /// <summary>
    /// Sync from Simple mode to Advanced mode.
    /// Creates a single group for each stage with the current simple values.
    /// </summary>
    private void SyncSimpleToAdvancedMode()
    {
        // Clear existing groups
        CalfPopulationGroups.Clear();
        HeiferPopulationGroups.Clear();
        LactatingPopulationGroups.Clear();
        DryPopulationGroups.Clear();
        
        // Create default groups with current values
        if (SteadyStateCalves > 0)
            CalfPopulationGroups.Add(new DairyPopulationGroup("Group 1", SteadyStateCalves));
        
        if (SteadyStateHeifers > 0)
            HeiferPopulationGroups.Add(new DairyPopulationGroup("Group 1", SteadyStateHeifers));
        
        if (SteadyStateLactating > 0)
            LactatingPopulationGroups.Add(new DairyPopulationGroup("Group 1", SteadyStateLactating));
        
        if (SteadyStateDry > 0)
            DryPopulationGroups.Add(new DairyPopulationGroup("Group 1", SteadyStateDry));
    }
    
    /// <summary>
    /// Sync from Advanced mode to Simple mode.
    /// Calculates totals from all groups and sets the simple values.
    /// </summary>
    private void SyncAdvancedToSimpleMode()
    {
        // Update steady-state values from group totals
        if (TotalCalfPopulation > 0)
            SteadyStateCalves = TotalCalfPopulation;
        
        if (TotalHeiferPopulation > 0)
            SteadyStateHeifers = TotalHeiferPopulation;
        
        if (TotalLactatingPopulation > 0)
            SteadyStateLactating = TotalLactatingPopulation;
        
        if (TotalDryPopulation > 0)
            SteadyStateDry = TotalDryPopulation;
    }

    #endregion
}
