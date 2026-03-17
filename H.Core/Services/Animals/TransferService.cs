using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Converters;
using H.Core.Factories;
using H.Core.Mappers;
using H.Core.Models;
using H.Infrastructure;

namespace H.Core.Services.Animals
{
    /// <summary>
    /// Provides functionality to transfer data between domain model objects and their corresponding Data Transfer Objects (DTOs).
    /// Handles mapping, unit conversion, and property value transformation between internal system models and external-facing DTOs.
    /// </summary>
    /// <typeparam name="TModelBase">The type of the domain model, must inherit from ModelBase.</typeparam>
    /// <typeparam name="TDto">The type of the Data Transfer Object, must implement IDto.</typeparam>
    public class TransferService<TModelBase, TDto> : ITransferService<TModelBase, TDto>
        where TModelBase : ModelBase
        where TDto : IDto, new()
    {
        #region Fields

        private readonly IUnitsOfMeasurementCalculator _unitsOfMeasurementCalculator;
        private readonly IFactory<TDto> _dtoFactory;
        private readonly IModelMapper<TDto, TModelBase> _dtoToModelMapper;
        private readonly IModelMapper<TModelBase, TDto> _modelToDtoMapper;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TransferService{TModelBase, TDto}"/> class.
        /// Configures mapping and unit conversion between model and DTO types.
        /// </summary>
        /// <param name="unitsOfMeasurementCalculator">
        /// Service used to determine the current measurement system (metric or imperial) and to convert numeric values
        /// between systems during transfer operations.
        /// </param>
        /// <param name="dtoFactory">
        /// Factory used to create new DTO instances and DTO copies (for safe mapping without mutating bound instances).
        /// </param>
        /// <param name="dtoToModelMapper">
        /// Mapper that maps from TDto to TModelBase (DTO → Domain).
        /// Must be configured for the type pair (TDto, TModelBase).
        /// Used in <see cref="TransferDtoToDomainObject(TDto, TModelBase)"/>.
        /// </param>
        /// <param name="modelToDtoMapper">
        /// Mapper that maps from TModelBase to TDto (Domain → DTO).
        /// Must be configured for the type pair (TModelBase, TDto).
        /// Used in <see cref="TransferDomainObjectToDto(TModelBase)"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown if required dependencies are null.</exception>
        public TransferService(IUnitsOfMeasurementCalculator unitsOfMeasurementCalculator, IFactory<TDto> dtoFactory, IModelMapper<TDto, TModelBase> dtoToModelMapper, IModelMapper<TModelBase, TDto> modelToDtoMapper)
        {
            if (dtoFactory != null)
            {
                _dtoFactory = dtoFactory;
            }
            else
            {
                throw new ArgumentNullException(nameof(dtoFactory));
            }

            if (unitsOfMeasurementCalculator != null)
            {
                _unitsOfMeasurementCalculator = unitsOfMeasurementCalculator;
            }
            else
            {
                throw new ArgumentNullException(nameof(unitsOfMeasurementCalculator));
            }

            if (modelToDtoMapper != null)
            {
                _modelToDtoMapper = modelToDtoMapper; 
            }
            else
            {
                throw new ArgumentNullException(nameof(modelToDtoMapper));
            }

            if (dtoToModelMapper != null)
            {
                _dtoToModelMapper = dtoToModelMapper;
            }
            else 
            {
                throw new ArgumentNullException(nameof(dtoToModelMapper));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Transfers a domain model object to its corresponding DTO, applying property mapping and unit conversion as needed.
        /// </summary>
        /// <param name="model">The domain model instance to transfer.</param>
        /// <returns>A DTO instance with values mapped and converted for external use.</returns>
        public TDto TransferDomainObjectToDto(TModelBase model)
        {
            var dto = _dtoFactory.CreateDto(new Farm());

            // Use the internal mapper
            PropertyMapper.CopyTo(model, dto);

            // Track which domain object this DTO was created from
            dto.DomainObjectGuid = model.Guid;

            // All numerical values are stored internally as metric values
            var propertyConverter = new PropertyConverter<IDto>(dto);

            // Get all properties that might need to be converted to imperial units before being shown to the user
            foreach (var property in propertyConverter.PropertyInfos)
            {
                // Convert the value from metric to imperial as needed. Note the converter won't convert anything if the display is in metric units
                var bindingValue = propertyConverter.GetBindingValueFromSystem(property, _unitsOfMeasurementCalculator.GetUnitsOfMeasurement());

                // Set the value of the property before displaying to the user
                property.SetValue(dto, bindingValue);
            }

            return dto;
        }

        /// <summary>
        /// Transfers a DTO to its corresponding domain model object, applying property mapping and unit conversion as needed.
        /// </summary>
        /// <param name="dto">The DTO instance to transfer.</param>
        /// <param name="model">The domain model instance to update.</param>
        /// <returns>A new domain model instance with values mapped and converted for internal use.</returns>
        public TModelBase TransferDtoToDomainObject(TDto dto, TModelBase model)
        {
            // Create a copy of the DTO since we don't want to change values on the original that is still bound to the GUI
            var copy = _dtoFactory.CreateDtoFromDtoTemplate(dto);

            // All numerical values are stored internally as metric values
            var propertyConverter = new PropertyConverter<IDto>(copy);

            // Get all properties that might need to be converted to imperial units before being shown to the user
            foreach (var property in propertyConverter.PropertyInfos)
            {
                // Convert the value from imperial to metric as needed (no conversion will occur if display is using metric)
                var bindingValue = propertyConverter.GetSystemValueFromBinding(property, _unitsOfMeasurementCalculator.GetUnitsOfMeasurement());

                // Set the value on the copy of the DTO
                property.SetValue(copy, bindingValue);
            }

            // Map values from the copy of the DTO to the internal system object.
            // Cast to TDto so PropertyMapper sees the concrete type's properties, not just IDto's.
            PropertyMapper.CopyTo((TDto)copy, model);

            return model;
        }

        #endregion
    }
}