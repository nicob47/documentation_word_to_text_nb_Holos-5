using H.Core.Calculators.Shelterbelt;
using H.Core.Emissions.Results;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Models.LandManagement.Shelterbelt;
using H.Core.Services.Analysis;
using H.Core.Services.Animals;
using H.Core.Services.LandManagement;
using Moq;

namespace H.Core.Test.Services.Analysis;

/// <summary>
/// Phase 6.2 — multi-field coverage for <see cref="FarmAnalysisService"/>.
///
/// Exercises the DTO mapping with a mocked <see cref="IFieldResultsService"/> + <see
/// cref="IAnimalService"/> so the tests stay deterministic and don't depend on the full carbon
/// pipeline (which the existing CarbonModellingIntegrationTest already covers).
/// </summary>
[TestClass]
public class FarmAnalysisServiceTests
{
    private Mock<IFieldResultsService> _fieldResultsService = null!;
    private Mock<IAnimalService> _animalService = null!;
    private ShelterbeltCalculator _shelterbeltCalculator = null!;
    private FarmAnalysisService _sut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _fieldResultsService = new Mock<IFieldResultsService>();
        _fieldResultsService.SetupProperty(s => s.AnimalResults, new List<AnimalComponentEmissionsResults>());

        _animalService = new Mock<IAnimalService>();
        _animalService.Setup(s => s.GetAnimalResults(It.IsAny<Farm>()))
            .Returns(new List<AnimalComponentEmissionsResults>());

        _shelterbeltCalculator = new ShelterbeltCalculator();

        _sut = new FarmAnalysisService(_fieldResultsService.Object, _animalService.Object, _shelterbeltCalculator);
    }

    [TestMethod]
    public void Constructor_NullFieldResultsService_Throws()
    {
        var ex = Assert.ThrowsExactly<ArgumentNullException>(
            () => new FarmAnalysisService(null!, _animalService.Object, _shelterbeltCalculator));
        Assert.AreEqual("fieldResultsService", ex.ParamName);
    }

    [TestMethod]
    public void Constructor_NullAnimalService_Throws()
    {
        var ex = Assert.ThrowsExactly<ArgumentNullException>(
            () => new FarmAnalysisService(_fieldResultsService.Object, null!, _shelterbeltCalculator));
        Assert.AreEqual("animalService", ex.ParamName);
    }

    [TestMethod]
    public void Constructor_NullShelterbeltCalculator_Throws()
    {
        var ex = Assert.ThrowsExactly<ArgumentNullException>(
            () => new FarmAnalysisService(_fieldResultsService.Object, _animalService.Object, null!));
        Assert.AreEqual("shelterbeltCalculator", ex.ParamName);
    }

    /// <summary>
    /// Build a synthetic farm with a Canadian province so the analysis-entry guard
    /// (rejects non-Canadian provinces) accepts it. Default <c>new Farm()</c> leaves
    /// <see cref="Farm.Province"/> at the enum default <c>SelectProvince</c>, which the
    /// guard correctly treats as "user hasn't picked a Canadian province yet" and
    /// refuses to run — so every test that hits RunAnalysis goes through this helper.
    /// </summary>
    private static Farm MakeFarm(string? name = null)
    {
        var farm = new Farm { Province = Province.Alberta };
        if (!string.IsNullOrEmpty(name))
        {
            farm.Name = name;
        }
        return farm;
    }

    [TestMethod]
    public void RunAnalysis_NullFarm_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => _sut.RunAnalysis(null!));
    }

    [TestMethod]
    public void RunAnalysis_NonCanadianProvince_ThrowsInvalidOperationExceptionWithClearMessage()
    {
        // Guard A: a farm imported from a v4 .json with Province=Laois (or any other
        // Ireland-mode value) used to silently produce a NaN-filled chart because every
        // Canada-keyed provider missed and the ICBM steady-state denominator collapsed
        // to zero. Now we refuse to run at all and surface a clear message that the
        // existing LastErrorMessage banner in GHGResultsView can show.
        var farm = new Farm { Province = Province.Laois, Name = "Imported v4 farm" };

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => _sut.RunAnalysis(farm));

        StringAssert.Contains(ex.Message, "non-Canadian");
        StringAssert.Contains(ex.Message, "Laois");
    }

    [TestMethod]
    public void RunAnalysis_DefaultSelectProvince_AlsoBlockedByGuard()
    {
        // A brand-new Farm() defaults to Province.SelectProvince (enum value 0). The guard
        // treats this the same way as any other non-Canadian value: refuse to run so the
        // user is forced into the soil settings to pick a province first. This is the
        // behaviour every other test in this class works around via MakeFarm().
        var farm = new Farm { Name = "Fresh farm with no province picked" };

        Assert.ThrowsExactly<InvalidOperationException>(() => _sut.RunAnalysis(farm));
    }

    [TestMethod]
    public void RunAnalysis_EmptyFarm_ReturnsEmptyResultsWithIsEmptyTrue()
    {
        _fieldResultsService.Setup(s => s.CalculateFinalResults(It.IsAny<Farm>()))
            .Returns(new List<CropViewItem>());

        var farm = MakeFarm("Bare farm");

        var result = _sut.RunAnalysis(farm);

        Assert.IsTrue(result.IsEmpty);
        Assert.AreEqual(0, result.YearResults.Count);
        Assert.AreEqual("Bare farm", result.FarmName);
    }

    [TestMethod]
    public void RunAnalysis_PrimesAnimalResultsBeforeCalculatingFinalResults()
    {
        // Track call order: AnimalResults must be set before CalculateFinalResults — the field
        // pipeline pulls grazing/manure inputs from AnimalResults during calculation.
        var animalResults = new List<AnimalComponentEmissionsResults>
        {
            new AnimalComponentEmissionsResults(),
        };

        var farm = MakeFarm();
        _animalService.Setup(s => s.GetAnimalResults(farm)).Returns(animalResults);

        List<AnimalComponentEmissionsResults>? observedAtCalculateTime = null;
        _fieldResultsService
            .Setup(s => s.CalculateFinalResults(farm))
            .Callback<Farm>(_ => observedAtCalculateTime = _fieldResultsService.Object.AnimalResults)
            .Returns(new List<CropViewItem>());

        _sut.RunAnalysis(farm);

        Assert.IsNotNull(observedAtCalculateTime);
        Assert.AreSame(animalResults, observedAtCalculateTime);
    }

    [TestMethod]
    public void RunAnalysis_MultipleFields_OrdersByFieldNameThenYear()
    {
        // Two fields, three years each, deliberately added in scrambled order.
        var crops = new List<CropViewItem>
        {
            new() { FieldName = "South Pasture", Year = 2022, SoilCarbon = 22 },
            new() { FieldName = "North Field",   Year = 2021, SoilCarbon = 11 },
            new() { FieldName = "South Pasture", Year = 2020, SoilCarbon = 20 },
            new() { FieldName = "North Field",   Year = 2022, SoilCarbon = 12 },
            new() { FieldName = "South Pasture", Year = 2021, SoilCarbon = 21 },
            new() { FieldName = "North Field",   Year = 2020, SoilCarbon = 10 },
        };
        _fieldResultsService.Setup(s => s.CalculateFinalResults(It.IsAny<Farm>())).Returns(crops);

        var result = _sut.RunAnalysis(MakeFarm());

        Assert.AreEqual(6, result.YearResults.Count);

        // Expect: North Field 2020/2021/2022 then South Pasture 2020/2021/2022.
        var expectedOrder = new[]
        {
            ("North Field", 2020), ("North Field", 2021), ("North Field", 2022),
            ("South Pasture", 2020), ("South Pasture", 2021), ("South Pasture", 2022),
        };
        for (int i = 0; i < expectedOrder.Length; i++)
        {
            Assert.AreEqual(expectedOrder[i].Item1, result.YearResults[i].FieldName, $"row {i}: field");
            Assert.AreEqual(expectedOrder[i].Item2, result.YearResults[i].Year, $"row {i}: year");
        }

        Assert.IsFalse(result.IsEmpty);
    }

    [TestMethod]
    public void RunAnalysis_PrefersCombinedInputsWhenNonZero()
    {
        // When CarbonService.CombineCarbonInputs has run, the Combined* fields hold the main+cover
        // crop totals and should win over the per-main-crop values.
        var crop = new CropViewItem
        {
            FieldName = "F",
            Year = 2024,
            AboveGroundCarbonInput = 100,
            CombinedAboveGroundInput = 150,           // main + cover
            BelowGroundCarbonInput = 200,
            CombinedBelowGroundInput = 240,
            ManureCarbonInputsPerHectare = 50,
            CombinedManureInput = 60,
            DigestateCarbonInputsPerHectare = 10,
            CombinedDigestateInput = 12,
        };
        _fieldResultsService.Setup(s => s.CalculateFinalResults(It.IsAny<Farm>()))
            .Returns(new List<CropViewItem> { crop });

        var result = _sut.RunAnalysis(MakeFarm());

        var row = result.YearResults.Single();
        Assert.AreEqual(150, row.AboveGroundCarbonInput);
        Assert.AreEqual(240, row.BelowGroundCarbonInput);
        Assert.AreEqual(60, row.ManureCarbonInput);
        Assert.AreEqual(12, row.DigestateCarbonInput);
    }

    [TestMethod]
    public void RunAnalysis_FallsBackToPerCropInputsWhenCombinedAreZero()
    {
        // No cover crop → CarbonService.CombineCarbonInputs leaves Combined* at zero. We should
        // still surface the main-crop inputs rather than blank rows.
        var crop = new CropViewItem
        {
            FieldName = "F",
            Year = 2024,
            AboveGroundCarbonInput = 100,
            CombinedAboveGroundInput = 0,
            BelowGroundCarbonInput = 200,
            CombinedBelowGroundInput = 0,
            ManureCarbonInputsPerHectare = 50,
            CombinedManureInput = 0,
            DigestateCarbonInputsPerHectare = 10,
            CombinedDigestateInput = 0,
        };
        _fieldResultsService.Setup(s => s.CalculateFinalResults(It.IsAny<Farm>()))
            .Returns(new List<CropViewItem> { crop });

        var result = _sut.RunAnalysis(MakeFarm());

        var row = result.YearResults.Single();
        Assert.AreEqual(100, row.AboveGroundCarbonInput);
        Assert.AreEqual(200, row.BelowGroundCarbonInput);
        Assert.AreEqual(50, row.ManureCarbonInput);
        Assert.AreEqual(10, row.DigestateCarbonInput);
    }

    [TestMethod]
    public void RunAnalysis_PopulatesCarbonModellingStrategyFromFarmDefaults()
    {
        var farm = MakeFarm();
        farm.Defaults.CarbonModellingStrategy = CarbonModellingStrategies.IPCCTier2;

        _fieldResultsService.Setup(s => s.CalculateFinalResults(farm)).Returns(new List<CropViewItem>());

        var result = _sut.RunAnalysis(farm);

        StringAssert.Contains(result.CarbonModellingStrategy, "IPCC");
    }

    [TestMethod]
    public void RunAnalysis_FarmWithoutShelterbelts_ReturnsEmptyShelterbeltYearResults()
    {
        _fieldResultsService.Setup(s => s.CalculateFinalResults(It.IsAny<Farm>()))
            .Returns(new List<CropViewItem>());

        var farm = MakeFarm();

        var result = _sut.RunAnalysis(farm);

        Assert.AreEqual(0, result.ShelterbeltYearResults.Count);
    }

    [TestMethod]
    public void RunAnalysis_MapsAnimalDrivenNitrogenAndN2OToDto()
    {
        // Phase 6.4: animal-driven N (manure applications + grazing deposits) and the
        // resulting N₂O flow through CropViewItem after FieldResultsService runs. Confirm
        // FarmAnalysisService surfaces those values in the DTO so the GUI can show them.
        var crop = new CropViewItem
        {
            FieldName = "Pasture",
            Year = 2024,
            AmountOfNitrogenAppliedFromManure = 142.5,    // kg N — applications + grazing
            TotalDirectNitrousOxidePerHectare = 1.83,     // kg N₂O ha⁻¹
            TotalIndirectNitrousOxidePerHectare = 0.47,   // kg N₂O ha⁻¹
        };
        _fieldResultsService.Setup(s => s.CalculateFinalResults(It.IsAny<Farm>()))
            .Returns(new List<CropViewItem> { crop });

        var result = _sut.RunAnalysis(MakeFarm());

        var row = result.YearResults.Single();
        Assert.AreEqual(142.5, row.NitrogenAppliedFromManure);
        Assert.AreEqual(1.83, row.DirectN2OPerHectare);
        Assert.AreEqual(0.47, row.IndirectN2OPerHectare);
        Assert.AreEqual(2.30, row.TotalN2OPerHectare, 1e-9);
    }

    [TestMethod]
    public void RunAnalysis_FarmWithEmptyShelterbeltComponent_StillProducesEmptyShelterbeltResults()
    {
        // A shelterbelt component with no rows / no trees → BuildTrannums populates nothing →
        // TotalResultsForEachYear returns an empty list. Confirms the FarmAnalysisService
        // shelterbelt branch handles a "shape's there, data isn't" component without throwing
        // (the v4 calculator does that gracefully and we preserve that contract).
        _fieldResultsService.Setup(s => s.CalculateFinalResults(It.IsAny<Farm>()))
            .Returns(new List<CropViewItem>());

        var farm = MakeFarm();
        farm.Components.Add(new ShelterbeltComponent { Name = "Empty belt" });

        var result = _sut.RunAnalysis(farm);

        Assert.AreEqual(0, result.ShelterbeltYearResults.Count);
        Assert.IsTrue(result.IsEmpty);
    }
}
