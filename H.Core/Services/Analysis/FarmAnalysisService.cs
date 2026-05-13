using System.Diagnostics;
using H.Core.Calculators.Shelterbelt;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Models.LandManagement.Shelterbelt;
using H.Core.Models.Results;
using H.Core.Services.Animals;
using H.Core.Services.LandManagement;
using H.Infrastructure;

namespace H.Core.Services.Analysis;

/// <inheritdoc />
public class FarmAnalysisService : IFarmAnalysisService
{
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

        var totalSw = Stopwatch.StartNew();
        long animalMs, fieldMs, shelterbeltMs, mapMs;

        // The field results service expects animal emissions to be populated before calculating
        // soil carbon results, because grazing / manure inputs feed the residue C/N calculations.
        var sw = Stopwatch.StartNew();
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
        // Output > Debug pane (where Trace.* lands) and the Trace listener wired up by
        // FieldResultsService.HTraceListener.
        Trace.WriteLine(
            $"[GHGAnalysis] Farm='{farm.Name}' total={totalSw.ElapsedMilliseconds}ms " +
            $"animal={animalMs}ms field={fieldMs}ms shelterbelt={shelterbeltMs}ms map={mapMs}ms " +
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
        // farm — mirroring that split here keeps both code paths consistent.
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

        // N₂O coverage (Phase 6.4): manure/digestate/grazing N flows through
        // FieldResultsService.CalculateNitrogenAtInterval into TotalDirect/IndirectNitrousOxide
        // per hectare. AmountOfNitrogenAppliedFromManure is the total field N from manure +
        // digestate + grazing-deposited manure.
        NitrogenAppliedFromManure = viewItem.AmountOfNitrogenAppliedFromManure,
        DirectN2OPerHectare = viewItem.TotalDirectNitrousOxidePerHectare,
        IndirectN2OPerHectare = viewItem.TotalIndirectNitrousOxidePerHectare,
    };
}
