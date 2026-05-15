using H.Core.Models.LandManagement.Fields;

namespace H.Core.Factories.Crops;

/// <summary>
/// Factory contract for constructing <see cref="CropDto"/> (view-bound) and
/// <see cref="CropViewItem"/> (domain-bound) instances. The factory layer sits between the
/// ViewModels and the persistence layer — VMs ask for a "new crop" or "new cover crop"
/// without knowing about the underlying mapper / initialization wiring.
///
/// <para><b>Why both DTO and ViewItem creation:</b></para>
/// New crops enter the system as DTOs (bound to the form fields). On save, the DTO is mapped
/// to a fresh <see cref="CropViewItem"/> via <see cref="CreateCropViewItem"/> and added to
/// the field's <c>CropViewItems</c> collection. The factory owns both ends of that mapping
/// so the wiring stays in one place.
/// </summary>
public interface ICropFactory : IFactory<CropDto>
{
    #region Public Methods

    /// <summary>
    /// Create a new <see cref="CropDto"/> seeded from an existing <see cref="CropViewItem"/>
    /// (e.g. when opening the editor for an already-saved crop). The DTO is bound to the
    /// view and absorbs mid-edit changes without mutating the source view item until save.
    /// </summary>
    /// <param name="template">The <see cref="CropViewItem"/> providing default values for the new DTO.</param>
    CropDto CreateCropDto(CropViewItem template);

    /// <summary>
    /// Reverse of <see cref="CreateCropDto"/> — produces a fresh <see cref="CropViewItem"/>
    /// from a DTO. Called on save to land the user's edits into the domain object that
    /// downstream services (carbon pipeline, manure service, etc.) consume.
    /// </summary>
    CropViewItem CreateCropViewItem(ICropDto cropDto);

    /// <summary>
    /// Convenience constructor for a cover crop. Identical to <see cref="IFactory{CropDto}.CreateDto()"/>
    /// but sets <see cref="CropDto.IsSecondaryCrop"/> = <c>true</c> and ties the DTO to the
    /// specified <paramref name="year"/>. Cover crops live in <c>FieldSystemComponent.CoverCrops</c>
    /// rather than <c>CropViewItems</c>; the carbon pipeline merges them into the main-crop
    /// row via <c>CombineInputsForAllCropsInSameYear</c> later.
    /// </summary>
    CropDto CreateCoverCropDto(int year);

    #endregion
}