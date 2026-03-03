using H.Core.Factories;
using H.Core.Factories.Crops;
using H.Core.Factories.Fields;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Services.LandManagement.Fields;

public interface IFieldComponentService 
{
    /// <summary>
    /// When adding a new crop to the field, the year must be the next in order so that all years of the field history are consecutive.
    /// </summary>
    /// <param name="fieldComponentDto">The field containing the crops</param>
    /// <returns>The next consecutive year to should be used</returns>
    int GetNextCropYear(IFieldComponentDto fieldComponentDto);

    /// <summary>
    /// Once we have new instance of a <see cref="CropDto"/>, we must initialize it so that a minimal set of properties
    /// have sensible defaults
    /// </summary>
    void InitializeCropDto(IFieldComponentDto fieldComponentDto, ICropDto cropDto);

    /// <summary>
    /// Provides basic initialization for a <see cref="FieldSystemComponent"/> when being added to the <see cref="Farm"/> by a user.
    /// </summary>
    /// <param name="farm">The <see cref="Farm"/> the component is being added to</param>
    /// <param name="fieldSystemComponent">The <see cref="FieldSystemComponent"/> that has been created</param>
    void InitializeComponent(Farm farm, FieldSystemComponent fieldSystemComponent);

    /// <summary>
    /// All years in a collection of crops must be consecutive with no years missing. If a crop is removed, ensure all years are represented from start to finish of collection.
    /// </summary>
    /// <param name="cropDtos">The list of crops and associated years representing the history of the field</param>
    void ResetAllYears(IEnumerable<ICropDto>? cropDtos);

    /// <summary>
    /// Responsible for transferring user input bound to a <see cref="FieldSystemComponentDto"/> to an internal <see cref="FieldSystemComponent"/> before being stored internally.
    /// Method will convert values according to the selected units of measurement.
    /// </summary>
    /// <param name="fieldComponentDto">The object that is bound to the view and collects user input</param>
    /// <param name="fieldSystemComponent">The internal domain object that is used as the basis for input to GHG calculations</param>
    /// <returns>The <see cref="FieldSystemComponent"/> once all values have been transferred</returns>
    FieldSystemComponent TransferFieldDtoToSystem(FieldSystemComponentDto fieldComponentDto,
        FieldSystemComponent fieldSystemComponent);

    /// <summary>
    /// Create a new instance that is based on the state of an existing <see cref="FieldSystemComponent"/>. This method is used to create a
    /// new instance of a <see cref="IFieldComponentDto"/> that will be bound to a view.
    /// </summary>
    /// <param name="template">The <see cref="FieldSystemComponent"/> that will be used to provide default values for the new <see cref="FieldSystemComponentDto"/> instance</param>
    /// <returns>An instance of a <see cref="IFieldComponentDto"/> that can be bound to the view</returns>
    IFieldComponentDto TransferToFieldComponentDto(FieldSystemComponent template);

    /// <summary>
    /// Responsible for transferring user input bound to a <see cref="ICropDto"/> to an internal <see cref="CropViewItem"/> before being stored internally.
    /// Method will convert values according to the selected units of measurement.
    /// </summary>
    /// <param name="cropDto">The object that is bound to the view and collects user input</param>
    /// <param name="cropViewItem">The internal domain object that is used as the basis for input to GHG calculations</param>
    /// <returns>The <see cref="CropViewItem"/> once all values have been transferred</returns>
    CropViewItem TransferCropDtoToSystem(ICropDto cropDto, CropViewItem cropViewItem);

    /// <summary>
    /// Create copies of all the <see cref="CropViewItem"/> in a <see cref="FieldSystemComponent"/> and add corresponding <see cref="CropDto"/> instances to the <see cref="FieldSystemComponentDto"/>
    /// </summary>
    /// <param name="fieldSystemComponent">The <see cref="FieldSystemComponent"/> used to provide the basis/values needed to instantiate a <see cref="FieldSystemComponentDto"/></param>
    /// <param name="fieldComponentDto">An instance of <see cref="IFieldComponentDto"/> that can be bound to the view</param>
    void ConvertCropViewItemsToDtoCollection(FieldSystemComponent fieldSystemComponent, IFieldComponentDto fieldComponentDto);

    /// <summary>
    /// Create a new instance that is based on the state of an existing <see cref="CropViewItem"/>. This method is used to create a
    /// new instance of a <see cref="ICropDto"/> that will be bound to a view.
    /// </summary>
    /// <param name="cropViewItem">The <see cref="CropViewItem"/> that will be used to provide the basis/values needed to instantiate a <see cref="ICropDto"/></param>
    /// <returns>An instance of <see cref="ICropDto"/> that can be bound to the view</returns>
    ICropDto TransferCropViewItemToCropDto(CropViewItem cropViewItem);

    void ConvertCropDtoCollectionToCropViewItemCollection(FieldSystemComponent fieldSystemComponent, IFieldComponentDto fieldComponentDto);

    /// <summary>
    /// Adds a new <see cref="CropViewItem"/> to the <see cref="FieldSystemComponent"/> when the user creates a new <see cref="CropDto"/> in the view
    /// </summary>
    /// <param name="fieldSystemComponent">The associated <see cref="FieldSystemComponent"/> that will contain the new <see cref="CropViewItem"/></param>
    /// <param name="cropDto">The <see cref="CropDto"/> that will be associated with the <see cref="CropViewItem"/></param>
    void AddCropDtoToSystem(FieldSystemComponent fieldSystemComponent, ICropDto cropDto);

    /// <summary>
    /// Removes the <see cref="CropViewItem"/> associated with the <see cref="CropDto"/> that the user has choose to remove
    /// </summary>
    /// <param name="fieldSystemComponent">The associated <see cref="FieldSystemComponent"/> that contains the <see cref="CropViewItem"/> to be removed</param>
    /// <param name="cropDto">The <see cref="CropDto"/> that identifies which <see cref="CropViewItem"/> to remove</param>
    void RemoveCropFromSystem(FieldSystemComponent fieldSystemComponent, ICropDto cropDto);

    CropViewItem? GetCropViewItemFromDto(ICropDto cropDto, FieldSystemComponent fieldSystemComponent);

    /// <summary>
    /// Saves the UI state for a field component to preserve across ViewModel disposal cycles
    /// </summary>
    /// <param name="fieldComponentGuid">The GUID of the field component</param>
    /// <param name="selectedCropGuid">The GUID of the currently selected crop</param>
    /// <param name="additionalState">Additional UI state to preserve</param>
    void SaveUIState(Guid fieldComponentGuid, Guid? selectedCropGuid, Dictionary<string, object>? additionalState = null);

    /// <summary>
    /// Retrieves previously saved UI state for a field component
    /// </summary>
    /// <param name="fieldComponentGuid">The GUID of the field component</param>
    /// <returns>The saved UI state, or null if not found</returns>
    FieldComponentUIState? GetUIState(Guid fieldComponentGuid);

    /// <summary>
    /// Clears old UI state entries to prevent memory growth
    /// </summary>
    /// <param name="maxAge">Maximum age of state entries to keep (default: 1 hour)</param>
    void CleanupOldUIState(TimeSpan? maxAge = null);
}