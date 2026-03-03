using System.Collections.ObjectModel;
using H.Core.Factories.Animals;
using H.Core.Enumerations;
using H.Core.Models.Animals.Dairy;

namespace H.Core.Factories.Animals.Dairy;

/// <summary>
/// Interface for dairy component data transfer object
/// </summary>
public interface IDairyComponentDto : IAnimalComponentDto
{
    // Herd Overview - Input Parameters
    int TotalMilkingCows { get; set; }
    double ReplacementRate { get; set; }
    int CalvingIntervalMonths { get; set; }
    int DryPeriodDays { get; set; }
    double CalfMortalityRate { get; set; }
    double FemaleCalfRatio { get; set; }
    
    // Lifecycle Stage Durations
    int CalfStageDurationDays { get; set; }
    int HeiferStageDurationDays { get; set; }
    int LactationDurationDays { get; set; }
    
    // Calculated Herd Composition - Read-only outputs
    int CalculatedCalves { get; }
    int CalculatedHeifers { get; }
    int CalculatedLactating { get; }
    int CalculatedDry { get; }
    
    // Herd Production Defaults - Used to populate management periods
    // NOTE: See DairyComponentDto for detailed explanation of two-level architecture
    double DefaultMilkProduction { get; set; }
    double DefaultMilkFatContent { get; set; }
    double DefaultMilkProteinContent { get; set; }
    
    // Staggered Progression - Flow Rate Inputs
    // These represent the number of animals entering each lifecycle stage per year
    int CalvesEnteringPerYear { get; set; }
    int HeifersEnteringPerYear { get; set; }
    int LactatingCowsEnteringPerYear { get; set; }
    int DryCowsEnteringPerYear { get; set; }
    
    // Staggered Progression - Calculated Steady-State Populations
    // These are read-only calculated properties showing the steady-state population in each stage
    int SteadyStateCalves { get; }
    int SteadyStateHeifers { get; }
    int SteadyStateLactating { get; }
    int SteadyStateDry { get; }
    int TotalSteadyStateHerdSize { get; }
    
    // Population Entry Mode - Simple vs Advanced
    bool UseAdvancedPopulationMode { get; set; }
    ObservableCollection<DairyPopulationGroup> CalfPopulationGroups { get; }
    ObservableCollection<DairyPopulationGroup> HeiferPopulationGroups { get; }
    ObservableCollection<DairyPopulationGroup> LactatingPopulationGroups { get; }
    ObservableCollection<DairyPopulationGroup> DryPopulationGroups { get; }
    int TotalCalfPopulation { get; }
    int TotalHeiferPopulation { get; }
    int TotalLactatingPopulation { get; }
    int TotalDryPopulation { get; }

    // Dynamic Management Practices - per-stage collections that users can add/remove
    ObservableCollection<ManagementPeriodDto> CalfManagementPractices { get; }
    ObservableCollection<ManagementPeriodDto> HeiferManagementPractices { get; }
    ObservableCollection<ManagementPeriodDto> LactatingManagementPractices { get; }
    ObservableCollection<ManagementPeriodDto> DryManagementPractices { get; }

    // Manure Handling Systems - Phase-specific configurations (legacy fixed properties)
    ManureStateType HeiferPhase1ManureHandlingSystem { get; set; }
    ManureStateType HeiferPhase2ManureHandlingSystem { get; set; }
    ManureStateType CalfPhase1ManureHandlingSystem { get; set; }
    ManureStateType CalfPhase2ManureHandlingSystem { get; set; }
    ManureStateType LactatingPhase1ManureHandlingSystem { get; set; }
    ManureStateType LactatingPhase2ManureHandlingSystem { get; set; }
    ManureStateType LactatingPhase3ManureHandlingSystem { get; set; }
    ManureStateType LactatingPhase4ManureHandlingSystem { get; set; }
    ManureStateType DryPhase1ManureHandlingSystem { get; set; }
    ManureStateType DryPhase2ManureHandlingSystem { get; set; }
    
    // Housing Types - Phase-specific configurations
    HousingType HeiferPhase1HousingType { get; set; }
    HousingType HeiferPhase2HousingType { get; set; }
    HousingType CalfPhase1HousingType { get; set; }
    HousingType CalfPhase2HousingType { get; set; }
    HousingType LactatingPhase1HousingType { get; set; }
    HousingType LactatingPhase2HousingType { get; set; }
    HousingType LactatingPhase3HousingType { get; set; }
    HousingType LactatingPhase4HousingType { get; set; }
    HousingType DryPhase1HousingType { get; set; }
    HousingType DryPhase2HousingType { get; set; }
}
