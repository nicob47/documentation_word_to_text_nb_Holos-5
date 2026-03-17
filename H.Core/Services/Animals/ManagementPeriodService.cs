using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Converters;
using H.Core.Enumerations;
using H.Core.Factories;
using H.Core.Factories.Animals;
using H.Core.Mappers;
using H.Core.Models;
using H.Core.Models.Animals;
using Microsoft.Extensions.Logging;
using Prism.Ioc;

namespace H.Core.Services.Animals;

public class ManagementPeriodService : IManagementPeriodService
{
    #region Fields

    private readonly ILogger _logger;
    private readonly IContainerProvider _containerProvider;
    private readonly IManagementPeriodFactory _managementPeriodFactory;
    private readonly IUnitsOfMeasurementCalculator _unitsOfMeasurementCalculator;

    private readonly IModelMapper<ManagementPeriodDto, ManagementPeriod> _managementPeriodDtoToManagementPeriodMapper;
    private readonly IModelMapper<ManagementPeriod, ManagementPeriodDto> _managementPeriodToManagementPeriodDtoMapper;

    #endregion

    #region Constructors

    public ManagementPeriodService(ILogger logger, IContainerProvider containerProvider, IManagementPeriodFactory managementPeriodFactory, IUnitsOfMeasurementCalculator unitsOfMeasurementCalculator)
    {
        if (logger != null)
        {
            _logger = logger;
        }
        else
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (containerProvider != null)
        {
            _containerProvider = containerProvider;
        }
        else
        {
            throw new ArgumentNullException(nameof(containerProvider));
        }

        if (managementPeriodFactory != null)
        {
            _managementPeriodFactory = managementPeriodFactory;
        }
        else
        {
            throw new ArgumentNullException(nameof(managementPeriodFactory));
        }

        if (unitsOfMeasurementCalculator != null)
        {
            _unitsOfMeasurementCalculator = unitsOfMeasurementCalculator;
        }
        else
        {
            throw new ArgumentNullException(nameof(unitsOfMeasurementCalculator));
        }

        _managementPeriodDtoToManagementPeriodMapper = _containerProvider.Resolve<IModelMapper<ManagementPeriodDto, ManagementPeriod>>(nameof(ManagementPeriodDtoToManagementPeriodMapper));
        _managementPeriodToManagementPeriodDtoMapper = _containerProvider.Resolve<IModelMapper<ManagementPeriod, ManagementPeriodDto>>(nameof(ManagementPeriodToManagementPeriodDtoMapper));
    }

    #endregion

    #region Public Methods

    public IManagementPeriodDto TransferToManagementPeriodDto(ManagementPeriod managementPeriod)
    {
        var managementPeriodDto = _managementPeriodToManagementPeriodDtoMapper.Map(managementPeriod);

        // All numerical values are stored internally as metric values
        var propertyConverter = new PropertyConverter<IManagementPeriodDto>(managementPeriodDto);

        // Get all properties that might need to be converted to imperial units before being shown to the user
        if (propertyConverter.PropertyInfos != null)
        {
            foreach (var property in propertyConverter.PropertyInfos)
            {
                // Convert the value from metric to imperial as needed. Note the converter won't convert anything if the display is in metric units
                var bindingValue = propertyConverter.GetBindingValueFromSystem(property, _unitsOfMeasurementCalculator.GetUnitsOfMeasurement());

                // Set the value of the property before displaying to the user
                property.SetValue(managementPeriodDto, bindingValue);
            }
        }

        return managementPeriodDto;
    }

    public IManagementPeriodDto TransferManagementPeriodDtoToSystem(IManagementPeriodDto managementPeriodDto, ManagementPeriod managementPeriod)
    {
        // Create a copy of the DTO since we don't want to change values on the original that is still bound to the GUI
        var copy = _managementPeriodFactory.CreateDtoFromDtoTemplate(managementPeriodDto);

        var propertyConverter = new PropertyConverter<IManagementPeriodDto>(copy);

        // Get all properties that might need to be converted to imperial units before being shown to the user
        if (propertyConverter.PropertyInfos != null)
        {
            foreach (var property in propertyConverter.PropertyInfos)
            {
                // Convert the value from imperial to metric as needed (no conversion will occur if display is using metric)
                var bindingValue = propertyConverter.GetSystemValueFromBinding(property, _unitsOfMeasurementCalculator.GetUnitsOfMeasurement());

                // Set the value on the copy of the DTO
                property.SetValue(copy, bindingValue);
            }
        }

        // Map values from the copy of the DTO to the internal system object
        if (copy is ManagementPeriodDto managementPeriodDtoCopy)
        {
            PropertyMapper.CopyTo(managementPeriodDtoCopy, managementPeriod);
        }

        return copy;
    }

    public IReadOnlyCollection<BeddingMaterialType> GetValidBeddingMaterialTypes(AnimalType animalType)
    {
        var result = new List<BeddingMaterialType>();
        result.Add(BeddingMaterialType.None);

        var category = animalType.GetComponentCategoryFromAnimalType();
        switch (category)
        {
            case ComponentCategory.BeefProduction:
                {
                    result.Add(BeddingMaterialType.WoodChip);
                    result.Add(BeddingMaterialType.Straw);
                }
                break;

            case ComponentCategory.Dairy:
                {
                    result.Add(BeddingMaterialType.Sand);
                    result.Add(BeddingMaterialType.SeparatedManureSolid);
                    result.Add(BeddingMaterialType.StrawLong);
                    result.Add(BeddingMaterialType.StrawChopped);
                    result.Add(BeddingMaterialType.Shavings);
                    result.Add(BeddingMaterialType.Sawdust);
                }
                break;

            case ComponentCategory.Swine:
                {
                    result.Add(BeddingMaterialType.StrawLong);
                    result.Add(BeddingMaterialType.StrawChopped);
                }
                break;

            case ComponentCategory.Sheep:
                {
                    result.Add(BeddingMaterialType.Straw);
                    result.Add(BeddingMaterialType.Shavings);
                }
                break;

            case ComponentCategory.Poultry:
                {
                    result.Add(BeddingMaterialType.Straw);
                    result.Add(BeddingMaterialType.Shavings);
                    result.Add(BeddingMaterialType.Sawdust);
                }
                break;

            case ComponentCategory.OtherLivestock:
                {
                    result.Add(BeddingMaterialType.Straw);
                }
                break;
        }

        return result;
    }

    #endregion
}