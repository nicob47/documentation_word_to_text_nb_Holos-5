using System.Diagnostics;
using H.Core.Calculators.Carbon;
using H.Core.Calculators.Nitrogen;
using H.Core.Enumerations;
using H.Core.Mappers;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Providers.Climate;
using H.Core.Services.LandManagement;

#nullable disable

namespace H.Core.Test.Services;

/// <summary>
/// Manual perf profile: exercises FieldResultsService.CalculateFinalResults end-to-end against a
/// representative multi-year / multi-field farm and dumps per-stage timings to the test output.
/// Designed to be run interactively (Test Explorer) when investigating analysis-pipeline cost.
/// The accompanying [Ignore] attribute keeps it out of normal CI runs.
/// </summary>
[TestClass]
public class FieldResultsServicePerfProfile : UnitTestBase
{
    private FieldResultsService _sut = null!;
    private const int FieldCount = 5;
    private const int YearSpan = 30;        // 1995..2024, mirrors a typical multi-decade rotation
    private const int RepetitionCount = 3;

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
    public void Profile_PropertyMapper_Map_HotPath()
    {
        // PropertyMapper.Map is invoked once per CropViewItem and once per child collection item
        // inside MapDetailsScreenViewItemFromComponentScreenViewItem. Reflection-heavy by design
        // (Type.GetProperties + per-property GetValue/SetValue), so we measure it in isolation.
        const int iterations = 50_000;
        var source = BuildRepresentativeCropViewItem();

        // Warmup so the JIT / reflection metadata is paid for outside the timed region.
        for (var i = 0; i < 1000; i++)
        {
            _ = PropertyMapper.Map<CropViewItem, CropViewItem>(source);
        }

        var sw = Stopwatch.StartNew();
        for (var i = 0; i < iterations; i++)
        {
            _ = PropertyMapper.Map<CropViewItem, CropViewItem>(source);
        }
        sw.Stop();

        var nsPerCall = (sw.Elapsed.TotalMilliseconds * 1_000_000d) / iterations;
        Console.WriteLine($"[Perf.PropertyMapper] iterations={iterations} total={sw.ElapsedMilliseconds}ms perCall={nsPerCall:F1}ns");
    }

    [TestMethod]
    public void Profile_CalculateFinalResults_EndToEnd_ICBM()
    {
        RunEndToEnd(CarbonModellingStrategies.ICBM);
    }

    [TestMethod]
    public void Profile_CalculateFinalResults_EndToEnd_Tier2()
    {
        RunEndToEnd(CarbonModellingStrategies.IPCCTier2);
    }

    private void RunEndToEnd(CarbonModellingStrategies strategy)
    {
        var farm = BuildRepresentativeFarm(strategy);

        // Wire Trace -> Console so the per-stage [GHGAnalysis.Field] / [GHGAnalysis.Init] traces
        // emitted by FieldResultsService land in the test output. Removed at end so we don't
        // leak listeners between tests.
        var listener = new TextWriterTraceListener(Console.Out);
        Trace.Listeners.Add(listener);
        try
        {
            // Warmup pass — first call pays for stage-state init + JIT.
            _sut.InitializeStageState(farm);
            _ = _sut.CalculateFinalResults(farm);

            for (var rep = 0; rep < RepetitionCount; rep++)
            {
                // Mark the state dirty so each rep re-initializes (worst-case scenario, mirrors
                // user clicking Recalculate or editing a field component).
                var stage = farm.StageStates.OfType<FieldSystemDetailsStageState>().Single();
                stage.IsInitialized = false;

                var sw = Stopwatch.StartNew();
                _sut.InitializeStageState(farm);
                var initMs = sw.ElapsedMilliseconds;

                sw.Restart();
                var results = _sut.CalculateFinalResults(farm);
                var calcMs = sw.ElapsedMilliseconds;

                Console.WriteLine(
                    $"[Perf.E2E] rep={rep} strategy={strategy} fields={FieldCount} years={YearSpan} " +
                    $"init={initMs}ms calc={calcMs}ms rows={results.Count}");
            }
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }
    }

    private static Farm BuildRepresentativeFarm(CarbonModellingStrategies strategy)
    {
        var farm = new Farm
        {
            Name = "PerfFarm",
            Defaults = { CarbonModellingStrategy = strategy },
        };

        for (var f = 0; f < FieldCount; f++)
        {
            var component = new FieldSystemComponent
            {
                Name = $"Field{f}",
                StartYear = 2024 - YearSpan + 1,
                EndYear = 2024,
            };

            // Alternate crops year over year so combineInputs / merge have non-trivial work.
            for (var y = 0; y < YearSpan; y++)
            {
                var item = BuildRepresentativeCropViewItem();
                item.Year = 2024 - YearSpan + 1 + y;
                item.FieldName = component.Name;
                item.CropType = (y % 2 == 0) ? CropType.Wheat : CropType.Canola;
                item.FieldSystemComponentGuid = component.Guid;
                component.CropViewItems.Add(item);
            }

            farm.Components.Add(component);
        }

        return farm;
    }

    private static CropViewItem BuildRepresentativeCropViewItem()
    {
        return new CropViewItem
        {
            CropType = CropType.Wheat,
            Area = 25.0,
            Yield = 3500.0,
            MoistureContentOfCropPercentage = 12,
            TillageType = TillageType.NoTill,
            HarvestMethod = HarvestMethods.CashCrop,
            NumberOfPesticidePasses = 2,
            NitrogenDepositionAmount = 5,
            NitrogenFixation = 0,
        };
    }
}
