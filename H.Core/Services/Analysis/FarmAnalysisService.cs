using System.Diagnostics;
using H.Core.Calculators.Shelterbelt;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Models.LandManagement.Shelterbelt;
using H.Core.Models.Results;
using H.Core.Services.Animals;
using H.Core.Services.LandManagement;
using H.Infrastructure;
using NLog;

namespace H.Core.Services.Analysis;

/// <inheritdoc />
public class FarmAnalysisService : IFarmAnalysisService
{
        // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
        // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

    private readonly IFieldResultsService _fieldResultsService;
    private readonly IAnimalService _animalService;
    private readonly ShelterbeltCalculator _shelterbeltCalculator;

    public FarmAnalysisService(
        IFieldResultsService fieldResultsService,
        IAnimalService animalService,
        ShelterbeltCalculator shelterbeltCalculator)
    {
        ArgumentNullException.ThrowIfNull(fieldResultsService);
        ArgumentNullException.ThrowIfNull(animalService);
        ArgumentNullException.ThrowIfNull(shelterbeltCalculator);

        _fieldResultsService = fieldResultsService;
        _animalService = animalService;
        _shelterbeltCalculator = shelterbeltCalculator;
    }

    public FarmAnalysisResults RunAnalysis(Farm farm)
    {
        ArgumentNullException.ThrowIfNull(farm);

        // Guard A: refuse to run the analysis on a farm with a non-Canadian province. The
        // Canadian-only data providers (Table_4 irrigation, Table_63 indoor temperature,
        // EcodistrictDefaults, SLC climate normals, ...) all return null/0 for keys outside
        // the Canadian set; ICBM's steady-state denominator divides by a zero climate
        // parameter and produces NaN/Infinity, which LiveCharts silently drops. The user
        // sees the analysis "succeed" but the chart is blank. Block the run up front so the
        // existing LastErrorMessage banner in GHGResultsView shows a clear message instead.
        //
        // Two province fields are checked. Farm.Province is the user's selected province;
        // GeographicData.DefaultSoilData.Province is what every downstream calculator
        // actually reads (per the Farm.Province XML doc), and they can disagree on
        // farms imported from v4 .json where the soil-polygon province wasn't updated.
        var selectedProvince = farm.Province;
        var soilProvince = farm.GeographicData?.DefaultSoilData?.Province;
        var nonCanadianProvinces = new List<string>();
        if (!CanadianProvinces.IsCanadian(selectedProvince))
        {
            nonCanadianProvinces.Add($"Farm.Province='{selectedProvince.GetDescription()}'");
        }
        if (soilProvince.HasValue && !CanadianProvinces.IsCanadian(soilProvince.Value))
        {
            nonCanadianProvinces.Add($"GeographicData.DefaultSoilData.Province='{soilProvince.Value.GetDescription()}'");
        }
        if (nonCanadianProvinces.Count > 0)
        {
            var detail = string.Join("; ", nonCanadianProvinces);
            var message =
                $"This farm has a non-Canadian province ({detail}). " +
                "Holos v5 supports the 13 Canadian provinces and territories only. " +
                "Open the farm's soil settings and pick a Canadian province before running the analysis. " +
                "(This often happens on farms imported from a v4 .json that was authored under v4's Ireland mode.)";

            _log.Error(
                $"{nameof(FarmAnalysisService)}.{nameof(RunAnalysis)} {message}");

            throw new InvalidOperationException(message);
        }

        var totalSw = Stopwatch.StartNew();
        long animalMs, fieldMs, shelterbeltMs, mapMs, initMs;

        // CalculateFinalResults reads the per-year detail view items from the farm's
        // FieldSystemDetailsStageState â€” which is populated lazily by InitializeStageState. In the
        // legacy WPF GUI that initialization happens when the user opens the details screen for a
        // field; the Avalonia GUI lets the user go straight from the field component editor to the
        // results page without ever visiting a details screen, so the stage state is empty and the
        // analysis produces zero rows.
        //
        // Only (re)initialize when the stage state isn't already populated. Rebuilding the whole
        // detail-item tree on every call (e.g. every time the user toggles the carbon modelling
        // strategy combo box) is wasted work â€” the strategy only affects the downstream math, not
        // the inputs â€” and on a multi-field / multi-decade farm it dominates the analysis time.
        // Callers that know inputs have changed (a field component was edited, the user hit
        // Recalculate, etc.) can flip `stageState.IsInitialized` back to false to force a rebuild.
        var sw = Stopwatch.StartNew();
        var fieldStageState = farm.StageStates
            .OfType<H.Core.Models.LandManagement.Fields.FieldSystemDetailsStageState>()
            .SingleOrDefault();
        var hasFields = farm.Components.OfType<FieldSystemComponent>().Any();
        if (hasFields && (fieldStageState is null || !fieldStageState.IsInitialized))
        {
            _fieldResultsService.InitializeStageState(farm);
        }
        initMs = sw.ElapsedMilliseconds;

        // The field results service expects animal emissions to be populated before calculating
        // soil carbon results, because grazing / manure inputs feed the residue C/N calculations.
        sw.Restart();
        _fieldResultsService.AnimalResults = _animalService.GetAnimalResults(farm);
        animalMs = sw.ElapsedMilliseconds;

        sw.Restart();
        var detailViewItems = _fieldResultsService.CalculateFinalResults(farm);
        fieldMs = sw.ElapsedMilliseconds;

        sw.Restart();
        var shelterbeltResults = CalculateShelterbeltResults(farm);
        shelterbeltMs = sw.ElapsedMilliseconds;

        sw.Restart();
        var result = new FarmAnalysisResults
        {
            FarmName = farm.Name ?? string.Empty,
            Province = farm.Province.GetDescription() ?? string.Empty,
            CarbonModellingStrategy = farm.Defaults.CarbonModellingStrategy.GetDescription() ?? string.Empty,
            YearResults = detailViewItems
                .OrderBy(v => v.FieldName)
                .ThenBy(v => v.Year)
                .Select(ToYearResult)
                .ToList(),
            ShelterbeltYearResults = shelterbeltResults,
        };
        mapMs = sw.ElapsedMilliseconds;
        totalSw.Stop();

        // Emit a structured trace so the user can see where the time goes without needing a full
        // profiler attached. The 'GHGAnalysis' tag makes it greppable in Visual Studio's
        _log.Info(
            $"[GHGAnalysis] Farm='{farm.Name}' total={totalSw.ElapsedMilliseconds}ms " +
            $"init={initMs}ms animal={animalMs}ms field={fieldMs}ms shelterbelt={shelterbeltMs}ms map={mapMs}ms " +
            $"rows={result.YearResults.Count} shelterbeltRows={result.ShelterbeltYearResults.Count} " +
            $"strategy={farm.Defaults.CarbonModellingStrategy}");

        return result;
    }

    private IReadOnlyList<ShelterbeltYearResult> CalculateShelterbeltResults(Farm farm)
    {
        var shelterbelts = farm.Components.OfType<ShelterbeltComponent>().ToList();
        if (shelterbelts.Count == 0)
        {
            return Array.Empty<ShelterbeltYearResult>();
        }

        // Each component needs its yearly Trannum data built up before the cross-component
        // aggregation. The calculator does both halves but exposes them as separate calls so the
        // GUI's "edit shelterbelt" flow can recompute one component without re-running the entire
        // farm â€” mirroring that split here keeps both code paths consistent.
        foreach (var component in shelterbelts)
        {
            _shelterbeltCalculator.CalculateInitialResults(component);
        }

        var trannumResults = _shelterbeltCalculator.TotalResultsForEachYear(shelterbelts);

        return trannumResults
            .OrderBy(r => r.ShelterbeltComponent.Name)
            .ThenBy(r => r.Year)
            .Select(ToShelterbeltYearResult)
            .ToList();
    }

    private static ShelterbeltYearResult ToShelterbeltYearResult(TrannumResultViewItem trannum) => new()
    {
        Year = trannum.Year,
        ShelterbeltName = trannum.ShelterbeltComponent.Name ?? string.Empty,
        TotalLivingBiomassCarbon = trannum.TotalLivingBiomassCarbon,
        TotalLivingBiomassCarbonChange = trannum.TotalLivingBiomassCarbonChange,
        TotalDeadOrganicMatterCarbon = trannum.TotalDeadOrganicMatterCarbon,
        TotalDeadOrganicMatterChange = trannum.TotalDeadOrganicMatterChange,
        TotalEcosystemCarbon = trannum.TotalEcosystemCarbon,
        TotalEcosystemCarbonChange = trannum.TotalEcosystemCarbonChange,
    };

    private static FieldAnalysisYearResult ToYearResult(CropViewItem viewItem) => new()
    {
        Year = viewItem.Year,
        FieldName = viewItem.FieldName ?? string.Empty,
        CropType = viewItem.CropType.GetDescription() ?? string.Empty,
        Area = viewItem.Area,

        // Combined* fields include cover-crop contributions when CarbonService.CombineCarbonInputs
        // has run; otherwise they fall back to the main-crop-only values.
        AboveGroundCarbonInput = viewItem.CombinedAboveGroundInput > 0
            ? viewItem.CombinedAboveGroundInput
            : viewItem.AboveGroundCarbonInput,
        BelowGroundCarbonInput = viewItem.CombinedBelowGroundInput > 0
            ? viewItem.CombinedBelowGroundInput
            : viewItem.BelowGroundCarbonInput,
        ManureCarbonInput = viewItem.CombinedManureInput > 0
            ? viewItem.CombinedManureInput
            : viewItem.ManureCarbonInputsPerHectare,
        DigestateCarbonInput = viewItem.CombinedDigestateInput > 0
            ? viewItem.CombinedDigestateInput
            : viewItem.DigestateCarbonInputsPerHectare,
        TotalCarbonInputs = viewItem.TotalCarbonInputs,
        SoilCarbon = viewItem.SoilCarbon,
        ChangeInSoilCarbon = viewItem.ChangeInCarbon,

        // Nâ‚‚O coverage (Phase 6.4): manure/digestate/grazing N flows through
        // FieldResultsService.CalculateNitrogenAtInterval into TotalDirect/IndirectNitrousOxide
        // per hectare. AmountOfNitrogenAppliedFromManure is the total field N from manure +
        // digestate + grazing-deposited manure.
        NitrogenAppliedFromManure = viewItem.AmountOfNitrogenAppliedFromManure,
        DirectN2OPerHectare = viewItem.TotalDirectNitrousOxidePerHectare,
        IndirectN2OPerHectare = viewItem.TotalIndirectNitrousOxidePerHectare,
    };
}
