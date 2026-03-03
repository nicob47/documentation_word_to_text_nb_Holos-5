using H.Core.Factories;
using H.Core.Factories.Crops;
using H.Core.Factories.Fields;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services.Animals;
using Microsoft.Extensions.Logging;

namespace H.Core.Services.LandManagement.Fields;

/// <summary>
/// Orchestrates operations for <see cref="FieldSystemComponent"/> and its DTOs.
/// - Creates and initializes field and crop DTOs/view items.
/// - Transfers data between domain models and DTOs using <see cref="ITransferService{TModelBase, TDto}"/>.
/// - Applies unit conversions via configured transfer services.
/// - Assists with UI-bound workflows such as year ordering and add/remove/update of crops.
/// - Preserves UI state across ViewModel lifecycle events.
/// </summary>
public class FieldComponentService : ComponentServiceBase, IFieldComponentService
{
    #region Fields
    
    private readonly IFieldFactory _fieldFactory;
    private readonly ICropFactory _cropFactory;

    private readonly ITransferService<CropViewItem, CropDto> _cropTransferService;
    private readonly ITransferService<FieldSystemComponent, FieldSystemComponentDto> _fieldTransferService;

    // State preservation for UI elements
    private readonly Dictionary<Guid, FieldComponentUIState> _uiStateCache = new();

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of <see cref="FieldComponentService"/>.
    /// </summary>
    /// <param name="fieldFactory">Factory used to create field-related DTOs and view items.</param>
    /// <param name="cropFactory">Factory used to create crop DTOs and view items.</param>
    /// <param name="logger">Logger injected into <see cref="ComponentServiceBase"/> for diagnostics.</param>
    /// <param name="cropTransferService">
    /// Transfer service that maps between <see cref="CropViewItem"/> (domain) and <see cref="CropDto"/> (DTO),
    /// including unit conversions for UI binding and persistence.
    /// </param>
    /// <param name="fieldTransferService">
    /// Transfer service that maps between <see cref="FieldSystemComponent"/> (domain) and <see cref="FieldSystemComponentDto"/> (DTO),
    /// including unit conversions for UI binding and persistence.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
    public FieldComponentService(
        IFieldFactory fieldFactory,
        ICropFactory cropFactory,
        ILogger logger,
        ITransferService<CropViewItem, CropDto> cropTransferService,
        ITransferService<FieldSystemComponent, FieldSystemComponentDto> fieldTransferService) : base(logger)
    {
        if (fieldTransferService != null)
        {
            _fieldTransferService = fieldTransferService;
        }
        else
        {
            throw new ArgumentNullException(nameof(fieldTransferService));
        }

        if (cropTransferService != null)
        {
            _cropTransferService = cropTransferService;
        }
        else
        {
            throw new ArgumentNullException(nameof(cropTransferService));
        }

        if (cropFactory != null)
        {
            _cropFactory = cropFactory;
        }
        else
        {
            throw new ArgumentNullException(nameof(cropFactory));
        }

        if (fieldFactory != null)
        {
            _fieldFactory = fieldFactory;
        }
        else
        {
            throw new ArgumentNullException(nameof(fieldFactory));
        }
    }

    #endregion

    #region State Preservation Methods

    /// <summary>
    /// Saves the UI state for a field component to preserve across ViewModel disposal cycles
    /// </summary>
    /// <param name="fieldComponentGuid">The GUID of the field component</param>
    /// <param name="selectedCropGuid">The GUID of the currently selected crop</param>
    /// <param name="additionalState">Additional UI state to preserve</param>
    public void SaveUIState(Guid fieldComponentGuid, Guid? selectedCropGuid, Dictionary<string, object>? additionalState = null)
    {
        Logger?.LogDebug("Saving UI state for field component {FieldComponentGuid}", fieldComponentGuid);

        var uiState = new FieldComponentUIState
        {
            FieldComponentGuid = fieldComponentGuid,
            SelectedCropGuid = selectedCropGuid,
            LastAccessed = DateTime.UtcNow,
            AdditionalState = additionalState ?? new Dictionary<string, object>()
        };

        _uiStateCache[fieldComponentGuid] = uiState;
    }

    /// <summary>
    /// Retrieves previously saved UI state for a field component
    /// </summary>
    /// <param name="fieldComponentGuid">The GUID of the field component</param>
    /// <returns>The saved UI state, or null if not found</returns>
    public FieldComponentUIState? GetUIState(Guid fieldComponentGuid)
    {
        Logger?.LogDebug("Retrieving UI state for field component {FieldComponentGuid}", fieldComponentGuid);

        if (_uiStateCache.TryGetValue(fieldComponentGuid, out var uiState))
        {
            uiState.LastAccessed = DateTime.UtcNow;
            Logger?.LogDebug("Found cached UI state for field component {FieldComponentGuid}", fieldComponentGuid);
            return uiState;
        }

        Logger?.LogDebug("No cached UI state found for field component {FieldComponentGuid}", fieldComponentGuid);
        return null;
    }

    /// <summary>
    /// Clears old UI state entries to prevent memory growth
    /// </summary>
    /// <param name="maxAge">Maximum age of state entries to keep (default: 1 hour)</param>
    public void CleanupOldUIState(TimeSpan? maxAge = null)
    {
        var cutoff = DateTime.UtcNow - (maxAge ?? TimeSpan.FromHours(1));
        var keysToRemove = _uiStateCache
            .Where(kvp => kvp.Value.LastAccessed < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _uiStateCache.Remove(key);
        }

        if (keysToRemove.Any())
        {
            Logger?.LogDebug("Cleaned up {Count} old UI state entries", keysToRemove.Count);
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Applies default initialization for a new <see cref="FieldSystemComponent"/> being added to a <see cref="Farm"/>.
    /// Ensures a unique name and marks the component as initialized. No-ops if already initialized.
    /// </summary>
    /// <param name="farm">The target farm.</param>
    /// <param name="fieldSystemComponent">The field component to initialize.</param>
    /// <exception cref="ArgumentNullException">Thrown when <see cref="fieldSystemComponent"/> is null.</exception>
    public void InitializeComponent(Farm farm, FieldSystemComponent fieldSystemComponent)
    {
        base.InitializeComponent(farm, fieldSystemComponent);

        fieldSystemComponent.EndYear = DateTime.Now.Year;
        fieldSystemComponent.StartYear = fieldSystemComponent.EndYear - 10;
    }

    /// <summary>
    /// Initializes a newly created <see cref="ICropDto"/> with sensible defaults and adds it to the parent field DTO.
    /// </summary>
    /// <param name="fieldComponentDto">Parent field DTO receiving the new crop.</param>
    /// <param name="cropDto">The crop DTO to initialize and add.</param>
    public void InitializeCropDto(IFieldComponentDto fieldComponentDto, ICropDto cropDto)
    {
        cropDto.Year = this.GetNextCropYear(fieldComponentDto);

        fieldComponentDto?.CropDtos?.Add(cropDto);
    }

    /// <summary>
    /// Computes the next chronological year to assign to a newly added crop for the specified field DTO.
    /// </summary>
    /// <param name="fieldComponentDto">The field DTO whose crop years are evaluated.</param>
    /// <returns>The next year to use.</returns>
    public int GetNextCropYear(IFieldComponentDto fieldComponentDto)
    {
        var result = DateTime.Now.Year;

        if (fieldComponentDto.CropDtos?.Any() == true)
        {
            result = fieldComponentDto.CropDtos.Min(dto => dto.Year) - 1;
        }

        return result;
    }

    /// <summary>
    /// Re-numbers crop years so that they are consecutive, descending from the maximum year.
    /// </summary>
    /// <param name="cropDtos">The collection of crops to normalize.</param>
    public void ResetAllYears(IEnumerable<ICropDto>? cropDtos)
    {
        if (cropDtos == null) return;
        var dtos = cropDtos.ToList();
        if (dtos.Any())
        {
            var maximumYear = dtos.Max(dto => dto.Year);

            // Use ThenBy to ensure stable sorting when years are equal
            var orderedDtos = dtos.OrderByDescending(dto => dto.Year).ThenBy(dto => dto.Guid).ToList();
            for (int i = 0; i < orderedDtos.Count; i++)
            {
                var dto = orderedDtos[i];
                dto.Year = maximumYear - i;
            }
        }
    }

    /// <summary>
    /// Converts a <see cref="CropViewItem"/> domain object to a <see cref="CropDto"/> for UI binding.
    /// </summary>
    /// <param name="cropViewItem">Domain model instance.</param>
    /// <returns>Mapped DTO instance.</returns>
    public ICropDto TransferCropViewItemToCropDto(CropViewItem cropViewItem)
    {
        return _cropTransferService.TransferDomainObjectToDto(cropViewItem);
    }

    /// <summary>
    /// Applies values from a <see cref="ICropDto"/> to an existing <see cref="CropViewItem"/> domain object.
    /// </summary>
    /// <param name="cropDto">Source DTO bound to the UI.</param>
    /// <param name="cropViewItem">Target domain model to update.</param>
    /// <returns>The updated <see cref="CropViewItem"/>.</returns>
    public CropViewItem TransferCropDtoToSystem(ICropDto cropDto, CropViewItem cropViewItem)
    {
        return _cropTransferService.TransferDtoToDomainObject((CropDto) cropDto, cropViewItem);
    }

    /// <summary>
    /// Applies values from a <see cref="FieldSystemComponentDto"/> to an existing <see cref="FieldSystemComponent"/> domain object.
    /// </summary>
    /// <param name="fieldComponentDto">Source DTO bound to the UI.</param>
    /// <param name="fieldSystemComponent">Target domain model to update.</param>
    /// <returns>The updated <see cref="FieldSystemComponent"/>.</returns>
    public FieldSystemComponent TransferFieldDtoToSystem(FieldSystemComponentDto fieldComponentDto,
        FieldSystemComponent fieldSystemComponent)
    {
        return _fieldTransferService.TransferDtoToDomainObject(fieldComponentDto, fieldSystemComponent);
    }

    /// <summary>
    /// Creates a new <see cref="IFieldComponentDto"/> from a <see cref="FieldSystemComponent"/> for UI binding.
    /// Also clones related <see cref="CropViewItem"/> instances into DTOs.
    /// </summary>
    /// <param name="template">The source field system component.</param>
    /// <returns>A DTO suitable for binding in the view.</returns>
    public IFieldComponentDto TransferToFieldComponentDto(FieldSystemComponent template)
    {
        var fieldComponentDto = _fieldTransferService.TransferDomainObjectToDto(template);

        this.ConvertCropViewItemsToDtoCollection(template, fieldComponentDto);

        return fieldComponentDto;
    }

    /// <summary>
    /// Rebuilds the crop DTO collection from the domain <see cref="FieldSystemComponent"/> crop view items.
    /// </summary>
    /// <param name="fieldSystemComponent">Source domain field component.</param>
    /// <param name="fieldComponentDto">Target DTO to receive crop copies.</param>
    public void ConvertCropViewItemsToDtoCollection(FieldSystemComponent fieldSystemComponent, IFieldComponentDto fieldComponentDto)
    {
        if (fieldComponentDto is not null && fieldSystemComponent is not null)
        {
            // Clear stale and outdated crop DTOs before rebuilding from domain objects
            fieldComponentDto.CropDtos?.Clear();

            // Loop through each crop view item in the domain object and create a corresponding DTO
            foreach (var cropViewItem in fieldSystemComponent.CropViewItems)
            {
                // Use the factory to create a new DTO based on the view item template
                var dto = _cropFactory.CreateCropDto(template: cropViewItem);

                // Add the newly created DTO to the field component DTO's crop collection
                fieldComponentDto.CropDtos?.Add(dto);
            }
        }
    }

    /// <summary>
    /// Pushes values from a field DTO crop collection back into the domain <see cref="FieldSystemComponent"/> crop view items.
    /// Only crops with matching GUIDs are updated.
    /// </summary>
    /// <param name="fieldSystemComponent">Target domain field component.</param>
    /// <param name="fieldComponentDto">Source DTO containing edited crop values.</param>
    public void ConvertCropDtoCollectionToCropViewItemCollection(FieldSystemComponent fieldSystemComponent, IFieldComponentDto fieldComponentDto)
    {
        if (fieldSystemComponent is not null && fieldComponentDto is not null)
        {
            if (fieldSystemComponent is not null && fieldComponentDto.CropDtos is not null)
            {
                foreach (var cropDto in fieldComponentDto.CropDtos)
                {
                    var viewItem = fieldSystemComponent.CropViewItems.SingleOrDefault(viewItem => viewItem.Guid.Equals(cropDto.Guid));
                    if (viewItem is not null)
                    {
                        this.TransferCropDtoToSystem(cropDto, viewItem);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds a new crop to the domain field component based on a provided crop DTO.
    /// </summary>
    /// <param name="fieldSystemComponent">Domain field component to modify.</param>
    /// <param name="cropDto">DTO used to create the new view item.</param>
    public void AddCropDtoToSystem(FieldSystemComponent fieldSystemComponent, ICropDto cropDto)
    {
        if (fieldSystemComponent is not null)
        {
            if (cropDto is not null)
            {
                var cropViewItem = _cropFactory.CreateCropViewItem(cropDto);

                fieldSystemComponent.CropViewItems.Add(cropViewItem);
            }
        }
    }

    /// <summary>
    /// Removes a crop view item from the domain field component that matches the provided DTO GUID.
    /// </summary>
    /// <param name="fieldSystemComponent">Domain field component to modify.</param>
    /// <param name="cropDto">DTO identifying which crop to remove.</param>
    public void RemoveCropFromSystem(FieldSystemComponent fieldSystemComponent, ICropDto cropDto)
    {
        if (cropDto is not null)
        {
            // By default, all DTO objects will have their GUID property set to be equal to the GUID of the associated domain object
            var cropViewItem = fieldSystemComponent.CropViewItems.SingleOrDefault(x => x.Guid.Equals(cropDto.Guid));
            if (cropViewItem is not null)
            {
                fieldSystemComponent.CropViewItems.Remove(cropViewItem);
            }
        }
    }

    /// <summary>
    /// Finds a crop view item in the domain field component that corresponds to the provided crop DTO GUID.
    /// </summary>
    /// <param name="cropDto">DTO whose GUID is used for lookup.</param>
    /// <param name="fieldSystemComponent">Domain field component to search.</param>
    /// <returns>The matching <see cref="CropViewItem"/> if found; otherwise, null.</returns>
    public CropViewItem? GetCropViewItemFromDto(ICropDto cropDto, FieldSystemComponent fieldSystemComponent)
    {
        return fieldSystemComponent.CropViewItems.SingleOrDefault(x => x.Guid.Equals(cropDto.Guid));
    }

    #endregion

    #region Private Methods

    #endregion
}