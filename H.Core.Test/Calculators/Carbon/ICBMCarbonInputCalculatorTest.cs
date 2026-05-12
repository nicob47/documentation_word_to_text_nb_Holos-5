using System.Collections.ObjectModel;
using H.Core.Calculators.Carbon;
using H.Core.Emissions.Results;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Providers.Climate;
using H.Core.Providers.Evapotranspiration;
using H.Core.Providers.Precipitation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace H.Core.Test.Calculators.Carbon;

[TestClass]
public class ICBMCarbonInputCalculatorTest : UnitTestBase
{
    private ICBMCarbonInputCalculator _sut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _sut = new ICBMCarbonInputCalculator();
    }

    private static Farm MakeFarm(Province province = Province.Manitoba, SoilFunctionalCategory category = SoilFunctionalCategory.Black)
    {
        var farm = new Farm { Province = province };
        farm.DefaultSoilData!.SoilFunctionalCategory = category;
        return farm;
    }

    #region Annuals

    [TestMethod]
    public void CalculateAboveGroundCarbonInputForAnnuals()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.Barley,
            Yield = 1000,
            PercentageOfProductYieldReturnedToSoil = 10,
            PercentageOfStrawReturnedToSoil = 20,
            CarbonConcentration = 0.45,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.CashCrop,
            BiomassCoefficientProduct = 0.451,
            BiomassCoefficientStraw = 0.4,
            PlantCarbonInAgriculturalProduct = 435.6,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: new CropViewItem(),
            farm: MakeFarm(),
            animalResults: new List<AnimalComponentEmissionsResults>());

        // C_ag = C_ptoSoil + C_s
        //      = (C_p * %yieldReturned/100) + (C_p * (R_s/R_p) * %strawReturned/100)
        //      = (435.6 * 0.1) + (435.6 * (0.4/0.451) * 0.2)
        Assert.AreEqual(120.83, currentYearViewItem.AboveGroundCarbonInput, 2);
    }

    [TestMethod]
    public void CalculateBelowGroundCarbonInputForAnnuals()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.Barley,
            Yield = 1000,
            PercentageOfRootsReturnedToSoil = 100,
            CarbonConcentration = 0.45,
            PercentageOfProductYieldReturnedToSoil = 2,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            PerennialStandLength = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.CashCrop,
            BiomassCoefficientProduct = 0.5,
            BiomassCoefficientStraw = 0,
            BiomassCoefficientRoots = 0.4,
            BiomassCoefficientExtraroot = 0.3,
            PlantCarbonInAgriculturalProduct = 403.92,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: null!,
            farm: MakeFarm(),
            animalResults: new List<AnimalComponentEmissionsResults>());

        // C_bg = C_r + C_e = [C_p * (R_r/R_p) * (S_r/100)] + [C_p * (R_e/R_p)]
        //      = [403.92 * 0.8 * 1] + [403.92 * 0.6] = 565.488
        Assert.AreEqual(565.49, currentYearViewItem.BelowGroundCarbonInput, 2);
    }

    #endregion

    #region Perennials

    [TestMethod]
    public void CalculateAboveGroundCarbonInputForPerennials()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.TameMixed,
            Yield = 5000,
            PercentageOfProductYieldReturnedToSoil = 35,
            PercentageOfStrawReturnedToSoil = 0, // Forced to zero for perennials.
            CarbonConcentration = 0.45,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.13,
            HarvestMethod = HarvestMethods.CashCrop,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: new CropViewItem(),
            farm: MakeFarm(),
            animalResults: new List<AnimalComponentEmissionsResults>());

        // C_ag = C_ptoSoil = C_p * (Sp/100) = 3011.538 * 0.35 ~ 1053.04
        Assert.AreEqual(1053.04, currentYearViewItem.AboveGroundCarbonInput, 2);
    }

    [TestMethod]
    public void CalculateAboveGroundCarbonInputForPerennialsWhenPlantCarbonForAgriculturalProductIsUnknownInCurrentYearButKnownInNextYear()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.TameMixed,
            Yield = 0,
            PercentageOfProductYieldReturnedToSoil = 10,
            PercentageOfStrawReturnedToSoil = 0,
            CarbonConcentration = 0.45,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.CashCrop,
            YearInPerennialStand = 1,
        };

        var nextYearViewItem = new CropViewItem
        {
            CropType = CropType.TameMixed,
            Yield = 1000,
            PercentageOfProductYieldReturnedToSoil = 10,
            PercentageOfStrawReturnedToSoil = 0,
            CarbonConcentration = 0.45,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.12,
            MoistureContentOfCropPercentage = 12,
            HarvestMethod = HarvestMethods.CashCrop,
            PlantCarbonInAgriculturalProduct = 435.6,
            YearInPerennialStand = 2,
        };

        var farm = MakeFarm();
        farm.ClimateData = new ClimateData
        {
            PrecipitationData = new PrecipitationData { January = 100, May = 200 },
            EvapotranspirationData = new EvapotranspirationData { January = 300, May = 600 },
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: nextYearViewItem,
            farm: farm,
            animalResults: new List<AnimalComponentEmissionsResults>());

        // This year's C_p = next-year C_p * EGFFP = 435.6 * 0.5 = 217.8
        // C_ag = 217.8 * (10/100) = 21.78
        Assert.AreEqual(21.78, currentYearViewItem.AboveGroundCarbonInput);
    }

    [TestMethod]
    public void CalculateAboveGroundCarbonInputForPerennialsWhenPlantCarbonForAgriculturalProductIsUnknownInCurrentYearAndIsUnknownInNextYear()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.TameMixed,
            Yield = 0,
            PercentageOfProductYieldReturnedToSoil = 10,
            PercentageOfStrawReturnedToSoil = 0,
            CarbonConcentration = 0.45,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.CashCrop,
            YearInPerennialStand = 1,
        };

        var nextYearViewItem = new CropViewItem
        {
            CropType = CropType.TameMixed,
            Yield = 0,
            PercentageOfProductYieldReturnedToSoil = 10,
            PercentageOfStrawReturnedToSoil = 0,
            CarbonConcentration = 0.45,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.12,
            MoistureContentOfCropPercentage = 12,
            HarvestMethod = HarvestMethods.CashCrop,
            YearInPerennialStand = 2,
        };

        var farm = MakeFarm();
        farm.Defaults = new Defaults { EstablishmentGrowthFactorPercentageForPerennials = 50 };
        farm.ClimateData = new ClimateData
        {
            PrecipitationData = new PrecipitationData { January = 100, May = 200 },
            EvapotranspirationData = new EvapotranspirationData { January = 300, May = 600 },
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: nextYearViewItem,
            farm: farm,
            animalResults: new List<AnimalComponentEmissionsResults>());

        // See https://github.com/holos-aafc/Holos/issues/405
        var moistureCorrection = (1.0 - 0.13) / (1.0 - 0.8);
        var expected = 10.293 * moistureCorrection;

        Assert.AreEqual(expected, currentYearViewItem.AboveGroundCarbonInput, 3);
    }

    [TestMethod]
    public void CalculateBelowGroundCarbonInputForPerennialsInFirstYear()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.TameMixed,
            Yield = 1000,
            PercentageOfRootsReturnedToSoil = 100,
            CarbonConcentration = 0.45,
            PercentageOfProductYieldReturnedToSoil = 0,
            PercentageOfExtraRootsReturnedToSoil = 100,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.CashCrop,
            BiomassCoefficientProduct = 0.5,
            BiomassCoefficientRoots = 0.4,
            BiomassCoefficientExtraroot = 0.3,
            PlantCarbonInAgriculturalProduct = 396,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: null!,
            farm: MakeFarm(),
            animalResults: new List<AnimalComponentEmissionsResults>());

        // Root C input < 450 floor → 450, plus extraroot 237.6 → 687.6
        Assert.AreEqual(687.6, currentYearViewItem.BelowGroundCarbonInput, 1);
    }

    [TestMethod]
    public void CalculateAboveGroundInputsForGreenManureHarvestMethod()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.Lentils,
            Yield = 1000,
            PercentageOfProductYieldReturnedToSoil = 2,
            PercentageOfStrawReturnedToSoil = 0,
            CarbonConcentration = 0.45,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.GreenManure,
            BiomassCoefficientProduct = 0.5,
            BiomassCoefficientStraw = 0.2,
            PlantCarbonInAgriculturalProduct = 403.92,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: new CropViewItem(),
            farm: MakeFarm(),
            animalResults: new List<AnimalComponentEmissionsResults>());

        Assert.AreEqual(7.92, currentYearViewItem.AboveGroundCarbonInput);
    }

    [TestMethod]
    public void CalculateBelowGroundInputsForGreenManureHarvestMethod()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.Lentils,
            Yield = 1000,
            PercentageOfRootsReturnedToSoil = 100,
            CarbonConcentration = 0.45,
            PercentageOfProductYieldReturnedToSoil = 2,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            PerennialStandLength = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.GreenManure,
            BiomassCoefficientProduct = 0.5,
            BiomassCoefficientStraw = 0.2,
            BiomassCoefficientRoots = 0.4,
            BiomassCoefficientExtraroot = 0.3,
            PlantCarbonInAgriculturalProduct = 403.92,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: null!,
            farm: MakeFarm(Province.Alberta),
            animalResults: new List<AnimalComponentEmissionsResults>());

        Assert.AreEqual(396, currentYearViewItem.BelowGroundCarbonInput, 3);
    }

    [TestMethod]
    public void CalculateInputsFromSupplementalHayFedToGrazingAnimals()
    {
        var farm = new Farm
        {
            Defaults = new Defaults { DefaultSupplementalFeedingLossPercentage = 20 },
        };

        var currentYearViewItem = new CropViewItem
        {
            CarbonConcentration = 0.45,

            // Supplemental hay fed to grazing animals: leftover C is added to above-ground inputs.
            HayImportViewItems = new ObservableCollection<HayImportViewItem>
            {
                new HayImportViewItem
                {
                    NumberOfBales = 10,
                    BaleWeight = 500,
                    MoistureContentAsPercentage = 12,
                },
            },
        };

        var result = _sut.CalculateInputsFromSupplementalHayFedToGrazingAnimals(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItems: null!,
            farm: farm);

        // = [(10 * 500) * (1 - 12/100)] * (20/100) * 0.45 = 396
        Assert.AreEqual(396, result);
    }

    /// <summary>Equation 2.2.5-3</summary>
    [TestMethod]
    public void CalculatePlantCarbonInAgriculturalProduct()
    {
        var currentYearViewItem = new CropViewItem
        {
            Yield = 1000,
            CarbonConcentration = 0.45,
            PercentageOfProductYieldReturnedToSoil = 22,
            CropType = CropType.Barley,
            MoistureContentOfCrop = 0.12,
        };

        var result = _sut.CalculatePlantCarbonInAgriculturalProduct(null!, currentYearViewItem, new Farm());

        // = [(1000 / (1 - 0.22)) * (1 - 0.12)] * 0.45
        Assert.AreEqual(507.69, result, delta: 2);
    }

    [TestMethod]
    public void CalculatePlantCarbonInAgriculturalProductGreenManure()
    {
        var currentYearViewItem = new CropViewItem
        {
            Yield = 1000,
            CarbonConcentration = 0.45,
            PercentageOfProductYieldReturnedToSoil = 100, // Indicates green manure
            CropType = CropType.Barley,
            MoistureContentOfCrop = 0.12,
        };

        var result = _sut.CalculatePlantCarbonInAgriculturalProduct(null!, currentYearViewItem, new Farm());

        // = 1000 * 0.88 * 0.45 = 396
        Assert.AreEqual(396, result, delta: 2);
    }

    [TestMethod]
    public void CalculatePlantCarbonInAgriculturalProductGrazedFieldAndCustomYieldAssignmentMethod()
    {
        var currentYearViewItem = new CropViewItem
        {
            Yield = 1000,
            CarbonConcentration = 0.45,
            PercentageOfProductYieldReturnedToSoil = 10,
            CropType = CropType.Barley,
            MoistureContentOfCrop = 0.12,
        };
        currentYearViewItem.GrazingViewItems.Add(new GrazingViewItem());

        var farm = new Farm { YieldAssignmentMethod = YieldAssignmentMethod.Custom };

        var result = _sut.CalculatePlantCarbonInAgriculturalProduct(null!, currentYearViewItem, farm);

        // Grazing default moisture (60%) drives this — yield * (1 - 0.6) * carbonConcentration = 450? See v4 expected.
        Assert.AreEqual(450, result, delta: 2);
    }

    [TestMethod]
    public void CalculateAboveGroundCarbonInputFromPerennialsGrazedFieldAndCustomYieldAssignmentMethod()
    {
        var currentYearViewItem = new CropViewItem
        {
            Yield = 1000,
            CarbonConcentration = 0.45,
            PercentageOfProductYieldReturnedToSoil = 10,
            CropType = CropType.Barley,
            MoistureContentOfCrop = 0.12,
            YearInPerennialStand = 1,
            PlantCarbonInAgriculturalProduct = 20,
        };
        currentYearViewItem.GrazingViewItems.Add(new GrazingViewItem { Utilization = 55 });

        var farm = new Farm { YieldAssignmentMethod = YieldAssignmentMethod.Custom };

        var result = _sut.CalculateAboveGroundCarbonInputFromPerennials(null!, currentYearViewItem, null!, farm);

        Assert.AreEqual(9, result);
    }

    [TestMethod]
    public void CalculateProductivity()
    {
        const double annualPrecipitation = 806;
        const double annualPotentialEvapotranspiration = 618;
        const double proportionOfPrecipitationMayToSeptember = 0.04;
        const double carbonConcentration = 0.45;

        var result = _sut.CalculateProductivity(
            annualPrecipitation: annualPrecipitation,
            annualPotentialEvapotranspiration: annualPotentialEvapotranspiration,
            proportionOfPrecipitationMayThroughSeptember: proportionOfPrecipitationMayToSeptember,
            carbonConcentration: carbonConcentration);

        // See https://github.com/holos-aafc/Holos/issues/405
        var moistureCorrection = (1.0 - 0.13) / (1.0 - 0.8);
        var expected = 87.578217100325958 * moistureCorrection;

        Assert.AreEqual(expected, result, 2);
    }

    [TestMethod]
    public void CalculateCarbonInputFromProduct()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.Barley,
            PlantCarbonInAgriculturalProduct = 200,
            PercentageOfProductYieldReturnedToSoil = 10,
            Yield = 1000,
            MoistureContentOfCrop = 0.12,
            CarbonConcentration = 0.45,
        };

        var result = _sut.CalculateCarbonInputFromProduct(null!, currentYearViewItem, null!, new Farm());

        // C_ptoSoil = C_p * (S_p / 100) = 200 * 0.1 = 20
        Assert.AreEqual(20, result);
    }

    [TestMethod]
    public void CalculateCarbonInputFromRootsForPerennialsWhenCurrentYearInput()
    {
        var previousYearViewItem = new CropViewItem { CarbonInputFromRoots = 5000 };
        var currentYearViewItem = new CropViewItem
        {
            PlantCarbonInAgriculturalProduct = 100,
            BiomassCoefficientProduct = 0.15,
            BiomassCoefficientRoots = 0.04,
            PercentageOfRootsReturnedToSoil = 100,
        };

        var result = _sut.CalculateCarbonInputFromRootsForPerennials(previousYearViewItem, currentYearViewItem, new Farm());

        Assert.AreEqual(5967.5, result, 1);
    }

    [TestMethod]
    public void CalculateCarbonInputFromRootsForPerennialsWhenCurrentYearInputsIsGreaterThanPreviousYearInputs()
    {
        var previousYearViewItem = new CropViewItem { CarbonInputFromRoots = 5000 };
        var currentYearViewItem = new CropViewItem
        {
            PlantCarbonInAgriculturalProduct = 100,
            BiomassCoefficientProduct = 0.15,
            BiomassCoefficientRoots = 0.04,
            PercentageOfRootsReturnedToSoil = 100,
        };

        var result = _sut.CalculateCarbonInputFromRootsForPerennials(previousYearViewItem, currentYearViewItem, new Farm());

        Assert.AreEqual(5967.5, result, 1);
    }

    [TestMethod]
    public void CalculateCarbonInputFromRootsForPerennialsWhenCurrentYearInputsIsLessThanPreviousYearInputs()
    {
        var previousYearViewItem = new CropViewItem { CarbonInputFromRoots = 50 };
        var currentYearViewItem = new CropViewItem
        {
            PlantCarbonInAgriculturalProduct = 100,
            BiomassCoefficientProduct = 0.15,
            BiomassCoefficientRoots = 0.04,
            PercentageOfRootsReturnedToSoil = 100,
        };

        var result = _sut.CalculateCarbonInputFromRootsForPerennials(previousYearViewItem, currentYearViewItem, new Farm());

        // Year-over-year: prev * (1 + 19.35/100) = 50 * 1.1935 = 59.675
        Assert.AreEqual(59.675, result);
    }

    [TestMethod]
    public void CalculateCarbonInputFromExtrarootsForPerennialsWhenCurrentYearInputsIsGreaterThanPreviousYearInputs()
    {
        var previousYearViewItem = new CropViewItem { CarbonInputFromExtraroots = 10 };
        var currentYearViewItem = new CropViewItem
        {
            PlantCarbonInAgriculturalProduct = 100,
            PercentageOfExtraRootsReturnedToSoil = 100,
            BiomassCoefficientProduct = 0.15,
            BiomassCoefficientExtraroot = 0.04,
        };

        var result = _sut.CalculateCarbonInputFromExtrarootsForPerennials(previousYearViewItem, currentYearViewItem, new Farm());

        Assert.AreEqual(26.7, result, 1);
    }

    [TestMethod]
    public void CalculateCarbonInputFromExtrarootsForPerennialsWhenCurrentYearInputsIsLessThanPreviousYearInputs()
    {
        var previousYearViewItem = new CropViewItem { CarbonInputFromExtraroots = 50 };
        var currentYearViewItem = new CropViewItem
        {
            PlantCarbonInAgriculturalProduct = 100,
            PercentageOfExtraRootsReturnedToSoil = 100,
            BiomassCoefficientProduct = 0.15,
            BiomassCoefficientExtraroot = 0.04,
        };

        var result = _sut.CalculateCarbonInputFromExtrarootsForPerennials(previousYearViewItem, currentYearViewItem, new Farm());

        // Extraroots do not carry over previous year's value — depends only on C_p.
        Assert.AreEqual(26.6, result, 0.1);
    }

    #endregion

    #region Silage

    [TestMethod]
    public void CalculateAboveGroundCarbonInputForSilage()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.BarleySilage,
            Yield = 1000,
            PercentageOfProductYieldReturnedToSoil = 35,
            PercentageOfStrawReturnedToSoil = 0,
            CarbonConcentration = 0.45,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.CashCrop,
            BiomassCoefficientProduct = 0,
            BiomassCoefficientStraw = 0,
            PlantCarbonInAgriculturalProduct = 534.6,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: new CropViewItem(),
            farm: MakeFarm(),
            animalResults: new List<AnimalComponentEmissionsResults>());

        Assert.AreEqual(213.23, currentYearViewItem.AboveGroundCarbonInput, 2);
    }

    [TestMethod]
    public void CalculateBelowGroundCarbonInputForSilage()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.BarleySilage,
            Yield = 1000,
            PercentageOfRootsReturnedToSoil = 100,
            CarbonConcentration = 0.45,
            PercentageOfProductYieldReturnedToSoil = 35,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            PerennialStandLength = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.CashCrop,
            BiomassCoefficientProduct = 0.2,
            BiomassCoefficientStraw = 0.5,
            BiomassCoefficientRoots = 0.4,
            BiomassCoefficientExtraroot = 0.3,
            PlantCarbonInAgriculturalProduct = 534.6,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: null!,
            farm: MakeFarm(Province.Alberta),
            animalResults: new List<AnimalComponentEmissionsResults>());

        // C_bg = C_r + C_e = [C_p * (R_r/R_p) * (S_r/100)] + [C_p * (R_e/R_p)]
        //      = [609.23 * 2 * 1] + [609.23 * 1.5] = 2132.305
        Assert.AreEqual(2132.305, currentYearViewItem.BelowGroundCarbonInput, 1);
    }

    #endregion

    #region Root Crops

    [TestMethod]
    public void CalculateAboveGroundCarbonInputForRootCrops()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.Potatoes,
            Yield = 1000,
            PercentageOfProductYieldReturnedToSoil = 0,
            PercentageOfStrawReturnedToSoil = 100,
            CarbonConcentration = 0.45,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.CashCrop,
            BiomassCoefficientProduct = 0.736,
            BiomassCoefficientStraw = 0.239,
            PlantCarbonInAgriculturalProduct = 396,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: new CropViewItem(),
            farm: MakeFarm(),
            animalResults: new List<AnimalComponentEmissionsResults>());

        // C_ag = C_s = 396 * (0.239/0.736) * 1 = 128.59
        Assert.AreEqual(128.59, currentYearViewItem.AboveGroundCarbonInput, 2);
    }

    [TestMethod]
    public void CalculateBelowGroundCarbonInputForRootCrops()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.Potatoes,
            Yield = 1000,
            PercentageOfRootsReturnedToSoil = 100,
            CarbonConcentration = 0.45,
            PercentageOfProductYieldReturnedToSoil = 0,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            PerennialStandLength = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.CashCrop,
            BiomassCoefficientProduct = 0.736,
            BiomassCoefficientStraw = 0.239,
            BiomassCoefficientRoots = 0.015,
            BiomassCoefficientExtraroot = 0.01,
            PlantCarbonInAgriculturalProduct = 396,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: null!,
            farm: MakeFarm(),
            animalResults: new List<AnimalComponentEmissionsResults>());

        // C_bg = C_ptoSoil + C_e = 0 + [396 * (0.01/0.736)] = 5.38
        Assert.AreEqual(5.38, currentYearViewItem.BelowGroundCarbonInput, 2);
    }

    #endregion

    #region Cover Crops

    [TestMethod]
    public void CalculateAboveGroundCarbonInputFromCoverCropsWhenCoverCropIsUsedAsMainCrop()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.WinterWheat,
            Yield = 1000,
            PercentageOfProductYieldReturnedToSoil = 50,
            PercentageOfStrawReturnedToSoil = 10,
            CarbonConcentration = 0.45,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.CashCrop,
            BiomassCoefficientProduct = 0.451,
            BiomassCoefficientStraw = 0.340,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: new CropViewItem(),
            farm: MakeFarm(Province.Alberta),
            animalResults: new List<AnimalComponentEmissionsResults>());

        // C_p ~ 792; C_ag = 792*0.5 + 792*(0.340/0.451)*0.1 = 455.6376
        Assert.AreEqual(455.6376, currentYearViewItem.AboveGroundCarbonInput, 4);
    }

    [TestMethod]
    public void CalculateBelowGroundCarbonInputFromCoverCropsWhenCoverCropIsUsedAsMainCrop()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.WinterWheat,
            Yield = 1000,
            PercentageOfRootsReturnedToSoil = 100,
            CarbonConcentration = 0.45,
            PercentageOfProductYieldReturnedToSoil = 100,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            PerennialStandLength = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.CashCrop,
            BiomassCoefficientProduct = 0.451,
            BiomassCoefficientStraw = 0.340,
            BiomassCoefficientRoots = 0.126,
            BiomassCoefficientExtraroot = 0.082,
            PlantCarbonInAgriculturalProduct = 396,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: null!,
            farm: MakeFarm(Province.Alberta),
            animalResults: new List<AnimalComponentEmissionsResults>());

        Assert.AreEqual(182.28, currentYearViewItem.BelowGroundCarbonInput, 1);
    }

    [TestMethod]
    public void CalculateAboveGroundCarbonInputFromCoverCropsWhenCoverCropIsUsedAsGreenManure()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.WinterWheat,
            Yield = 1000,
            PercentageOfProductYieldReturnedToSoil = 50,
            PercentageOfStrawReturnedToSoil = 10,
            CarbonConcentration = 0.45,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.GreenManure,
            BiomassCoefficientProduct = 0.451,
            BiomassCoefficientStraw = 0.340,
            PlantCarbonInAgriculturalProduct = 594,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: new CropViewItem(),
            farm: MakeFarm(Province.Alberta),
            animalResults: new List<AnimalComponentEmissionsResults>());

        Assert.AreEqual(198, currentYearViewItem.AboveGroundCarbonInput, 1);
    }

    [TestMethod]
    public void CalculateBelowGroundCarbonInputFromCoverCropsWhenCoverCropIsUsedAsGreenManure()
    {
        var currentYearViewItem = new CropViewItem
        {
            CropType = CropType.WinterWheat,
            Yield = 1000,
            PercentageOfRootsReturnedToSoil = 100,
            CarbonConcentration = 0.45,
            PercentageOfProductYieldReturnedToSoil = 50,
            IrrigationType = IrrigationType.RainFed,
            AmountOfIrrigation = 0,
            PerennialStandLength = 0,
            MoistureContentOfCrop = 0.12,
            HarvestMethod = HarvestMethods.GreenManure,
            BiomassCoefficientProduct = 0.451,
            BiomassCoefficientStraw = 0.340,
            BiomassCoefficientRoots = 0.126,
            BiomassCoefficientExtraroot = 0.082,
            PlantCarbonInAgriculturalProduct = 594,
        };

        _sut.AssignInputs(
            previousYearViewItem: null!,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: null!,
            farm: MakeFarm(Province.BritishColumbia),
            animalResults: new List<AnimalComponentEmissionsResults>());

        Assert.AreEqual(104.131479140329, currentYearViewItem.BelowGroundCarbonInput, 3);
    }

    #endregion
}
