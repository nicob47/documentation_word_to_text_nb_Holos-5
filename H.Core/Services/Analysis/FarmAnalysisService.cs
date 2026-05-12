using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
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

    public FarmAnalysisService(IFieldResultsService fieldResultsService, IAnimalService animalService)
    {
        ArgumentNullException.ThrowIfNull(fieldResultsService);
        ArgumentNullException.ThrowIfNull(animalService);

        _fieldResultsService = fieldResultsService;
        _animalService = animalService;
    }

    public FarmAnalysisResults RunAnalysis(Farm farm)
    {
        ArgumentNullException.ThrowIfNull(farm);

        // The field results service expects animal emissions to be populated before calculating
        // soil carbon results, because grazing / manure inputs feed the residue C/N calculations.
        _fieldResultsService.AnimalResults = _animalService.GetAnimalResults(farm);

        var detailViewItems = _fieldResultsService.CalculateFinalResults(farm);

        return new FarmAnalysisResults
        {
            FarmName = farm.Name ?? string.Empty,
            Province = farm.Province.GetDescription() ?? string.Empty,
            CarbonModellingStrategy = farm.Defaults.CarbonModellingStrategy.GetDescription() ?? string.Empty,
            YearResults = detailViewItems
                .OrderBy(v => v.FieldName)
                .ThenBy(v => v.Year)
                .Select(ToYearResult)
                .ToList(),
        };
    }

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
    };
}
