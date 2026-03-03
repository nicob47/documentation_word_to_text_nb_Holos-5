using H.Core.Factories;
using H.Core.Factories.Crops;
using H.Core.Factories.Fields;
using H.Core.Factories.Rotations;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Models.LandManagement.Rotation;
using H.Core.Services.Animals;
using Microsoft.Extensions.Logging;

namespace H.Core.Services.LandManagement.Fields;

public class RotationComponentService : ComponentServiceBase, IRotationComponentService
{
    #region Fields

    private IFieldFactory _fieldFactory;
    private ICropFactory _cropFactory;
    private readonly ITransferService<RotationComponent, RotationComponentDto> _rotationTransferService;

    #endregion

    #region Constructors

    public RotationComponentService(
        ILogger logger, 
        IFieldFactory fieldFactory, 
        ICropFactory cropFactory,
        ITransferService<RotationComponent, RotationComponentDto> rotationTransferService) : base(logger)
    {
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

        if (rotationTransferService != null)
        {
            _rotationTransferService = rotationTransferService;
        }
        else
        {
            throw new ArgumentNullException(nameof(rotationTransferService));
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Applies default initialization for a new <see cref="RotationComponent"/> being added to a <see cref="Farm"/>.
    /// Ensures a unique name and marks the component as initialized. No-ops if already initialized.
    /// </summary>
    /// <param name="farm">The target farm.</param>
    /// <param name="rotationComponent">The rotation component to initialize.</param>
    /// <exception cref="ArgumentNullException">Thrown when <see cref="rotationComponent"/> is null.</exception>
    public void InitializeComponent(Farm farm, RotationComponent rotationComponent)
    {
        base.InitializeComponent(farm, rotationComponent);

        if (rotationComponent is null)
        {
            return;
        }

        
        rotationComponent.EndYear = DateTime.Now.Year;
        rotationComponent.StartYear = rotationComponent.EndYear - 10;
    }

    /// <summary>
    /// Creates a new <see cref="IRotationComponentDto"/> from a <see cref="RotationComponent"/> for UI binding.
    /// </summary>
    /// <param name="template">The source rotation component.</param>
    /// <returns>A DTO suitable for binding in the view.</returns>
    public IRotationComponentDto TransferToRotationComponentDto(RotationComponent template)
    {
        var rotationComponentDto = _rotationTransferService.TransferDomainObjectToDto(template);

        return rotationComponentDto;
    }

    public RotationComponent TransferRotationDtoToSystem(RotationComponentDto rotationDto, RotationComponent rotationComponent)
    {
        return _rotationTransferService.TransferDtoToDomainObject(rotationDto, rotationComponent);
    }

    #endregion
}