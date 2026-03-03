using AutoMapper;
using H.Core.Factories.Animals.Dairy;
using H.Core.Models;
using H.Core.Models.Animals.Dairy;
using H.Core.Services.Animals;
using Microsoft.Extensions.Logging;
using Prism.Ioc;
using H.Core.Models.Animals;
using H.Core.Enumerations;
using H.Core.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;
using H.Core.Factories.Animals;
using System.Collections.ObjectModel;

namespace H.Core.Services.Animals.Dairy;

/// <summary>
/// Service for managing dairy component operations and data transfer.
/// Handles initialization, validation, and conversion between domain models and DTOs.
/// </summary>
public class DairyComponentService : ComponentServiceBase, IDairyComponentService
{
    #region Fields

    private readonly IMapper _mapper;
    private readonly IMapper _animalGroupMapper;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the DairyComponentService
    /// </summary>
    /// <param name="logger">Logger for diagnostic and error logging</param>
    /// <param name="containerProvider">Container provider to resolve the dairy-specific mapper</param>
    /// <exception cref="ArgumentNullException">Thrown if containerProvider is null</exception>
    public DairyComponentService(ILogger logger, IContainerProvider containerProvider) : base(logger)
    {
        if (containerProvider == null)
        {
            throw new ArgumentNullException(nameof(containerProvider));
        }

        // Resolve the dairy-specific mapper by name
        _mapper = containerProvider.Resolve<IMapper>(nameof(DairyComponentToDtoMapper));
        
        // Resolve the animal group mapper for converting between AnimalGroup and AnimalGroupDto
        _animalGroupMapper = containerProvider.Resolve<IMapper>(nameof(AnimalGroupToAnimalGroupDtoMapper));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Initializes a new dairy component with default values appropriate for a dairy operation.
    /// Ensures a unique name and marks the component as initialized.
    /// </summary>
    /// <param name="farm">The farm to which this dairy component belongs</param>
    /// <param name="dairyComponent">The dairy component to initialize</param>
    public void InitializeComponent(Farm farm, DairyComponent dairyComponent)
    {
        base.InitializeComponent(farm, dairyComponent);

        // Add any dairy-specific initialization here
        // For example, setting default herd parameters
    }

    /// <summary>
    /// Creates a dairy component DTO from a domain model for UI binding.
    /// Uses AutoMapper to transfer properties from the domain model to the DTO.
    /// 
    /// ARCHITECTURE NOTE:
    /// This method also converts AnimalGroup domain objects to AnimalGroupDto objects.
    /// This ensures the view binds to DTOs (with validation) instead of domain objects.
    /// </summary>
    /// <param name="dairyComponent">The source dairy component domain model</param>
    /// <returns>A DTO suitable for binding in the view</returns>
    public IDairyComponentDto TransferToDairyComponentDto(DairyComponent dairyComponent)
    {
        var dairyComponentDto = _mapper.Map<DairyComponentDto>(dairyComponent);
        
        // Convert AnimalGroup domain objects to AnimalGroupDto objects
        dairyComponentDto.AnimalGroupDtos = new ObservableCollection<AnimalGroupDto>();
        foreach (var animalGroup in dairyComponent.Groups)
        {
            var animalGroupDto = _animalGroupMapper.Map<AnimalGroupDto>(animalGroup);
            dairyComponentDto.AnimalGroupDtos.Add(animalGroupDto);
        }
        
        Logger?.LogDebug(
            $"Transferred DairyComponent to DTO: {dairyComponentDto.Name}, " +
            $"converted {dairyComponent.Groups.Count} AnimalGroups to AnimalGroupDtos");
        
        return dairyComponentDto;
    }

    /// <summary>
    /// Transfers data from DTO back to the domain model after validation.
    /// Uses AutoMapper to apply changes from the DTO to the existing domain model.
    /// 
    /// ARCHITECTURE NOTE:
    /// This method also converts AnimalGroupDto objects back to AnimalGroup domain objects.
    /// This ensures validated user input flows from DTOs to the domain model.
    /// </summary>
    /// <param name="dairyDto">The source DTO containing user input</param>
    /// <param name="dairyComponent">The target domain model to update</param>
    /// <returns>The updated dairy component</returns>
    public DairyComponent TransferDairyDtoToSystem(DairyComponentDto dairyDto, DairyComponent dairyComponent)
    {
        // Transfer basic properties
        _mapper.Map(dairyDto, dairyComponent);
        
        // Convert AnimalGroupDto objects back to AnimalGroup domain objects
        // Clear existing groups and rebuild from DTOs
        dairyComponent.Groups.Clear();
        foreach (var animalGroupDto in dairyDto.AnimalGroupDtos)
        {
            var animalGroup = _animalGroupMapper.Map<AnimalGroup>(animalGroupDto);
            dairyComponent.Groups.Add(animalGroup);
        }
        
        Logger?.LogDebug(
            $"Transferred DTO to DairyComponent: {dairyComponent.Name}, " +
            $"converted {dairyDto.AnimalGroupDtos.Count} AnimalGroupDtos to AnimalGroups");
        
        return dairyComponent;
    }

    /// <summary>
    /// Auto-generates the four lifecycle-based animal groups for a dairy herd based on calculated herd composition.
    /// Creates: Calf group, Heifer group, Lactating cow group, and Dry cow group.
    /// 
    /// ARCHITECTURE NOTE:
    /// This method bridges the gap between the simplified herd overview inputs and the detailed
    /// animal group structure required for emissions calculations. The user enters high-level parameters
    /// (e.g., total milking cows, replacement rate), and this method creates the detailed animal groups
    /// with appropriate management periods.
    /// 
    /// WHY AUTO-GENERATION?
    /// - Simplifies data entry for users
    /// - Ensures consistency across the herd
    /// - Reduces errors from manual animal group creation
    /// - Advanced users can still manually adjust individual groups after generation
    /// 
    /// IMPORTANT - DATA PRESERVATION:
    /// This method ONLY generates groups if:
    /// 1. forceRegeneration = true (user explicitly clicked "Regenerate Groups" button), OR
    /// 2. The component has NO existing groups (first-time setup)
    /// 
    /// This prevents accidental deletion of user-configured animal groups when loading saved components.
    /// </summary>
    /// <param name="dairyDto">The DTO containing herd overview parameters and calculated animal counts</param>
    /// <param name="dairyComponent">The dairy component to populate with animal groups</param>
    /// <param name="forceRegeneration">If true, clears existing groups and regenerates. If false, only generates if component has no groups.</param>
    public void GenerateAnimalGroups(DairyComponentDto dairyDto, DairyComponent dairyComponent, bool forceRegeneration = false)
    {
        ArgumentNullException.ThrowIfNull(dairyDto);
        ArgumentNullException.ThrowIfNull(dairyComponent);

        // DATA PRESERVATION: Only generate groups if explicitly requested OR component is empty
        var hasExistingGroups = dairyComponent.Groups.Any();
        
        if (hasExistingGroups && !forceRegeneration)
        {
            Logger?.LogInformation(
                $"Skipping auto-generation for dairy component '{dairyComponent.Name}' - " +
                $"component has {dairyComponent.Groups.Count} existing groups. " +
                $"Set forceRegeneration=true to regenerate.");
            return;
        }

        // If forcing regeneration, clear existing groups
        if (forceRegeneration && hasExistingGroups)
        {
            Logger?.LogWarning(
                $"Force regeneration requested - clearing {dairyComponent.Groups.Count} existing groups " +
                $"from dairy component '{dairyComponent.Name}'");
            dairyComponent.Groups.Clear();
            dairyDto.AnimalGroupDtos.Clear();
        }

        // Generate the four lifecycle-based groups
        var calfGroup = CreateCalfGroup(dairyDto);
        var heiferGroup = CreateHeiferGroup(dairyDto);
        var lactatingGroup = CreateLactatingGroup(dairyDto);
        var dryGroup = CreateDryGroup(dairyDto);

        // Add groups to component (domain model)
        dairyComponent.Groups.Add(calfGroup);
        dairyComponent.Groups.Add(heiferGroup);
        dairyComponent.Groups.Add(lactatingGroup);
        dairyComponent.Groups.Add(dryGroup);
        
        // Also add as DTOs for UI binding
        dairyDto.AnimalGroupDtos.Add(_animalGroupMapper.Map<AnimalGroupDto>(calfGroup));
        dairyDto.AnimalGroupDtos.Add(_animalGroupMapper.Map<AnimalGroupDto>(heiferGroup));
        dairyDto.AnimalGroupDtos.Add(_animalGroupMapper.Map<AnimalGroupDto>(lactatingGroup));
        dairyDto.AnimalGroupDtos.Add(_animalGroupMapper.Map<AnimalGroupDto>(dryGroup));

        Logger?.LogInformation(
            $"Auto-generated 4 animal groups for dairy component '{dairyComponent.Name}': " +
            $"{calfGroup.Name} ({dairyDto.CalculatedCalves} head), " +
            $"{heiferGroup.Name} ({dairyDto.CalculatedHeifers} head), " +
            $"{lactatingGroup.Name} ({dairyDto.CalculatedLactating} head), " +
            $"{dryGroup.Name} ({dairyDto.CalculatedDry} head)");
    }

    #endregion

    #region Private Methods - Animal Group Creation

    /// <summary>
    /// Creates a calf group (birth to 4 months) with appropriate management period
    /// </summary>
    private AnimalGroup CreateCalfGroup(DairyComponentDto dairyDto)
    {
        var group = new AnimalGroup
        {
            Name = "Dairy Calves",
            GroupType = AnimalType.DairyCalves,
            Guid = Guid.NewGuid()
        };

        var managementPeriod = new ManagementPeriod
        {
            Name = "Calf Period",
            AnimalType = AnimalType.DairyCalves,
            NumberOfAnimals = dairyDto.CalculatedCalves,
            Start = new DateTime(DateTime.Now.Year, 1, 1),
            End = new DateTime(DateTime.Now.Year, 12, 31),
            NumberOfDays = 365,
            StartWeight = 45,  // Typical calf birth weight (kg)
            EndWeight = 120,   // Typical calf weight at 4 months (kg)
            PeriodDailyGain = 0.6,  // Typical daily gain for calves (kg/day)
            Guid = Guid.NewGuid()
        };

        // Calves are typically on milk/starter diet
        managementPeriod.HousingDetails.HousingType = HousingType.HousedInBarn;
        managementPeriod.ManureDetails.StateType = ManureStateType.SolidStorage;

        group.ManagementPeriods.Add(managementPeriod);

        return group;
    }

    /// <summary>
    /// Creates a heifer group (replacement stock) with appropriate management period
    /// </summary>
    private AnimalGroup CreateHeiferGroup(DairyComponentDto dairyDto)
    {
        var group = new AnimalGroup
        {
            Name = "Dairy Heifers",
            GroupType = AnimalType.DairyHeifers,
            Guid = Guid.NewGuid()
        };

        var managementPeriod = new ManagementPeriod
        {
            Name = "Heifer Period",
            AnimalType = AnimalType.DairyHeifers,
            NumberOfAnimals = dairyDto.CalculatedHeifers,
            Start = new DateTime(DateTime.Now.Year, 1, 1),
            End = new DateTime(DateTime.Now.Year, 12, 31),
            NumberOfDays = 365,
            StartWeight = 120,  // Weight at 4 months (kg)
            EndWeight = 600,    // Weight at first calving (kg)
            PeriodDailyGain = 0.8,  // Typical daily gain for heifers (kg/day)
            Guid = Guid.NewGuid()
        };

        // Heifers are typically housed or on pasture
        managementPeriod.HousingDetails.HousingType = HousingType.HousedInBarn;
        managementPeriod.ManureDetails.StateType = ManureStateType.SolidStorage;

        group.ManagementPeriods.Add(managementPeriod);

        return group;
    }

    /// <summary>
    /// Creates a lactating cow group with milk production parameters from herd defaults
    /// </summary>
    private AnimalGroup CreateLactatingGroup(DairyComponentDto dairyDto)
    {
        var group = new AnimalGroup
        {
            Name = "Lactating Cows",
            GroupType = AnimalType.DairyLactatingCow,
            Guid = Guid.NewGuid()
        };

        var managementPeriod = new ManagementPeriod
        {
            Name = "Lactation Period",
            AnimalType = AnimalType.DairyLactatingCow,
            NumberOfAnimals = dairyDto.CalculatedLactating,
            Start = new DateTime(DateTime.Now.Year, 1, 1),
            End = new DateTime(DateTime.Now.Year, 12, 31),
            NumberOfDays = 365,
            StartWeight = 650,  // Typical mature cow weight (kg)
            EndWeight = 650,    // Constant weight during lactation
            PeriodDailyGain = 0,  // No gain during lactation
            
            // Use the herd-level production defaults
            MilkProduction = dairyDto.DefaultMilkProduction,
            MilkFatContent = dairyDto.DefaultMilkFatContent,
            MilkProteinContentAsPercentage = dairyDto.DefaultMilkProteinContent,
            
            Guid = Guid.NewGuid()
        };

        // Lactating cows typically in barn with freestall or tiestall
        managementPeriod.HousingDetails.HousingType = HousingType.HousedInBarn;
        managementPeriod.ManureDetails.StateType = ManureStateType.LiquidWithNaturalCrust;

        group.ManagementPeriods.Add(managementPeriod);

        return group;
    }

    /// <summary>
    /// Creates a dry cow group (non-lactating, pre-calving) with appropriate management period
    /// </summary>
    private AnimalGroup CreateDryGroup(DairyComponentDto dairyDto)
    {
        var group = new AnimalGroup
        {
            Name = "Dry Cows",
            GroupType = AnimalType.DairyDryCow,
            Guid = Guid.NewGuid()
        };

        var managementPeriod = new ManagementPeriod
        {
            Name = "Dry Period",
            AnimalType = AnimalType.DairyDryCow,
            NumberOfAnimals = dairyDto.CalculatedDry,
            Start = new DateTime(DateTime.Now.Year, 1, 1),
            End = new DateTime(DateTime.Now.Year, 12, 31),
            NumberOfDays = dairyDto.DryPeriodDays,  // Use the dry period from herd overview
            StartWeight = 650,  // Typical mature cow weight (kg)
            EndWeight = 700,    // Weight gain during dry period preparing for calving (kg)
            PeriodDailyGain = 0.8,  // Weight gain during dry period (kg/day)
            MilkProduction = 0,  // No milk production during dry period
            Guid = Guid.NewGuid()
        };

        // Dry cows typically housed separately
        managementPeriod.HousingDetails.HousingType = HousingType.HousedInBarn;
        managementPeriod.ManureDetails.StateType = ManureStateType.SolidStorage;

        group.ManagementPeriods.Add(managementPeriod);

        return group;
    }

    #endregion
}
