using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Models;
using Microsoft.Extensions.Logging;
using Prism.Ioc;

namespace H.Core.Services;

public abstract class ComponentServiceBase : IComponentService
{
    #region Fields

    protected ILogger Logger = null!;
    protected IContainerProvider ContainerProvider = null!;
    protected IUnitsOfMeasurementCalculator UnitsOfMeasurementCalculator = null!;

    #endregion

    protected ComponentServiceBase(ILogger logger, IContainerProvider containerProvider, IUnitsOfMeasurementCalculator unitsOfMeasurementCalculator)
    {
        if (unitsOfMeasurementCalculator != null)
        {
            UnitsOfMeasurementCalculator = unitsOfMeasurementCalculator; 
        }
        else
        {
            throw new ArgumentNullException(nameof(unitsOfMeasurementCalculator));
        }

        if (containerProvider != null)
        {
            ContainerProvider = containerProvider; 
        }
        else
        {
            throw new ArgumentNullException(nameof(containerProvider));
        }

        if (logger != null)
        {
            Logger = logger; 
        }
        else
        {
            throw new ArgumentNullException(nameof(logger));
        }
    }

    protected ComponentServiceBase(ILogger logger, IContainerProvider containerProvider)
    {
        if (containerProvider != null)
        {
            ContainerProvider = containerProvider;
        }
        else
        {
            throw new ArgumentNullException(nameof(containerProvider));
        }

        if (logger != null)
        {
            Logger = logger;
        }
        else
        {
            throw new ArgumentNullException(nameof(logger));
        }
    }

    protected ComponentServiceBase(ILogger logger)
    {
        if (logger != null)
        {
            Logger = logger;
        }
        else
        {
            throw new ArgumentNullException(nameof(logger));
        }
    }

    protected ComponentServiceBase()
    {
    }

    #region Public Methods
    
    public string GetUniqueComponentName(Farm farm, ComponentBase component)
    {
        // Handle null farm or null components collection - return base name with timestamp for uniqueness
        if (farm?.Components == null)
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
            return $"{component.ComponentNameDisplayString}_{timestamp}";
        }

        var i = 2;

        // Don't add number to component name at first (i.e. just use "Cow-Calf" and not "Cow-Calf #1").
        var proposedName = component.ComponentNameDisplayString;

        // While the names are the same, try and make a unique name for this component.
        while (farm.Components.Where(x => string.IsNullOrWhiteSpace(x.Name) == false).Any(y => y.Name!.Equals(proposedName)))
        {
            proposedName = component.ComponentNameDisplayString + " #" + (i++);
        }

        return proposedName!;
    }

    public void InitializeComponent(Farm farm, ComponentBase? component)
    {
        if (component is null)
        {
            Logger.LogError($"Called with null {nameof(component)} parameter");

            return;
        }

        if (component.IsInitialized)
        {
            Logger.LogDebug("Component '{ComponentName}' is already initialized, skipping initialization", component.Name);

            return;
        }

        Logger.LogDebug("Initializing component: {ComponentName}", component.Name ?? "(name not set)");

        component.IsInitialized = true;
        component.Name = this.GetUniqueComponentName(farm, component);

        Logger.LogInformation("Successfully initialized component with name: '{ComponentName}'", component.Name);
    }

    #endregion
}