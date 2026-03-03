using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Models.LandManagement.Fields;
using H.Core.Models.LandManagement.Rotation;
using H.Core.Services.Animals;
using H.Core.Services.LandManagement.Fields;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;

namespace H.Core.Services;

public class ComponentInitializationService : IComponentInitializationService
{
    #region Fields

    private readonly IFieldComponentService _fieldComponentService;
    private readonly IStorageService _storageService;
    private IAnimalComponentService _animalComponentService;
    private IRotationComponentService _rotationComponentService;
    private ILogger _logger;

    #endregion

    #region Constructors

    public ComponentInitializationService(
        ILogger logger,
        IStorageService storageService, 
        IFieldComponentService fieldComponentService, 
        IAnimalComponentService animalComponentService,
        IRotationComponentService rotationComponentService)
    {
        if (logger != null)
        {
            _logger = logger; 
        }
        else
        {
            throw new ArgumentNullException(nameof(logger));
        }

        if (rotationComponentService != null)
        {
            _rotationComponentService = rotationComponentService;
        }
        else
        {
            throw new ArgumentNullException(nameof(rotationComponentService));
        }

        if (animalComponentService != null)
        {
            _animalComponentService = animalComponentService;
        }
        else
        {
            throw new ArgumentNullException(nameof(animalComponentService));
        }

        if (storageService != null)
        {
            _storageService = storageService;
        }
        else
        {
            throw new ArgumentNullException(nameof(storageService));
        }

        if (fieldComponentService != null)
        {
            _fieldComponentService = fieldComponentService;
        }
        else
        {
            throw new ArgumentNullException(nameof(fieldComponentService));
        }
    }

    #endregion

    #region Public Methods

    public void Initialize(ComponentBase componentBase)
    {
        var activeFarm = _storageService.GetActiveFarm();

        if (componentBase is FieldSystemComponent fieldSystemComponent)
        {
            _fieldComponentService.InitializeComponent(activeFarm, fieldSystemComponent);
        }
        else if (componentBase is AnimalComponentBase animalComponentBase)
        {
            _animalComponentService.InitializeComponent(activeFarm, animalComponentBase);
        }
        else if (componentBase is RotationComponent rotationComponent)
        {
            _rotationComponentService.InitializeComponent(activeFarm, rotationComponent);
        }
        else
        {
            // Fallback for component types without a dedicated service (e.g. Shelterbelt, Anaerobic Digestion).
            // Uses the base InitializeComponent which assigns a unique Name and marks the component as initialized.
            ((IComponentService)_fieldComponentService).InitializeComponent(activeFarm, componentBase);
        }
    } 

    #endregion
}