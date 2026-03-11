using AutoMapper;
using H.Core.Enumerations;
using H.Core.Factories;
using H.Core.Factories.Animals;
using H.Core.Mappers;
using H.Core.Models.Animals;
using Microsoft.Extensions.Logging;
using Prism.Ioc;
using System.Collections.ObjectModel;

namespace H.Core.Services.Animals;

public class AnimalComponentService : ComponentServiceBase, IAnimalComponentService
{
    #region Fields

    private readonly IAnimalComponentFactory _animalComponentFactory;
    private readonly ITransferService<AnimalComponentBase, AnimalComponentDto> _animalComponentTransferService;
    private readonly IMapper _animalGroupMapper;
    private readonly IManagementPeriodService _managementPeriodService;

    /// <summary>
    /// Valid animal types for the Other Animals category, used when populating
    /// <see cref="AnimalGroupDto.ValidAnimalTypes"/> during domain-to-DTO conversion.
    /// </summary>
    private static readonly AnimalType[] OtherAnimalTypes =
    {
        AnimalType.NotSelected,
        AnimalType.Bison,
        AnimalType.Goats,
        AnimalType.Alpacas,
        AnimalType.Deer,
        AnimalType.Elk,
        AnimalType.Llamas,
        AnimalType.Horses,
        AnimalType.Mules
    };

    #endregion

    #region Constructors

    public AnimalComponentService(
        ILogger logger,
        IAnimalComponentFactory animalComponentFactory,
        ITransferService<AnimalComponentBase, AnimalComponentDto> animalComponentTransferService,
        IContainerProvider containerProvider,
        IManagementPeriodService managementPeriodService) : base(logger)
    {
        _animalComponentTransferService = animalComponentTransferService ?? throw new ArgumentNullException(nameof(animalComponentTransferService));
        _animalComponentFactory = animalComponentFactory ?? throw new ArgumentNullException(nameof(animalComponentFactory));
        _managementPeriodService = managementPeriodService ?? throw new ArgumentNullException(nameof(managementPeriodService));

        if (containerProvider == null) throw new ArgumentNullException(nameof(containerProvider));

        _animalGroupMapper = containerProvider.Resolve<IMapper>(nameof(AnimalGroupToAnimalGroupDtoMapper));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Transfers a domain <see cref="AnimalComponentBase"/> to an <see cref="AnimalComponentDto"/> for UI binding.
    /// Populates <see cref="AnimalComponentDto.AnimalGroupDtos"/> and each group's
    /// <see cref="AnimalGroupDto.ManagementPeriodDtos"/> so that the Other Animals views can display
    /// saved data correctly.
    /// </summary>
    public IAnimalComponentDto TransferToAnimalComponentDto(AnimalComponentBase animalComponent)
    {
        // Base mapping: copies component-level properties (Name, Guid, etc.)
        var dto = _animalComponentTransferService.TransferDomainObjectToDto(animalComponent);

        // The base mapper does not map Groups → AnimalGroupDtos (property names differ),
        // so we must iterate through groups manually — same pattern as DairyComponentService.
        dto.AnimalGroupDtos = new ObservableCollection<AnimalGroupDto>();

        foreach (var animalGroup in animalComponent.Groups)
        {
            var animalGroupDto = _animalGroupMapper.Map<AnimalGroupDto>(animalGroup);

            // Populate the valid animal types for the group's ComboBox
            animalGroupDto.ValidAnimalTypes = new ObservableCollection<AnimalType>(OtherAnimalTypes);

            // Map each management period within this group to a ManagementPeriodDto.
            // Uses ManagementPeriodService which handles unit conversion (metric ↔ imperial).
            foreach (var managementPeriod in animalGroup.ManagementPeriods)
            {
                var managementPeriodDto = (ManagementPeriodDto)_managementPeriodService.TransferToManagementPeriodDto(managementPeriod);
                animalGroupDto.ManagementPeriodDtos.Add(managementPeriodDto);
            }

            dto.AnimalGroupDtos.Add(animalGroupDto);
        }

        Logger?.LogDebug(
            "Transferred AnimalComponent to DTO: {ComponentName}, " +
            "converted {GroupCount} groups with their management periods",
            dto.Name, animalComponent.Groups.Count);

        return dto;
    }

    /// <summary>
    /// Transfers an <see cref="AnimalComponentDto"/> back to the domain <see cref="AnimalComponentBase"/>.
    /// Converts <see cref="AnimalGroupDto.ManagementPeriodDtos"/> back to <see cref="ManagementPeriod"/>
    /// objects on the domain model so that data entered in the Other Animals views is persisted.
    /// </summary>
    public AnimalComponentBase TransferAnimalComponentDtoToSystem(
        AnimalComponentDto animalComponentDto,
        AnimalComponentBase animalComponent)
    {
        // Base mapping: copies component-level properties back to the domain model
        var result = _animalComponentTransferService.TransferDtoToDomainObject(animalComponentDto, animalComponent);

        // Rebuild groups from DTOs — clear and repopulate to stay in sync
        result.Groups.Clear();

        foreach (var animalGroupDto in animalComponentDto.AnimalGroupDtos)
        {
            var animalGroup = _animalGroupMapper.Map<AnimalGroup>(animalGroupDto);

            // Convert each ManagementPeriodDto back to a ManagementPeriod domain object
            foreach (var managementPeriodDto in animalGroupDto.ManagementPeriodDtos)
            {
                var managementPeriod = new ManagementPeriod();
                _managementPeriodService.TransferManagementPeriodDtoToSystem(managementPeriodDto, managementPeriod);
                animalGroup.ManagementPeriods.Add(managementPeriod);
            }

            result.Groups.Add(animalGroup);
        }

        Logger?.LogDebug(
            "Transferred DTO to AnimalComponent: {ComponentName}, " +
            "converted {GroupCount} groups with their management periods",
            result.Name, result.Groups.Count);

        return result;
    }

    #endregion
}