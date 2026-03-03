using H.Core.Factories.Animals.Dairy;

namespace H.Avalonia.Test.ViewModels.ComponentViews.Dairy;

/// <summary>
/// Unit tests for the DairyComponentDto class.
/// Tests validation, property changes, and calculated herd composition.
/// </summary>
[TestClass]
public class DairyComponentDtoTests
{
    #region Fields

    private DairyComponentDto _dto = null!;

    #endregion

    #region Initialization

    [ClassInitialize]
    public static void ClassInitialize(TestContext context)
    {
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _dto = new DairyComponentDto
        {
            Name = "Test Dairy Herd",
            TotalMilkingCows = 100,
            ReplacementRate = 30.0,
            CalvingIntervalMonths = 14,
            DryPeriodDays = 60,
            CalfMortalityRate = 5.0,
            FemaleCalfRatio = 50.0
        };
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_CreatesDto()
    {
        // Arrange & Act
        var dto = new DairyComponentDto();

        // Assert
        Assert.IsNotNull(dto);
    }

    [TestMethod]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var dto = new DairyComponentDto();

        // Assert
        Assert.AreEqual(100, dto.TotalMilkingCows);
        Assert.AreEqual(30.0, dto.ReplacementRate);
        Assert.AreEqual(14, dto.CalvingIntervalMonths);
        Assert.AreEqual(60, dto.DryPeriodDays);
        Assert.AreEqual(5.0, dto.CalfMortalityRate);
        Assert.AreEqual(50.0, dto.FemaleCalfRatio);
        
        // Assert - Default Production Values
        Assert.AreEqual(25.0, dto.DefaultMilkProduction);
        Assert.AreEqual(3.9, dto.DefaultMilkFatContent);
        Assert.AreEqual(3.2, dto.DefaultMilkProteinContent);
    }

    [TestMethod]
    public void Constructor_CalculatesInitialHerdComposition()
    {
        // Arrange & Act
        var dto = new DairyComponentDto();

        // Assert
        Assert.IsTrue(dto.CalculatedCalves >= 0);
        Assert.IsTrue(dto.CalculatedHeifers >= 0);
        Assert.IsTrue(dto.CalculatedLactating >= 0);
        Assert.IsTrue(dto.CalculatedDry >= 0);
    }

    #endregion

    #region TotalMilkingCows Validation Tests

    [TestMethod]
    public void TotalMilkingCows_ValidValue_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.TotalMilkingCows = 500;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
        Assert.AreEqual(500, _dto.TotalMilkingCows);
    }

    [TestMethod]
    public void TotalMilkingCows_Zero_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.TotalMilkingCows = 0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.TotalMilkingCows)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Total milking cows must be greater than zero", errors.First());
    }

    [TestMethod]
    public void TotalMilkingCows_Negative_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.TotalMilkingCows = -10;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.TotalMilkingCows)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Total milking cows must be greater than zero", errors.First());
    }

    [TestMethod]
    public void TotalMilkingCows_ExceedsMaximum_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.TotalMilkingCows = 15000;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.TotalMilkingCows)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Total milking cows cannot exceed 10,000", errors.First());
    }

    [TestMethod]
    public void TotalMilkingCows_BoundaryValue_MaximumAllowed_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.TotalMilkingCows = 10000;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
        Assert.AreEqual(10000, _dto.TotalMilkingCows);
    }

    [TestMethod]
    public void TotalMilkingCows_BoundaryValue_One_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.TotalMilkingCows = 1;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
        Assert.AreEqual(1, _dto.TotalMilkingCows);
    }

    #endregion

    #region ReplacementRate Validation Tests

    [TestMethod]
    public void ReplacementRate_ValidValue_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.ReplacementRate = 25.5;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
        Assert.AreEqual(25.5, _dto.ReplacementRate);
    }

    [TestMethod]
    public void ReplacementRate_Negative_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.ReplacementRate = -5.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.ReplacementRate)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Replacement rate cannot be negative", errors.First());
    }

    [TestMethod]
    public void ReplacementRate_ExceedsMaximum_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.ReplacementRate = 150.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.ReplacementRate)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Replacement rate cannot exceed 100%", errors.First());
    }

    [TestMethod]
    public void ReplacementRate_BoundaryValues_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Zero
        _dto.ReplacementRate = 0;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - 100%
        _dto.ReplacementRate = 100.0;
        Assert.IsFalse(_dto.HasErrors);
    }

    #endregion

    #region CalvingIntervalMonths Validation Tests

    [TestMethod]
    public void CalvingIntervalMonths_ValidValue_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.CalvingIntervalMonths = 13;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
        Assert.AreEqual(13, _dto.CalvingIntervalMonths);
    }

    [TestMethod]
    public void CalvingIntervalMonths_TooLow_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.CalvingIntervalMonths = 8;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.CalvingIntervalMonths)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Calving interval must be at least 10 months", errors.First());
    }

    [TestMethod]
    public void CalvingIntervalMonths_TooHigh_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.CalvingIntervalMonths = 30;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.CalvingIntervalMonths)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Calving interval cannot exceed 24 months", errors.First());
    }

    [TestMethod]
    public void CalvingIntervalMonths_BoundaryValues_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Minimum
        _dto.CalvingIntervalMonths = 10;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Maximum
        _dto.CalvingIntervalMonths = 24;
        Assert.IsFalse(_dto.HasErrors);
    }

    #endregion

    #region DryPeriodDays Validation Tests

    [TestMethod]
    public void DryPeriodDays_ValidValue_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DryPeriodDays = 45;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
        Assert.AreEqual(45, _dto.DryPeriodDays);
    }

    [TestMethod]
    public void DryPeriodDays_Negative_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DryPeriodDays = -10;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.DryPeriodDays)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Dry period cannot be negative", errors.First());
    }

    [TestMethod]
    public void DryPeriodDays_ExceedsMaximum_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DryPeriodDays = 150;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.DryPeriodDays)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Dry period cannot exceed 120 days", errors.First());
    }

    [TestMethod]
    public void DryPeriodDays_BoundaryValues_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Zero
        _dto.DryPeriodDays = 0;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Maximum
        _dto.DryPeriodDays = 120;
        Assert.IsFalse(_dto.HasErrors);
    }

    #endregion

    #region CalfMortalityRate Validation Tests

    [TestMethod]
    public void CalfMortalityRate_ValidValue_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.CalfMortalityRate = 3.5;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
        Assert.AreEqual(3.5, _dto.CalfMortalityRate);
    }

    [TestMethod]
    public void CalfMortalityRate_Negative_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.CalfMortalityRate = -2.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.CalfMortalityRate)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Calf mortality rate cannot be negative", errors.First());
    }

    [TestMethod]
    public void CalfMortalityRate_ExceedsMaximum_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.CalfMortalityRate = 60.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.CalfMortalityRate)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Calf mortality rate cannot exceed 50%", errors.First());
    }

    [TestMethod]
    public void CalfMortalityRate_BoundaryValues_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Zero
        _dto.CalfMortalityRate = 0;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Maximum
        _dto.CalfMortalityRate = 50.0;
        Assert.IsFalse(_dto.HasErrors);
    }

    #endregion

    #region FemaleCalfRatio Validation Tests

    [TestMethod]
    public void FemaleCalfRatio_ValidValue_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.FemaleCalfRatio = 48.5;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
        Assert.AreEqual(48.5, _dto.FemaleCalfRatio);
    }

    [TestMethod]
    public void FemaleCalfRatio_Negative_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.FemaleCalfRatio = -10.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.FemaleCalfRatio)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Female calf ratio cannot be negative", errors.First());
    }

    [TestMethod]
    public void FemaleCalfRatio_ExceedsMaximum_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.FemaleCalfRatio = 120.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.FemaleCalfRatio)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Female calf ratio cannot exceed 100%", errors.First());
    }

    [TestMethod]
    public void FemaleCalfRatio_BoundaryValues_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Zero
        _dto.FemaleCalfRatio = 0;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Maximum
        _dto.FemaleCalfRatio = 100.0;
        Assert.IsFalse(_dto.HasErrors);
    }

    #endregion

    #region Calculated Herd Composition Tests

    [TestMethod]
    public void CalculatedHerdComposition_InitialValues_AreCorrect()
    {
        // Arrange & Act
        var dto = new DairyComponentDto
        {
            TotalMilkingCows = 100,
            ReplacementRate = 30.0,
            DryPeriodDays = 60,
            CalfMortalityRate = 5.0,
            FemaleCalfRatio = 50.0
        };

        // Assert
        Assert.IsTrue(dto.CalculatedHeifers > 0, "Should have calculated heifers");
        Assert.IsTrue(dto.CalculatedDry > 0, "Should have calculated dry cows");
        Assert.IsTrue(dto.CalculatedLactating > 0, "Should have calculated lactating cows");
        Assert.IsTrue(dto.CalculatedCalves > 0, "Should have calculated calves");
    }

    [TestMethod]
    public void CalculatedHerdComposition_LactatingPlusDry_DoesNotExceedTotal()
    {
        // Arrange & Act
        _dto.TotalMilkingCows = 200;

        // Assert
        var total = _dto.CalculatedLactating + _dto.CalculatedDry;
        Assert.IsTrue(total <= _dto.TotalMilkingCows,
            $"Lactating ({_dto.CalculatedLactating}) + Dry ({_dto.CalculatedDry}) should not exceed Total ({_dto.TotalMilkingCows})");
    }

    [TestMethod]
    public void CalculatedHerdComposition_RecalculatesWhenTotalMilkingCowsChanges()
    {
        // Arrange
        var initialLactating = _dto.CalculatedLactating;
        var initialDry = _dto.CalculatedDry;
        var initialHeifers = _dto.CalculatedHeifers;

        // Act
        _dto.TotalMilkingCows = 200;

        // Assert
        Assert.AreNotEqual(initialLactating, _dto.CalculatedLactating, "Lactating should recalculate");
        Assert.AreNotEqual(initialDry, _dto.CalculatedDry, "Dry should recalculate");
        Assert.AreNotEqual(initialHeifers, _dto.CalculatedHeifers, "Heifers should recalculate");
    }

    [TestMethod]
    public void CalculatedHerdComposition_RecalculatesWhenReplacementRateChanges()
    {
        // Arrange
        var initialHeifers = _dto.CalculatedHeifers;

        // Act
        _dto.ReplacementRate = 40.0;

        // Assert
        Assert.AreNotEqual(initialHeifers, _dto.CalculatedHeifers,
            "Heifers should recalculate when replacement rate changes");
    }

    [TestMethod]
    public void CalculatedHerdComposition_RecalculatesWhenDryPeriodChanges()
    {
        // Arrange
        var initialDry = _dto.CalculatedDry;
        var initialLactating = _dto.CalculatedLactating;

        // Act
        _dto.DryPeriodDays = 90;

        // Assert
        Assert.AreNotEqual(initialDry, _dto.CalculatedDry, "Dry cows should recalculate");
        Assert.AreNotEqual(initialLactating, _dto.CalculatedLactating, "Lactating should recalculate");
    }

    [TestMethod]
    public void CalculatedHerdComposition_RecalculatesWhenCalfMortalityRateChanges()
    {
        // Arrange
        var initialCalves = _dto.CalculatedCalves;

        // Act
        _dto.CalfMortalityRate = 10.0;

        // Assert
        Assert.AreNotEqual(initialCalves, _dto.CalculatedCalves,
            "Calves should recalculate when mortality rate changes");
    }

    [TestMethod]
    public void CalculatedHerdComposition_RecalculatesWhenFemaleCalfRatioChanges()
    {
        // Arrange
        var initialCalves = _dto.CalculatedCalves;

        // Act
        _dto.FemaleCalfRatio = 60.0;

        // Assert
        Assert.AreNotEqual(initialCalves, _dto.CalculatedCalves,
            "Calves should recalculate when female calf ratio changes");
    }

    [TestMethod]
    public void CalculatedHerdComposition_AllValuesNonNegative()
    {
        // Arrange & Act
        _dto.TotalMilkingCows = 50;
        _dto.DryPeriodDays = 30;
        _dto.ReplacementRate = 20.0;
        _dto.CalfMortalityRate = 10.0;
        _dto.FemaleCalfRatio = 45.0;

        // Assert
        Assert.IsTrue(_dto.CalculatedCalves >= 0, "Calculated calves should be non-negative");
        Assert.IsTrue(_dto.CalculatedHeifers >= 0, "Calculated heifers should be non-negative");
        Assert.IsTrue(_dto.CalculatedLactating >= 0, "Calculated lactating should be non-negative");
        Assert.IsTrue(_dto.CalculatedDry >= 0, "Calculated dry should be non-negative");
    }

    [TestMethod]
    public void CalculatedHerdComposition_WithZeroDryPeriod_AllCowsAreLactating()
    {
        // Arrange & Act
        _dto.DryPeriodDays = 0;

        // Assert
        Assert.AreEqual(0, _dto.CalculatedDry, "With zero dry period, no cows should be dry");
        Assert.AreEqual(_dto.TotalMilkingCows, _dto.CalculatedLactating,
            "With zero dry period, all cows should be lactating");
    }

    [TestMethod]
    public void CalculatedHerdComposition_WithZeroReplacementRate_NoHeifers()
    {
        // Arrange & Act
        _dto.ReplacementRate = 0;

        // Assert
        Assert.AreEqual(0, _dto.CalculatedHeifers, "With zero replacement rate, there should be no heifers");
    }

    [TestMethod]
    public void CalculatedHerdComposition_WithMaxMortality_FewerCalves()
    {
        // Arrange
        _dto.CalfMortalityRate = 10.0;
        var calvesWithLowMortality = _dto.CalculatedCalves;

        // Act
        _dto.CalfMortalityRate = 40.0;

        // Assert
        Assert.IsTrue(_dto.CalculatedCalves < calvesWithLowMortality,
            "Higher mortality should result in fewer surviving calves");
    }

    #endregion

    #region Property Change Notification Tests

    [TestMethod]
    public void PropertyChanged_RaisedWhenTotalMilkingCowsChanges()
    {
        // Arrange
        bool propertyChangedRaised = false;
        _dto.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(_dto.TotalMilkingCows))
            {
                propertyChangedRaised = true;
            }
        };

        // Act
        _dto.TotalMilkingCows = 150;

        // Assert
        Assert.IsTrue(propertyChangedRaised);
    }

    [TestMethod]
    public void PropertyChanged_RaisedForCalculatedValues()
    {
        // Arrange
        var propertiesChanged = new List<string>();
        _dto.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName != null)
            {
                propertiesChanged.Add(args.PropertyName);
            }
        };

        // Act
        _dto.TotalMilkingCows = 150;

        // Assert
        Assert.IsTrue(propertiesChanged.Contains(nameof(_dto.CalculatedLactating)),
            "CalculatedLactating should raise PropertyChanged");
        Assert.IsTrue(propertiesChanged.Contains(nameof(_dto.CalculatedDry)),
            "CalculatedDry should raise PropertyChanged");
    }

    #endregion

    #region Edge Case Tests

    [TestMethod]
    public void EdgeCase_MinimumViableHerd_OneAnimal()
    {
        // Arrange & Act
        var dto = new DairyComponentDto
        {
            TotalMilkingCows = 1,
            ReplacementRate = 0,
            DryPeriodDays = 0
        };

        // Assert
        Assert.IsFalse(dto.HasErrors);
        Assert.AreEqual(1, dto.CalculatedLactating);
        Assert.AreEqual(0, dto.CalculatedDry);
    }

    [TestMethod]
    public void EdgeCase_MaximumHerd_10000Animals()
    {
        // Arrange & Act
        var dto = new DairyComponentDto
        {
            TotalMilkingCows = 10000,
            ReplacementRate = 30.0
        };

        // Assert
        Assert.IsFalse(dto.HasErrors);
        Assert.IsTrue(dto.CalculatedHeifers > 0);
        Assert.IsTrue(dto.CalculatedLactating > 0);
    }

    [TestMethod]
    public void EdgeCase_AllParametersAtMaximum()
    {
        // Arrange & Act
        var dto = new DairyComponentDto
        {
            TotalMilkingCows = 10000,
            ReplacementRate = 100.0,
            CalvingIntervalMonths = 24,
            DryPeriodDays = 120,
            CalfMortalityRate = 50.0,
            FemaleCalfRatio = 100.0
        };

        // Assert
        Assert.IsFalse(dto.HasErrors);
        Assert.IsTrue(dto.CalculatedCalves >= 0);
        Assert.IsTrue(dto.CalculatedHeifers >= 0);
        Assert.IsTrue(dto.CalculatedLactating >= 0);
        Assert.IsTrue(dto.CalculatedDry >= 0);
    }

    #endregion

    #region DefaultMilkProduction Validation Tests

    [TestMethod]
    public void DefaultMilkProduction_ValidValue_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DefaultMilkProduction = 30.0;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
        Assert.AreEqual(30.0, _dto.DefaultMilkProduction);
    }

    [TestMethod]
    public void DefaultMilkProduction_Negative_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DefaultMilkProduction = -5.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.DefaultMilkProduction)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Milk production cannot be negative", errors.First());
    }

    [TestMethod]
    public void DefaultMilkProduction_ExceedsMaximum_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DefaultMilkProduction = 150.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.DefaultMilkProduction)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Milk production cannot exceed 100 kg/day", errors.First());
    }

    [TestMethod]
    public void DefaultMilkProduction_BoundaryValues_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Zero
        _dto.DefaultMilkProduction = 0;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Maximum
        _dto.DefaultMilkProduction = 100.0;
        Assert.IsFalse(_dto.HasErrors);
    }

    [TestMethod]
    public void DefaultMilkProduction_TypicalValues_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Low production
        _dto.DefaultMilkProduction = 15.0;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Average production
        _dto.DefaultMilkProduction = 25.0;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - High production
        _dto.DefaultMilkProduction = 40.0;
        Assert.IsFalse(_dto.HasErrors);
    }

    #endregion

    #region DefaultMilkFatContent Validation Tests

    [TestMethod]
    public void DefaultMilkFatContent_ValidValue_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DefaultMilkFatContent = 4.2;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
        Assert.AreEqual(4.2, _dto.DefaultMilkFatContent);
    }

    [TestMethod]
    public void DefaultMilkFatContent_Negative_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DefaultMilkFatContent = -1.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.DefaultMilkFatContent)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Milk fat content cannot be negative", errors.First());
    }

    [TestMethod]
    public void DefaultMilkFatContent_ExceedsMaximum_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DefaultMilkFatContent = 15.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.DefaultMilkFatContent)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Milk fat content cannot exceed 10%", errors.First());
    }

    [TestMethod]
    public void DefaultMilkFatContent_BoundaryValues_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Zero
        _dto.DefaultMilkFatContent = 0;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Maximum
        _dto.DefaultMilkFatContent = 10.0;
        Assert.IsFalse(_dto.HasErrors);
    }

    [TestMethod]
    public void DefaultMilkFatContent_TypicalBreedValues_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Holstein
        _dto.DefaultMilkFatContent = 3.7;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Jersey
        _dto.DefaultMilkFatContent = 4.9;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Guernsey
        _dto.DefaultMilkFatContent = 4.5;
        Assert.IsFalse(_dto.HasErrors);
    }

    #endregion

    #region DefaultMilkProteinContent Validation Tests

    [TestMethod]
    public void DefaultMilkProteinContent_ValidValue_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DefaultMilkProteinContent = 3.5;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
        Assert.AreEqual(3.5, _dto.DefaultMilkProteinContent);
    }

    [TestMethod]
    public void DefaultMilkProteinContent_Negative_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DefaultMilkProteinContent = -1.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.DefaultMilkProteinContent)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Milk protein content cannot be negative", errors.First());
    }

    [TestMethod]
    public void DefaultMilkProteinContent_ExceedsMaximum_HasError()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act
        _dto.DefaultMilkProteinContent = 12.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        var errors = _dto.GetErrors(nameof(_dto.DefaultMilkProteinContent)) as IEnumerable<string>;
        Assert.IsNotNull(errors);
        Assert.AreEqual("Milk protein content cannot exceed 10%", errors.First());
    }

    [TestMethod]
    public void DefaultMilkProteinContent_BoundaryValues_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Zero
        _dto.DefaultMilkProteinContent = 0;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Maximum
        _dto.DefaultMilkProteinContent = 10.0;
        Assert.IsFalse(_dto.HasErrors);
    }

    [TestMethod]
    public void DefaultMilkProteinContent_TypicalBreedValues_NoErrors()
    {
        // Arrange
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Holstein
        _dto.DefaultMilkProteinContent = 3.1;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Jersey
        _dto.DefaultMilkProteinContent = 3.8;
        Assert.IsFalse(_dto.HasErrors);

        // Act & Assert - Guernsey
        _dto.DefaultMilkProteinContent = 3.5;
        Assert.IsFalse(_dto.HasErrors);
    }

    #endregion

    #region Combined Default Production Tests

    [TestMethod]
    public void DefaultProduction_AllValid_NoErrors()
    {
        // Arrange & Act - Typical Holstein operation
        _dto.DefaultMilkProduction = 28.0;
        _dto.DefaultMilkFatContent = 3.7;
        _dto.DefaultMilkProteinContent = 3.1;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
    }

    [TestMethod]
    public void DefaultProduction_HighProducingJerseyHerd_NoErrors()
    {
        // Arrange & Act - High producing Jersey herd
        _dto.DefaultMilkProduction = 22.0;
        _dto.DefaultMilkFatContent = 5.0;
        _dto.DefaultMilkProteinContent = 3.9;

        // Assert
        Assert.IsFalse(_dto.HasErrors);
    }

    [TestMethod]
    public void DefaultProduction_MultipleInvalid_HasMultipleErrors()
    {
        // Arrange & Act
        _dto.DefaultMilkProduction = -5.0;
        _dto.DefaultMilkFatContent = 15.0;
        _dto.DefaultMilkProteinContent = -2.0;

        // Assert
        Assert.IsTrue(_dto.HasErrors);
        
        var milkErrors = _dto.GetErrors(nameof(_dto.DefaultMilkProduction)) as IEnumerable<string>;
        Assert.IsNotNull(milkErrors);
        
        var fatErrors = _dto.GetErrors(nameof(_dto.DefaultMilkFatContent)) as IEnumerable<string>;
        Assert.IsNotNull(fatErrors);
        
        var proteinErrors = _dto.GetErrors(nameof(_dto.DefaultMilkProteinContent)) as IEnumerable<string>;
        Assert.IsNotNull(proteinErrors);
    }

    #endregion
}

