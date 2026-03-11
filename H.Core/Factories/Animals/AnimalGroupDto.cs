using H.Core.Enumerations;
using H.Core.Models.Animals;
using System.Collections.ObjectModel;

namespace H.Core.Factories.Animals;

public class AnimalGroupDto : DtoBase, IAnimalGroupDto
{
    #region Fields

    private AnimalType? _groupType;
    private bool _isSelected;
    private ObservableCollection<ManagementPeriodDto> _managementPeriodDtos;

    #endregion

    #region Constructors

    public AnimalGroupDto()
    {
        _managementPeriodDtos = new ObservableCollection<ManagementPeriodDto>();
    }

    #endregion

    #region Properties

    public ObservableCollection<AnimalType> ValidAnimalTypes { get; set; } = new ObservableCollection<AnimalType>();

    public AnimalType? GroupType
    {
        get => _groupType;
        set => SetProperty(ref _groupType, value);
    }

    /// <summary>
    /// Indicates whether this group is currently selected in the UI.
    /// Used to toggle the visual selection highlight via the <c>selected</c> CSS class.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    /// <summary>
    /// Collection of management period DTOs that belong to this animal group.
    /// Mirrors the domain model's <see cref="AnimalGroup.ManagementPeriods"/> collection.
    /// </summary>
    public ObservableCollection<ManagementPeriodDto> ManagementPeriodDtos
    {
        get => _managementPeriodDtos;
        set => SetProperty(ref _managementPeriodDtos, value);
    }

	#endregion
}
