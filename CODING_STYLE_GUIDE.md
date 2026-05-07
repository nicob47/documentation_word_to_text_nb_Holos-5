# Holos .NET Coding Style Guide

## Overview

This document outlines the coding conventions and style guidelines used throughout the Holos application codebase. Following these conventions ensures consistency, maintainability, and readability across the entire project.

## Table of Contents

1. [Naming Conventions](#naming-conventions)
2. [Code Organization](#code-organization)
3. [Documentation Standards](#documentation-standards)
4. [Language Conventions](#language-conventions)
5. [Error Handling](#error-handling)
6. [Testing Conventions](#testing-conventions)
7. [Framework-Specific Guidelines](#framework-specific-guidelines)

---

## Naming Conventions

### Classes and Interfaces

**Classes**
- Use **PascalCase** for all class names
- Use descriptive, meaningful names that indicate purpose
- Include category/type suffix when appropriate

```csharp
// Good
public class ContainerRegistrationService
public class SheepResultsService
public class Table_30_Default_Bedding_Material_Composition_Provider
public class UserSettingsDTO

// Avoid
public class service
public class helper
public class utils
```

**Interfaces**
- Prefix with **"I"** followed by PascalCase
- Use descriptive names indicating the contract

```csharp
// Good
public interface IAnimalResultsService
public interface ISheepResultsService
public interface ITimePeriodItem
public interface IN2OEmissionFactorCalculator

// Avoid
public interface AnimalService
public interface Service
```

### Methods

**Public and Protected Methods**
- Use **PascalCase** for all method names
- Use descriptive verb-noun combinations
- Include calculation equation numbers in XML documentation when applicable

```csharp
// Good
public double CalculateGrossEnergyIntake(double dryMatterIntake)
public void RegisterTypes(IContainerRegistry containerRegistry)
protected override void OnInitialized()
private void SetUpLogging(IContainerRegistry containerRegistry)

// Avoid
public double calculate(double value)
public void setup()
```

**Calculation Methods**
- Follow equation naming pattern when implementing scientific calculations
- Include equation reference in method name when applicable

```csharp
// Good - Scientific calculations with equation references
/// <summary>
/// Equation 3.1.1-3
/// </summary>
public double CalculateNetEnergyForMaintenance(double maintenanceCoefficient, double weight)

/// <summary>
/// Equation 4.2.1-1
/// </summary>
public double CalculateProteinIntake(double grossEnergyIntake, double crudeProtein)
```

### Properties and Fields

**Properties**
- Use **PascalCase** for public properties
- Use descriptive names indicating what the property represents

```csharp
// Good
public string ComponentName { get; set; }
public DateTime ManagementPeriodStartDate { get; set; }
public double CustomN2OEmissionFactor { get; set; }
public bool CircumferenceGenerationOverriden { get; set; }
```

**Fields**
- Use **camelCase** with underscore prefix for private fields
- Use **PascalCase** for public/protected fields (rare cases)
- Use **UPPER_CASE** for constants

```csharp
// Good - Private fields
private readonly IContainerProvider _containerProvider;
private readonly ILogger _logger;
private bool _circumferenceGenerationOverriden;
private List<List<double>> _table;

// Good - Constants
private const double DieselConversion = 70;
public const double CarbonConcentration = 0.45;

// Good - Static readonly for complex constants
private static readonly DietProvider _dietProvider;
protected readonly Table_43_Beef_Dairy_Default_Emission_Factors_Provider _defaultEmissionFactorsProvider;
```

### Variables and Parameters

**Local Variables**
- Use **camelCase** for local variables
- Use descriptive names, avoid abbreviations

```csharp
// Good
var animalGroupEmissionResults = new List<AnimalGroupEmissionResults>();
var registrationService = new ContainerRegistrationService(Container, logger);
double dailyAverageOutdoorTemperature = farm.ClimateData.GetMeanTemperatureForDay(dateTime);

// Avoid
var results = new List<AnimalGroupEmissionResults>();
var svc = new ContainerRegistrationService(Container, logger);
double temp = farm.ClimateData.GetMeanTemperatureForDay(dateTime);
```

**Method Parameters**
- Use **camelCase** for parameter names
- Use descriptive names that clearly indicate the parameter's purpose

```csharp
// Good
public double CalculateNetEnergyForMaintenance(
    double maintenanceCoefficient, 
    double weight)

public void InitializeViewModel(ComponentBase component)

protected override void RegisterTypes(IContainerRegistry containerRegistry)

// Avoid
public double Calculate(double coeff, double w)
public void Init(ComponentBase c)
```

### Enums

- Use **PascalCase** for enum names and values
- Use descriptive names for both the enum and its values

```csharp
// Good
public enum AnimalType
{
    NotSelected,
    Beef,
    Dairy,
    Sheep,
    BeefBackgrounder,
    ChickenHens
}

public enum CarbonModellingStrategies
{
    IPCCTier2,
    ICBMModel
}
```

---

## Code Organization

### File Structure

**Class Organization**
Follow this order within class files:

```csharp
public class ExampleClass
{
    #region Fields
    // Private fields first, then protected, then public
    #endregion

    #region Constructors
    // Constructors in order of complexity (parameterless first)
    #endregion

    #region Properties
    // Properties grouped by visibility
    #endregion

    #region Public Methods
    // Public methods
    #endregion

    #region Protected Methods
    // Protected methods
    #endregion

    #region Private Methods
    // Private methods
    #endregion

    #region Equations
    // Scientific calculation methods with equation references
    #endregion

    #region Event Handlers
    // Event handling methods
    #endregion
}
```

**Using Statements**
- Group using statements logically
- System namespaces first, then third-party, then project namespaces
- Use alias for disambiguation when needed

```csharp
// Good
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using H.Core.Enumerations;
using H.Core.Models;
using KmlHelpers = H.Avalonia.Infrastructure.KmlHelpers;
```

### Region Usage

Use regions to organize code sections logically:

```csharp
#region Fields
#region Constructors  
#region Properties
#region Public Methods
#region Protected Methods
#region Private Methods
#region Equations
#region Event Handlers
#region Initialization
#region Tests  // For test classes
```

---

## Documentation Standards

### XML Documentation

**Class Documentation**
```csharp
/// <summary>
/// Service responsible for registering all dependency injection container types
/// </summary>
public class ContainerRegistrationService
```

**Method Documentation**
```csharp
/// <summary>
/// Registers all dependency injection services and views with comprehensive error handling.
/// Sets up logging first, then delegates to ContainerRegistrationService for organized registration.
/// </summary>
/// <param name="containerRegistry">The container registry to register types with</param>
protected override void RegisterTypes(IContainerRegistry containerRegistry)
```

**Equation Documentation**
```csharp
/// <summary>
/// Equation 3.1.1-3
/// Equation 3.2.1-2
/// Equation 3.3.1-3
/// </summary>
/// <param name="maintenanceCoefficient">Maintenance coefficient � adjusted for temperature (MJ day^-1 kg?�)</param>
/// <param name="weight">Average weight (kg head^-1)</param>
/// <returns>Net energy for maintenance (MJ head^-1 day^-1)</returns>
public double CalculateNetEnergyForMaintenance(double maintenanceCoefficient, double weight)
```

### Comments

**Inline Comments**
- Use sparingly, only when code intent is not clear
- Prefer self-documenting code over comments

```csharp
// Good - Explains complex business logic
// V5 object
containerRegistry.RegisterSingleton<Storage>();

// V4 object  
containerRegistry.RegisterSingleton<IStorage, H.Core.Storage>();

// Good - Explains non-obvious behavior
if (temperature > 20)
{
    temperature = 20; // Upper limit = 20, temperatures above 20�C use 20�C
}

// Avoid - States the obvious
var count = items.Count(); // Get the count of items
```

---

## Language Conventions

### Variable Declarations

**Use `var` appropriately**
```csharp
// Good - Type is obvious from right side
var registrationService = new ContainerRegistrationService(Container, logger);
var animalGroupEmissionResults = new List<AnimalGroupEmissionResults>();

// Good - Explicit type when not obvious
double temperature = farm.ClimateData.GetMeanTemperatureForDay(dateTime);
string componentName = component?.Name ?? "Unknown";

// Avoid - var when type is not obvious
var result = SomeMethodReturningUnknownType();
```

### Method Structure

**Parameter Validation**
```csharp
public ViewModelBase(IStorageService storageService)
{
    if (storageService != null)
    {
        this.StorageService = storageService;
        this.StorageService.Storage.ApplicationData.GlobalSettings.PropertyChanged += GlobalSettingsPropertyChanged;
    }
    else
    {
        throw new ArgumentNullException(nameof(storageService));
    }
}
```

**Guard Clauses**
```csharp
public double CalculateValue(double input)
{
    if (Math.Abs(input) < double.Epsilon)
    {
        return 0;
    }

    // Main calculation logic here
    return result;
}
```

### Property Patterns

**Auto-implemented Properties**
```csharp
// Good - Simple properties
public string Name { get; set; }
public DateTime StartDate { get; set; }
```

**Properties with Backing Fields**
```csharp
// Good - When validation or side effects needed
private bool _circumferenceGenerationOverriden;

public bool CircumferenceGenerationOverriden
{
    get => _circumferenceGenerationOverriden;
    set
    {
        if (SetProperty(ref _circumferenceGenerationOverriden, value))
        {
            RaisePropertyChanged(nameof(CircumferenceGenerationNotOverriden));
        }
    }
}
```

---

## Error Handling

### Exception Handling

**Logging and Re-throwing**
```csharp
try
{
    var logger = Container.Resolve<ILogger>();
    
    var registrationService = new ContainerRegistrationService(Container, logger);
    registrationService.RegisterAllTypes(containerRegistry);

    logger.LogInformation("All container types registered successfully");
}
catch (Exception ex)
{
    var logger = Container.Resolve<ILogger>();
    logger.LogError(ex, "Failed to register container types: {ErrorMessage}", ex.Message);
    throw;
}
```

**Validation Methods**
```csharp
public bool ValidatePercentage(string propertyName, double value)
{
    if (value < 0 || value > 100)
    {
        AddError(propertyName, H.Core.Properties.Resources.ErrorMustBeBetween0And100);
        return false;
    }
    else
    {
        RemoveError(propertyName);
        return true;
    }
}
```

### Debug Traces

```csharp
Trace.TraceError($"{nameof(ShelterbeltAgTRatioProvider)}.{nameof(GetAboveGroundBiomassTotalTreeBiomassRatio)}" +
    $" unable to get data for the tree species: {treeSpecies} and age: {age}." +
    $" Returning default value of {defaultValue}.");
```

---

## Testing Conventions

### Test Class Structure

```csharp
[TestClass]
public class SheepResultsServiceTest
{
    #region Fields
    
    private SheepResultsService _resultsService; 
    
    #endregion

    #region Initialization

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _resultsService = new SheepResultsService();
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    #region Tests

    /// <summary>
    /// Equation 3.4.1-2
    /// </summary>
    [TestMethod]
    public void CalculateLambRatioReturnsCorrectValue()
    {
        // Arrange
        var numberOfLambs = 104.500;
        var numberOfEwes = 105.875;
        
        // Act
        var result = _resultsService.CalculateLambRatio(numberOfLambs, numberOfEwes);
        
        // Assert
        Assert.AreEqual(0.987012987012987, result);
    }

    #endregion
}
```

### Test Naming

```csharp
// Good - Descriptive test names
[TestMethod]
public void CalculateEntericMethaneEmissionReturnsCorrectValue()

[TestMethod] 
public void GetDataReturnsNonEmptyList()

[TestMethod]
public void ValidatePercentageReturnsFalseForNegativeValue()

// Avoid - Vague names
[TestMethod]
public void Test1()

[TestMethod]
public void CheckMethod()
```

---

## Framework-Specific Guidelines

### Prism MVVM Patterns

**ViewModels**
- Inherit from `ViewModelBase`
- Implement proper constructor dependency injection
- Use `SetProperty` for property change notification

```csharp
public class ExampleViewModel : ViewModelBase
{
    private string _name;
    
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    
    public ExampleViewModel(IStorageService storageService) : base(storageService)
    {
        // Constructor logic
    }
}
```

### Dependency Injection

**Service Registration**
```csharp
// Good - Descriptive registration with logging
_logger.LogDebug("Registering application services");

containerRegistry.RegisterSingleton<IFarmHelper, FarmHelper>();
containerRegistry.RegisterSingleton<IComponentInitializationService, ComponentInitializationService>();

_logger.LogInformation("Successfully registered application services");
```

### Entity Framework / Data Access

**Data Models**
```csharp
public class ExampleData : ModelBase
{
    #region Properties
    
    public CropType CropType { get; set; }
    public double InterceptValue { get; set; }
    public double SlopeValue { get; set; }
    
    #endregion
    
    #region Public Methods
    
    public override string ToString()
    {
        return $"{nameof(CropType)}: {CropType}, {nameof(InterceptValue)}: {InterceptValue}";
    }
    
    #endregion
}
```

### Avalonia XAML Binding Pitfalls

**Never combine `StringFormat` with a two-way / editable binding.** Avalonia
throws `System.NotSupportedException: 'Two way bindings are not supported with
a string format'` at runtime the first time the user commits an edit, because
`StringFormat` is one-way (value → display string) and there's no general way
to parse the formatted text back to the source type.

This affects:
- `<TextBox Text="{Binding X, StringFormat=...}">` — `Text` is two-way by default
- `<NumericUpDown Value="{Binding X, StringFormat=...}">` — `Value` is two-way by default
- `<DataGridTextColumn Binding="{Binding X, StringFormat=...}">` whenever the column is editable (i.e., the parent `<DataGrid>` is not `IsReadOnly="True"` and the column itself is not `IsReadOnly="True"`)
- Any binding with explicit `Mode=TwoWay` plus `StringFormat`

```xml
<!-- WRONG: crashes when the user finishes editing the cell -->
<DataGridTextColumn Header="% in Diet"
                    Binding="{Binding PercentageInDiet, StringFormat='{}{0:F1}', Mode=TwoWay}"
                    IsReadOnly="False"/>

<!-- WRONG: crashes when the user types into the TextBox -->
<TextBox Text="{Binding Quantity, StringFormat='{}{0:F2}'}"/>
```

**Safe alternatives:**

1. **For numeric editing, use `<NumericUpDown FormatString="...">`** — Avalonia's
   `FormatString` attribute is a separate mechanism that survives editing because
   the control parses input as numbers, then re-formats for display. This is the
   idiomatic v5 pattern (see `AnimalsStep2View.axaml`, `ClimateDataView.axaml`).

   ```xml
   <NumericUpDown Value="{Binding Quantity}" FormatString="0.00"/>
   ```

2. **For non-editable display, use a `<TextBlock>`** — TextBlock is read-only by
   nature, so `StringFormat` is always safe.

   ```xml
   <TextBlock Text="{Binding CrudeProtein, StringFormat='Crude Protein: {0:F1}%'}"/>
   ```

3. **For DataGrid display columns, mark them read-only and `StringFormat` is fine.**
   Either set `IsReadOnly="True"` at the `DataGrid` level (covers all columns) or
   on the individual `DataGridTextColumn`.

   ```xml
   <DataGrid IsReadOnly="True">
       <DataGridTextColumn Binding="{Binding TDN, StringFormat='{}{0:F1}'}"/>
   </DataGrid>
   ```

4. **For an editable cell that needs formatted display, drop `StringFormat`** and
   accept the raw value while editing. Computed/summary panels elsewhere can show
   the formatted version.

   ```xml
   <!-- RIGHT: editable, no formatting on the cell itself -->
   <DataGridTextColumn Binding="{Binding PercentageInDiet, Mode=TwoWay}"
                       IsReadOnly="False"/>
   ```

5. **If you genuinely need formatted-while-editing**, write an `IValueConverter`
   that handles both directions (formats on `Convert`, parses on `ConvertBack`)
   and use it instead of `StringFormat`.

---

## Summary

Following these conventions ensures:

- **Consistency** across the entire codebase
- **Readability** for new and existing developers  
- **Maintainability** over the long term
- **Professional standards** in line with .NET best practices

When in doubt, look at existing code in the project for examples, and always prioritize clarity and consistency over personal preferences.