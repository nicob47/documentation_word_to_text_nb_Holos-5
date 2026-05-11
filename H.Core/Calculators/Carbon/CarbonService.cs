using H.Core.Emissions.Results;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services;
using H.Core.Services.Animals;

namespace H.Core.Calculators.Carbon;

/// <summary>
/// Routes carbon-input and carbon-loss calculations to either IPCC Tier 2 or ICBM depending on
/// crop support and farm settings, and orchestrates the per-field aggregations (combining main
/// and cover crop inputs, summing C lost from bale exports, accounting for grazing-animal C).
/// </summary>
public class CarbonService : ICarbonService
{
    #region Fields

    private readonly IIPCCTier2CarbonInputCalculator _ipccTier2CarbonInputCalculator;
    private readonly IICBMCarbonInputCalculator _icbmCarbonInputCalculator;
    private readonly IAnimalService _animalService;
    private readonly IFieldComponentHelper _fieldComponentHelper;

    #endregion

    #region Constructors

    public CarbonService()
    {
        _ipccTier2CarbonInputCalculator = new IPCCTier2CarbonInputCalculator();
        _icbmCarbonInputCalculator = new ICBMCarbonInputCalculator();
        _animalService = new AnimalResultsService();
        _fieldComponentHelper = new FieldComponentHelper();
    }

    #endregion

    #region Public Methods

    public bool CanCalculateInputsUsingIpccTier2(CropViewItem cropViewItem)
    {
        return _ipccTier2CarbonInputCalculator.CanCalculateInputsForCrop(cropViewItem);
    }

    public void AssignInputsAndLosses(AdjoiningYears tuple, Farm farm,
        List<AnimalComponentEmissionsResults> animalResults)
    {
        this.AssignInputsAndLosses(tuple.PreviousYearViewItem!, tuple.CurrentYearViewItem!, tuple.NextYearViewItem!, farm, animalResults);
    }

    public void AssignInputsAndLosses(CropViewItem previousYear, CropViewItem viewItem, CropViewItem nextYear,
        Farm farm, List<AnimalComponentEmissionsResults> animalResults)
    {
        this.AssignInputs(previousYear, viewItem, nextYear, farm, animalResults);
        this.CalculateLosses(viewItem, farm);
    }

    public void AssignInputsAndLosses(List<CropViewItem> viewItems, Farm farm,
        List<AnimalComponentEmissionsResults> animalResults)
    {
        this.AssignInputs(viewItems, farm, animalResults);
        this.CalculateLosses(viewItems, farm);
    }

    public void CalculateLosses(List<CropViewItem> viewItems, Farm farm)
    {
        foreach (var viewItem in viewItems.OrderBy(x => x.Year))
        {
            this.CalculateLosses(viewItem, farm);
        }
    }

    public void AssignInputs(List<CropViewItem> cropViewItems, Farm farm,
        List<AnimalComponentEmissionsResults> animalResults)
    {
        var orderedItems = cropViewItems.OrderBy(x => x.Year).ToList();
        foreach (var item in orderedItems)
        {
            var tuple = _fieldComponentHelper.GetAdjoiningYears(orderedItems, item.Year);
            this.AssignInputs(
                previousYear: tuple.PreviousYearViewItem!,
                viewItem: tuple.CurrentYearViewItem!,
                nextYear: tuple.NextYearViewItem!,
                farm: farm,
                animalResults: animalResults);
        }
    }

    public void AssignInputs(CropViewItem previousYear, CropViewItem viewItem, CropViewItem nextYear, Farm farm,
        List<AnimalComponentEmissionsResults> animalResults)
    {
        var canUseTier2 = this.CanCalculateInputsUsingIpccTier2(viewItem);

        if (farm.IsCommandLineMode)
        {
            this.AssignInputsForCommandLine(previousYear, viewItem, nextYear, farm, animalResults, canUseTier2);
        }
        else
        {
            // GUI mode.
            //
            // If the user selected ICBM as the modelling strategy, we must use the ICBM input calculator
            // even when Tier 2 could handle this crop, because ICBM assigns CarbonInputFromProduct/Straw/
            // Roots/Extraroots — which the N-budget needs in
            // NitrogenService.CalculateAboveGroundResidueNitrogen. Greedily picking Tier 2 here would
            // leave AboveGroundCarbonInput non-zero but the per-residue C properties at zero, producing
            // no N from crop residues.
            var useTier2 = farm.Defaults.CarbonModellingStrategy == CarbonModellingStrategies.IPCCTier2 && canUseTier2;
            if (useTier2)
            {
                _ipccTier2CarbonInputCalculator.AssignInputs(viewItem, farm, animalResults);
            }
            else
            {
                _icbmCarbonInputCalculator.AssignInputs(previousYear, viewItem, nextYear, farm, animalResults);
            }
        }
    }

    public void CalculateLosses(CropViewItem cropViewItem, Farm farm)
    {
        this.CalculateCarbonLostFromHayExports(farm, cropViewItem);
    }

    public double SumTotalAbovegroundCarbonInput(List<CropViewItem> viewItems) =>
        viewItems.Sum(x => x.AboveGroundCarbonInput);

    public double SumTotalBelowgroundCarbonInput(List<CropViewItem> viewItems) =>
        viewItems.Sum(x => x.BelowGroundCarbonInput);

    public double SumTotalManureCarbonInput(List<CropViewItem> viewItems) =>
        viewItems.Sum(x => x.ManureCarbonInputsPerHectare);

    public double SumTotalDigestateCarbonInput(List<CropViewItem> viewItems) =>
        viewItems.Sum(x => x.DigestateCarbonInputsPerHectare);

    /// <summary>
    /// (kg C ha^-1)
    /// </summary>
    public double CalculateManureCarbonInputFromGrazingAnimals(FieldSystemComponent fieldSystemComponent,
        CropViewItem cropViewItem,
        List<AnimalComponentEmissionsResults> results, Farm farm)
    {
        var result = 0d;

        var grazingItems = cropViewItem.GrazingViewItems
            .Where(x => x.Start.Year == cropViewItem.Year)
            .ToList();

        foreach (var grazingViewItem in grazingItems)
        {
            foreach (var groupEmissionsByMonth in _animalService.GetGroupEmissionsFromGrazingAnimals(results, grazingViewItem))
            {
                result += (groupEmissionsByMonth.MonthlyFecalCarbonExcretion -
                           groupEmissionsByMonth.MonthlyManureMethaneEmission) / cropViewItem.Area;
            }
        }

        return result < 0 ? 0 : result;
    }

    /// <summary>
    /// Equation 5.6.1-1
    ///
    /// (kg C ha^-1)
    /// </summary>
    public void CalculateManureCarbonInputByGrazingAnimals(FieldSystemComponent fieldSystemComponent,
        IEnumerable<AnimalComponentEmissionsResults> results,
        List<CropViewItem> cropViewItems, Farm farm)
    {
        var resultsList = results.ToList();
        foreach (var cropViewItem in cropViewItems)
        {
            cropViewItem.TotalCarbonInputFromManureFromAnimalsGrazingOnPasture =
                this.CalculateManureCarbonInputFromGrazingAnimals(fieldSystemComponent, cropViewItem, resultsList, farm);
        }
    }

    public double CalculateInputsFromSupplementalHayFedToGrazingAnimals(CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem, CropViewItem nextYearViewItems, Farm farm)
    {
        return _icbmCarbonInputCalculator.CalculateInputsFromSupplementalHayFedToGrazingAnimals(
            previousYearViewItem, currentYearViewItem, nextYearViewItems, farm);
    }

    /// <summary>
    /// (kg C)
    /// </summary>
    public double GetSupplementalLosses(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        CropViewItem nextYearViewItems,
        Farm farm)
    {
        return _icbmCarbonInputCalculator.GetSupplementalLosses(
            previousYearViewItem, currentYearViewItem, nextYearViewItems, farm);
    }

    public void CalculateCarbonLostFromHayExports(Farm farm, CropViewItem cropViewItem)
    {
        var dryMatter = this.CalculateTotalDryMatterLossFromResidueExports(cropViewItem, farm);

        cropViewItem.TotalDryMatterLostFromBaleExports = dryMatter;
        cropViewItem.TotalCarbonLossFromBaleExports = dryMatter * farm.Defaults.CarbonConcentration;
    }

    /// <summary>
    /// Total dry matter lost from a field once amounts re-imported as supplemental hay elsewhere
    /// on the farm have been deducted.
    ///
    /// (kg DM)
    /// </summary>
    public double CalculateTotalDryMatterLossFromResidueExports(CropViewItem cropViewItem, Farm farm)
    {
        var field = farm.GetFieldSystemComponent(cropViewItem.FieldSystemComponentGuid);
        if (field == null || !field.HasHayHarvestInYear(cropViewItem.Year))
        {
            return 0;
        }

        var dryMatterHarvested = cropViewItem.GetHayHarvestsByYear(cropViewItem.Year).Sum(x => x.AboveGroundBiomassDryWeight);
        var dryMatterImported = farm.GetHayImportsUsingImportedHayFromSourceFieldByYear(cropViewItem.FieldSystemComponentGuid, cropViewItem.Year)
            .Sum(x => x.AboveGroundBiomassDryWeight);

        var dryMatter = dryMatterHarvested - dryMatterImported;
        return dryMatter < 0 ? 0 : dryMatter;
    }

    /// <summary>
    /// Equation 11.3.2-4
    /// </summary>
    public void CalculateCarbonLostByGrazingAnimals(
        Farm farm,
        FieldSystemComponent fieldSystemComponent,
        IEnumerable<AnimalComponentEmissionsResults> animalComponentEmissionsResults,
        List<CropViewItem> viewItems)
    {
        var resultsList = animalComponentEmissionsResults.ToList();
        foreach (var cropViewItem in viewItems)
        {
            if (cropViewItem.HarvestMethod == HarvestMethods.StubbleGrazing)
            {
                continue;
            }

            var totalCarbonLossesForField = 0d;
            var totalCarbonUptakeForField = 0d;

            foreach (var componentResults in resultsList)
            {
                if (componentResults.Component is not AnimalComponentBase animalComponentBase)
                {
                    continue;
                }

                foreach (var animalGroup in animalComponentBase.Groups)
                {
                    var grazingManagementPeriodsByGroup = _animalService
                        .GetGrazingManagementPeriods(animalGroup, fieldSystemComponent)
                        .Where(x => x.Start.Year == cropViewItem.Year)
                        .ToList();

                    var (lossesForPeriods, uptakeForPeriods) = this.CalculateUptakeByGrazingAnimals(
                        grazingManagementPeriodsByGroup, cropViewItem, animalGroup, fieldSystemComponent, farm, animalComponentBase);

                    totalCarbonLossesForField += lossesForPeriods;
                    totalCarbonUptakeForField += uptakeForPeriods;
                }
            }

            var amountsNotEaten = this.GetSupplementalLosses(null!, cropViewItem, null!, farm);

            cropViewItem.TotalCarbonLossesByGrazingAnimals = totalCarbonLossesForField - amountsNotEaten;
            cropViewItem.TotalCarbonUptakeByAnimals = totalCarbonUptakeForField;
        }
    }

    /// <summary>
    /// Once C inputs have been determined for all crops in a year, combine the main and cover crops:
    /// the main crop is what feeds the C/N models, so its <see cref="CropViewItem.CombinedAboveGroundInput"/>
    /// and friends absorb the secondary crops' contributions.
    /// </summary>
    public void CombineCarbonInputs(Farm farm, List<CropViewItem> viewItems)
    {
        foreach (var year in viewItems.GetDistinctYears())
        {
            var viewItemsForYear = viewItems.GetItemsByYear(year);
            var mainCropForYear = _fieldComponentHelper.GetMainCropForYear(viewItemsForYear, year);
            if (mainCropForYear == null)
            {
                continue;
            }

            var secondaryCropsForYear = viewItemsForYear.GetSecondaryCrops(mainCropForYear);

            var coverAbove = this.SumTotalAbovegroundCarbonInput(secondaryCropsForYear);
            var coverBelow = this.SumTotalBelowgroundCarbonInput(secondaryCropsForYear);
            var coverManure = this.SumTotalManureCarbonInput(secondaryCropsForYear);
            var coverDigestate = this.SumTotalDigestateCarbonInput(secondaryCropsForYear);

            mainCropForYear.CombinedAboveGroundInput = mainCropForYear.AboveGroundCarbonInput + coverAbove;
            mainCropForYear.CombinedBelowGroundInput = mainCropForYear.BelowGroundCarbonInput + coverBelow;
            mainCropForYear.CombinedManureInput = mainCropForYear.ManureCarbonInputsPerHectare + coverManure;
            mainCropForYear.CombinedDigestateInput = mainCropForYear.DigestateCarbonInputsPerHectare + coverDigestate;
            mainCropForYear.TotalCarbonInputs =
                mainCropForYear.CombinedAboveGroundInput +
                mainCropForYear.CombinedBelowGroundInput +
                mainCropForYear.CombinedManureInput +
                mainCropForYear.CombinedDigestateInput;
        }
    }

    public Tuple<double, double> CalculateUptakeByGrazingAnimals(
        List<ManagementPeriod> managementPeriods,
        CropViewItem viewItem,
        AnimalGroup animalGroup,
        FieldSystemComponent fieldSystemComponent,
        Farm farm,
        AnimalComponentBase animalComponent)
    {
        if (managementPeriods.Count == 0)
        {
            return new Tuple<double, double>(0, 0);
        }

        var averageUtilizationRate = viewItem.GrazingViewItems.Any()
            ? viewItem.GrazingViewItems.Average(x => x.Utilization)
            : 0;

        if (managementPeriods.Count == 1)
        {
            // Equations 11.3.2-4 / 11.3.2-5. The result is reduced by area in Eq. 11.3.2-9.
            var resultsForPeriod = _animalService.GetResultsForManagementPeriod(animalGroup, farm, animalComponent, managementPeriods.Single());
            var totalCarbonUptake = resultsForPeriod.TotalCarbonUptakeByAnimals();

            var utilizationRate = averageUtilizationRate / 100.0;
            if (utilizationRate <= 0)
            {
                utilizationRate = 1;
            }

            return new Tuple<double, double>(totalCarbonUptake / utilizationRate, totalCarbonUptake);
        }

        // Multiple management periods: total uptake sums all periods; carbon-losses term scales only
        // the last period by the utilization rate (Eq. 11.3.2-6 / 11.3.2-7).
        var orderedPeriods = managementPeriods.OrderBy(x => x.Start).ToList();
        var lastPeriod = orderedPeriods[^1];
        var resultsForLast = _animalService.GetResultsForManagementPeriod(animalGroup, farm, animalComponent, lastPeriod);
        var totalCarbonUptakeForLastPeriod = resultsForLast.TotalCarbonUptakeByAnimals();

        var denominator = averageUtilizationRate / 100;
        if (denominator <= 0)
        {
            denominator = 1;
        }
        var carbonUptakeForLastPeriod = totalCarbonUptakeForLastPeriod / denominator;

        var totalCarbonUptakeAcrossAllPeriods = totalCarbonUptakeForLastPeriod;
        var carbonUptakeForOtherPeriods = 0d;
        for (int i = 0; i < orderedPeriods.Count - 1; i++)
        {
            var resultsForPeriod = _animalService.GetResultsForManagementPeriod(animalGroup, farm, animalComponent, orderedPeriods[i]);
            var uptakeForPeriod = resultsForPeriod.TotalCarbonUptakeByAnimals();
            totalCarbonUptakeAcrossAllPeriods += uptakeForPeriod;
            carbonUptakeForOtherPeriods += uptakeForPeriod;
        }

        return new Tuple<double, double>(carbonUptakeForOtherPeriods + carbonUptakeForLastPeriod, totalCarbonUptakeAcrossAllPeriods);
    }

    #endregion

    #region Private Methods

    private void AssignInputsForCommandLine(
        CropViewItem previousYear,
        CropViewItem viewItem,
        CropViewItem nextYear,
        Farm farm,
        List<AnimalComponentEmissionsResults> animalResults,
        bool canUseTier2)
    {
        // CLI mode supports overriding the default residue-input calculation method.
        var residueInputMethod = farm.Defaults.ResidueInputCalculationMethod;

        if (residueInputMethod == ResidueInputCalculationMethod.ICBM)
        {
            _icbmCarbonInputCalculator.AssignInputs(previousYear, viewItem, nextYear, farm, animalResults);
            return;
        }

        if (residueInputMethod == ResidueInputCalculationMethod.IPCCTier2 && canUseTier2)
        {
            _ipccTier2CarbonInputCalculator.AssignInputs(viewItem, farm, animalResults);
            return;
        }

        // Default fallback: when ICBM is the active modelling strategy we must use the ICBM input
        // calculator (it populates CarbonInputFromProduct/Straw/Roots/Extraroots, which the N
        // calculations need). Otherwise prefer Tier 2 when the crop supports it.
        if (farm.Defaults.CarbonModellingStrategy == CarbonModellingStrategies.ICBM || !canUseTier2)
        {
            _icbmCarbonInputCalculator.AssignInputs(previousYear, viewItem, nextYear, farm, animalResults);
        }
        else
        {
            _ipccTier2CarbonInputCalculator.AssignInputs(viewItem, farm, animalResults);
        }
    }

    #endregion
}
