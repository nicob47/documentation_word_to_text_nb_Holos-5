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

/// <summary>
/// GUI-facing façade that orchestrates the carbon / nitrogen analysis pipeline and maps the
/// result into a flat <see cref="FarmAnalysisResults"/> DTO.
///
/// <para><b>What it does, in order:</b></para>
/// <list type="number">
///   <item>Validate the farm has a supported Canadian province (defense-in-depth — the GUI shouldn't let a non-Canadian value reach here, but a v4 import can sneak one in).</item>
///   <item>Lazy-build the per-year detail view items via <see cref="IFieldResultsService.InitializeStageState"/> if they aren't already populated. The Avalonia GUI lets users jump straight to results without touching the legacy details screen that v4's WPF GUI used to build the stage state for them, so the analysis service has to do it.</item>
///   <item>Prime <c>_fieldResultsService.AnimalResults</c> with the animal emissions before the field-level math runs (the nitrogen calculator reads grazing/manure deposits from there).</item>
///   <item>Run <see cref="IFieldResultsService.CalculateFinalResults(Farm)"/> for the field-level ICBM / Tier 2 math.</item>
///   <item>Run the shelterbelt calculator orthogonally (only when the farm has shelterbelt components — uses an allometric model, not soil-pool dynamics).</item>
///   <item>Map both into the <see cref="FarmAnalysisResults"/> DTO so view models can bind without depending on <c>CropViewItem</c>.</item>
/// </list>
///
/// <para>
/// Emits a <c>[GHGAnalysis]</c> trace line with a per-phase ms breakdown — useful for spotting
/// regressions without attaching a profiler. See <c>Carbon_Model_Flow.md</c> for the full
/// pipeline diagram.
/// </para>
/// </summary>
public class FarmAnalysisService : IFarmAnalysisService
{
    // NLog logger. Replaces legacy Trace.TraceError/Warning/Information/WriteLine calls so every
    // log line in the codebase goes through the single NLog pipeline configured in NLog.config.
    private static readonly Logger _log = LogManager.GetCurrentClassLogger();

    private readonly IFieldResultsService _fieldResultsService;
    private readonly IAnimalService _animalService;
    private readonly ShelterbeltCalculator _shelterbeltCalculator;

    /// <summary>
    /// Constructs the façade with the three calculation services it orchestrates. All are
    /// required; passing <c>null</c> for any throws <see cref="ArgumentNullException"/>.
    /// </summary>
    /// <param name="fieldResultsService">Owns the per-year detail view items and runs the field-level ICBM / Tier 2 math.</param>
    /// <param name="animalService">Computes per-component animal emissions; primed into <c>_fieldResultsService.AnimalResults</c> before the field pass so nitrogen calc can fold in grazing / manure N.</param>
    /// <param name="shelterbeltCalculator">Allometric shelterbelt biomass / DOM / ecosystem-C model. Orthogonal to the soil-pool calculations.</param>
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

    /// <inheritdoc />
    /// <remarks>
    /// Synchronous — runs the full carbon + nitrogen pipeline on the calling thread. The Avalonia
    /// VM (<c>GHGResultsViewModel.RunAnalysisAsync</c>) wraps the call in <c>Task.Run</c> so the
    /// UI thread stays responsive while this runs.
    ///
    /// <para>Throws <see cref="InvalidOperationException"/> when the farm's province is outside
    /// the Canadian whitelist (Guard A) — the GUI catches the exception and surfaces the message
    /// via the existing <c>LastErrorMessage</c> banner.</para>
    /// </remarks>
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

    /// <summary>
    /// Runs the shelterbelt allometric model for any shelterbelt components on the farm and maps
    /// the per-component, per-year <c>TrannumResultViewItem</c>s into flat
    /// <see cref="ShelterbeltYearResult"/> rows. Returns an empty collection when the farm has no
    /// shelterbelt components — orthogonal to the field-level ICBM / Tier 2 path, so we can
    /// short-circuit cheaply.
    /// </summary>
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

    /// <summary>
    /// Maps a per-component, per-year <see cref="TrannumResultViewItem"/> (the v4 shelterbelt
    /// calculator's internal row type) into the flat <see cref="ShelterbeltYearResult"/> DTO.
    /// Carbon quantities are kept in their native Mg C km⁻¹ — see the DTO XML doc for units.
    /// </summary>
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

    /// <summary>
    /// Maps a single merged-per-year <see cref="CropViewItem"/> into the
    /// <see cref="FieldAnalysisYearResult"/> DTO that the GUI binds to.
    ///
    /// <para><b>Combined* vs per-crop field selection:</b></para>
    /// The <c>Combined*</c> fields hold "main crop + cover crop" inputs after
    /// <c>CarbonService.CombineCarbonInputs</c> has run. For simple fields without a cover crop
    /// those fields are zero, so we fall back to the main-crop-only values
    /// (<c>AboveGroundCarbonInput</c> et al). If neither has been populated the user will see
    /// zeros — that's a legitimate "no inputs assigned" signal rather than something to paper over.
    ///
    /// <para>SoilCarbon is in kg C ha⁻¹ (not Mg) — see the DTO XML doc.</para>
    /// </summary>
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
