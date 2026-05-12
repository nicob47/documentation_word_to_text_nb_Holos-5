using H.Core.Calculators.Carbon;
using H.Core.Calculators.Nitrogen;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Providers.Climate;
using H.Core.Services.LandManagement;

#nullable disable

namespace H.Core.Test.Services;

/// <summary>
/// Phase 4.5 smoke tests for <see cref="FieldResultsService.CalculateFinalResults(Farm)"/>.
///
/// Verifies that the orchestrator entry point can be constructed via the v5 carbon-calculator
/// stack (ICBMSoilCarbonCalculator + IPCCTier2SoilCarbonCalculator + N2OEmissionFactorCalculator)
/// and that the bare paths return without exception. Deterministic and offline — no NASA / SLC
/// climate calls, in contrast to the existing CarbonModellingIntegrationTest. Regressions in
/// the constructor chain or the empty-farm entry handling will surface here.
/// </summary>
[TestClass]
public class FieldResultsServiceCalculateFinalResultsSmokeTest : UnitTestBase
{
    private FieldResultsService _sut = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var climateProvider = new ClimateProvider(_slcClimateProvider.Object);
        var n2OEmissionFactorCalculator = new N2OEmissionFactorCalculator(climateProvider);
        var icbm = new ICBMSoilCarbonCalculator(climateProvider, n2OEmissionFactorCalculator);
        var ipcc = new IPCCTier2SoilCarbonCalculator(climateProvider, n2OEmissionFactorCalculator);

        _sut = new FieldResultsService(icbm, ipcc, n2OEmissionFactorCalculator);
    }

    [TestMethod]
    public void CalculateFinalResultsForFarmWithNoFieldsReturnsEmptyAndDoesNotThrow()
    {
        var farm = new Farm();

        var result = _sut.CalculateFinalResults(farm);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void CalculateFinalResultsForCollectionOfFarmsWithNoFieldsReturnsEmptyAndDoesNotThrow()
    {
        var farms = new[] { new Farm(), new Farm() };

        var result = _sut.CalculateFinalResults(farms);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void CalculateFinalResultsRespectsEmptyStageStateWithoutThrowing()
    {
        // A farm with an empty FieldSystemDetailsStageState exercises the GetStageState branch
        // inside CalculateFinalResults; the groupBy/loop should be a no-op rather than NRE.
        var farm = new Farm();
        farm.StageStates.Add(new FieldSystemDetailsStageState());

        var result = _sut.CalculateFinalResults(farm);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
}
