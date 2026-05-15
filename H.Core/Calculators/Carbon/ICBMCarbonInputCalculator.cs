using H.Core.Emissions.Results;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;

namespace H.Core.Calculators.Carbon;

/// <summary>
/// ICBM-specific carbon-input math: turns yield + crop type + manure / digestate / grazing
/// inputs into the per-hectare <c>AboveGroundCarbonInput</c> / <c>BelowGroundCarbonInput</c> /
/// <c>ManureCarbonInputsPerHectare</c> / <c>DigestateCarbonInputsPerHectare</c> fields that
/// the ICBM pool dynamics consume.
///
/// <para><b>Relationship to <see cref="ICBMSoilCarbonCalculator"/>:</b></para>
/// The input-side math currently lives in both this class and on the soil-carbon calculator.
/// The Phase 4 follow-up list in <c>MEMORY.md</c> tracks the migration that strips the
/// duplicates from <see cref="ICBMSoilCarbonCalculator"/> in favour of this calculator.
///
/// <para><b>Perennial-root floor:</b></para>
/// <see cref="MinimumPerennialRootCarbonInput"/> = 450 kg C ha⁻¹ — Holos won't let perennial
/// root-C input fall below this regardless of yield, matching v4's behaviour. The annual
/// increase percentage (<see cref="PerennialRootCarbonAnnualIncreasePercent"/> = 19.35%) is
/// applied year-over-year for stands that haven't reached the maximum yet.
/// </summary>
public class ICBMCarbonInputCalculator : CarbonInputCalculatorBase, IICBMCarbonInputCalculator
{
    /// <summary>Moisture content (fraction) used to convert fresh-biomass yields to dry matter.</summary>
    private const double MoistureContentFreshBiomass = 0.8;

    /// <summary>Moisture content (fraction) of air-dried hay used in supplemental-feed math.</summary>
    private const double MoistureContentAirDriedHay = 0.13;

    /// <summary>Floor for perennial root C input regardless of low yield (kg C ha⁻¹). Matches v4.</summary>
    private const double MinimumPerennialRootCarbonInput = 450;

    /// <summary>Year-over-year increase percentage for perennial root C input until the maximum is reached.</summary>
    private const double PerennialRootCarbonAnnualIncreasePercent = 19.35;

    #region Public Methods

    public CropViewItem AssignInputs(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        CropViewItem nextYearViewItem,
        Farm farm,
        List<AnimalComponentEmissionsResults> animalResults)
    {
        manureService.Initialize(farm, animalResults);
        digestateService.Initialize(farm, animalResults);

        var isNonSwathingGrazingScenario = farm.IsNonSwathingGrazingScenario(currentYearViewItem);

        currentYearViewItem.PlantCarbonInAgriculturalProduct = this.CalculatePlantCarbonInAgriculturalProduct(
            previousYearViewItem: previousYearViewItem,
            currentYearViewItem: currentYearViewItem,
            farm: farm);

        currentYearViewItem.CarbonInputFromProduct = this.CalculateCarbonInputFromProduct(
            previousYearViewItem: previousYearViewItem,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItem: nextYearViewItem,
            farm: farm);

        if (isNonSwathingGrazingScenario)
        {
            // Total C losses from grazing animals is calculated in Eq. 11.3.2-4.

            // Eq. 11.3.2-6
            currentYearViewItem.PlantCarbonInAgriculturalProduct =
                currentYearViewItem.TotalCarbonLossesByGrazingAnimals / currentYearViewItem.Area;

            // Eq. 11.3.2-7
            currentYearViewItem.CarbonInputFromProduct =
                (currentYearViewItem.TotalCarbonLossesByGrazingAnimals - currentYearViewItem.TotalCarbonUptakeByAnimals)
                / currentYearViewItem.Area;

            // Eq. 11.3.2-9
            var moistureContent = currentYearViewItem.GrazingViewItems.Any()
                ? currentYearViewItem.GrazingViewItems.Average(x => x.MoistureContentAsPercentage)
                : 1;
            var totalYieldForArea = (currentYearViewItem.TotalCarbonLossesByGrazingAnimals / farm.Defaults.CarbonConcentration)
                                    / (1 - (moistureContent / 100.0));

            currentYearViewItem.Yield = totalYieldForArea / currentYearViewItem.Area;
        }

        currentYearViewItem.CarbonInputFromStraw = this.CalculateCarbonInputFromStraw(
            previousYearViewItem: previousYearViewItem,
            currentYearViewItem: currentYearViewItem,
            farm: farm);

        currentYearViewItem.CarbonInputFromRoots = this.CalculateCarbonInputFromRoots(
            previousYearViewItem: previousYearViewItem,
            currentYearViewItem: currentYearViewItem,
            farm: farm);

        currentYearViewItem.CarbonInputFromExtraroots = this.CalculateCarbonInputFromExtraroot(
            previousYearViewItem: previousYearViewItem,
            currentYearViewItem: currentYearViewItem,
            farm: farm);

        currentYearViewItem.AboveGroundCarbonInput = this.CalculateTotalAboveGroundCarbonInput(
            cropViewItem: currentYearViewItem,
            farm: farm);

        // Add any supplemental feeding amounts that were given to grazing animals
        currentYearViewItem.AboveGroundCarbonInput += this.CalculateInputsFromSupplementalHayFedToGrazingAnimals(
            previousYearViewItem: previousYearViewItem,
            currentYearViewItem: currentYearViewItem,
            nextYearViewItems: nextYearViewItem,
            farm: farm);

        currentYearViewItem.BelowGroundCarbonInput = this.CalculateTotalBelowGroundCarbonInput(
            cropViewItem: currentYearViewItem,
            farm: farm);

        this.AssignManureInputs(previousYearViewItem, currentYearViewItem, nextYearViewItem, farm, animalResults);

        currentYearViewItem.DigestateCarbonInputsPerHectare =
            digestateService.GetTotalDigestateCarbonInputsForField(farm, currentYearViewItem.Year, currentYearViewItem);
        currentYearViewItem.DigestateCarbonInputsPerHectareFromApplicationsOnly =
            currentYearViewItem.GetTotalCarbonFromAppliedDigestate(ManureLocationSourceType.Livestock) / currentYearViewItem.Area;

        currentYearViewItem.TotalCarbonInputs =
            currentYearViewItem.AboveGroundCarbonInput +
            currentYearViewItem.BelowGroundCarbonInput +
            currentYearViewItem.ManureCarbonInputsPerHectare +
            currentYearViewItem.DigestateCarbonInputsPerHectare;

        return currentYearViewItem;
    }

    public void AssignManureInputs(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        CropViewItem nextYearViewItem,
        Farm farm,
        List<AnimalComponentEmissionsResults> animalResults)
    {
        base.AssignManureCarbonInputs(currentYearViewItem, farm, animalResults);
    }

    public double CalculatePlantCarbonInAgriculturalProduct(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        Farm farm)
    {
        if (currentYearViewItem.DoNotRecalculatePlantCarbonInAgriculturalProduct)
        {
            return currentYearViewItem.PlantCarbonInAgriculturalProduct;
        }

        if (currentYearViewItem.CropType.IsFallow() || currentYearViewItem.CropType == CropType.NotSelected)
        {
            return 0;
        }

        // Old farms fix: legacy data stored moisture as a percentage (>1) instead of a fraction.
        if (currentYearViewItem.MoistureContentOfCrop > 1)
        {
            currentYearViewItem.MoistureContentOfCrop /= 100;
        }

        var isGrazed = currentYearViewItem.GrazingViewItems.Any();
        var moistureContentFraction = isGrazed
            ? currentYearViewItem.GrazingViewItems.Average(x => x.MoistureContentAsPercentage) / 100.0
            : currentYearViewItem.MoistureContentOfCrop;

        var isCustomYieldAssignmentMethod = farm.YieldAssignmentMethod == YieldAssignmentMethod.Custom;
        var isAllProductReturned = Math.Abs(currentYearViewItem.PercentageOfProductYieldReturnedToSoil - 100) < double.Epsilon;
        var isSwathing = currentYearViewItem.HarvestMethod == HarvestMethods.Swathing;
        var isGreenManure = currentYearViewItem.HarvestMethod == HarvestMethods.GreenManure;
        var isCustomYieldAndIsGrazed = isCustomYieldAssignmentMethod && isGrazed;
        var hasHarvest = currentYearViewItem.GetHayHarvests().Any();
        var isCustomYieldAndNoHarvestAndNoGrazing = isCustomYieldAssignmentMethod && !hasHarvest && !isGrazed;

        var moistureContentAdjustment = 1.0 - moistureContentFraction;
        var carbonConcentration = currentYearViewItem.CarbonConcentration;
        var yield = currentYearViewItem.Yield;

        if (isAllProductReturned || isSwathing || isGreenManure || isCustomYieldAndIsGrazed || isCustomYieldAndNoHarvestAndNoGrazing)
        {
            return yield * moistureContentAdjustment * carbonConcentration;
        }

        var grossYield = yield / (1 - (currentYearViewItem.PercentageOfProductYieldReturnedToSoil / 100.0));
        return grossYield * moistureContentAdjustment * carbonConcentration;
    }

    public double EstimatePlantCarbonInAgriculturalProductForNextYear(
        CropViewItem nextYearViewItem,
        Farm farm)
    {
        if (nextYearViewItem == null)
        {
            return 0;
        }

        var year = nextYearViewItem.Year;
        return this.CalculateProductivity(
            annualPrecipitation: farm.ClimateData.GetTotalPrecipitationForYear(year),
            annualPotentialEvapotranspiration: farm.ClimateData.GetTotalEvapotranspirationForYear(year),
            proportionOfPrecipitationMayThroughSeptember: farm.ClimateData.ProportionOfPrecipitationFallingInMayThroughSeptember(year),
            carbonConcentration: nextYearViewItem.CarbonConcentration);
    }

    public double CalculateProductivity(
        double annualPrecipitation,
        double annualPotentialEvapotranspiration,
        double proportionOfPrecipitationMayThroughSeptember,
        double carbonConcentration)
    {
        // See https://github.com/holos-aafc/Holos/issues/405 for the moisture-correction rationale.
        var moistureCorrection = (1.0 - MoistureContentAirDriedHay) / (1.0 - MoistureContentFreshBiomass);

        var production = 2.973
                         + (0.00453 * annualPrecipitation)
                         + (-0.00259 * annualPotentialEvapotranspiration)
                         + (6.187 * proportionOfPrecipitationMayThroughSeptember);

        return Math.Exp(production) * moistureCorrection * carbonConcentration;
    }

    public double CalculateAboveGroundCarbonInputFromPerennials(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        CropViewItem nextYearViewItem,
        Farm farm)
    {
        var estimatedPlantCarbonInAgriculturalProductInNextYear =
            this.EstimatePlantCarbonInAgriculturalProductForNextYear(currentYearViewItem, farm);

        var productReturnedFraction = currentYearViewItem.PercentageOfProductYieldReturnedToSoil / 100;

        if (currentYearViewItem.YearInPerennialStand == 1)
        {
            return CalculateFirstYearPerennialCarbonInput(
                currentYearViewItem,
                nextYearViewItem,
                farm,
                estimatedPlantCarbonInAgriculturalProductInNextYear,
                productReturnedFraction);
        }

        // Eq. 2.1.2-27 — any year other than the first.
        if (currentYearViewItem.PlantCarbonInAgriculturalProduct > 0)
        {
            return CarbonInputFromKnownPlantCarbon(currentYearViewItem, farm, productReturnedFraction);
        }

        // C_p for the current year is unknown; fall back to the estimated value.
        var estimatedPlantC = estimatedPlantCarbonInAgriculturalProductInNextYear * farm.Defaults.EstablishmentGrowthFactorFractionForPerennials;
        currentYearViewItem.PlantCarbonInAgriculturalProduct = estimatedPlantC;
        return estimatedPlantC * productReturnedFraction;
    }

    private double CalculateFirstYearPerennialCarbonInput(
        CropViewItem currentYearViewItem,
        CropViewItem nextYearViewItem,
        Farm farm,
        double estimatedPlantCarbonInAgriculturalProductInNextYear,
        double productReturnedFraction)
    {
        if (currentYearViewItem.PlantCarbonInAgriculturalProduct > 0)
        {
            // Eq. 2.1.2-20: C_p for the current year is known.
            return CarbonInputFromKnownPlantCarbon(currentYearViewItem, farm, productReturnedFraction);
        }

        if (nextYearViewItem != null && (nextYearViewItem.PlantCarbonInAgriculturalProduct > 0 || nextYearViewItem.Yield > 0))
        {
            // Eq. 2.1.2-21: derive this year's C_p from the next year's known/calculable C_p.
            var plantCarbonInAgriculturalProductForNextYear = nextYearViewItem.PlantCarbonInAgriculturalProduct > 0
                ? nextYearViewItem.PlantCarbonInAgriculturalProduct
                : this.CalculatePlantCarbonInAgriculturalProduct(null!, nextYearViewItem, farm);

            // Eq. 2.1.2-23
            var thisYearsPlantCarbon = plantCarbonInAgriculturalProductForNextYear * farm.Defaults.EstablishmentGrowthFactorFractionForPerennials;

            // Assign now so C_r and C_e for this year have a C_p to work with.
            currentYearViewItem.PlantCarbonInAgriculturalProduct = thisYearsPlantCarbon;

            // Eq. 2.1.2-24
            return thisYearsPlantCarbon * productReturnedFraction;
        }

        // Neither current nor next year has C_p; use the estimated value.
        var thisYearsEstimated = estimatedPlantCarbonInAgriculturalProductInNextYear * farm.Defaults.EstablishmentGrowthFactorFractionForPerennials;
        currentYearViewItem.PlantCarbonInAgriculturalProduct = thisYearsEstimated;

        if (nextYearViewItem != null)
        {
            // Eq. 2.1.2-25 — record the calculated value on next year and lock it from being overwritten.
            nextYearViewItem.PlantCarbonInAgriculturalProduct = estimatedPlantCarbonInAgriculturalProductInNextYear;
            nextYearViewItem.DoNotRecalculatePlantCarbonInAgriculturalProduct = true;
        }

        return thisYearsEstimated * productReturnedFraction;
    }

    private static double CarbonInputFromKnownPlantCarbon(CropViewItem viewItem, Farm farm, double productReturnedFraction)
    {
        var isCustomYieldAndGrazed = farm.YieldAssignmentMethod == YieldAssignmentMethod.Custom && viewItem.HasGrazingViewItems;
        if (isCustomYieldAndGrazed)
        {
            var returned = 1.0 - (viewItem.GetAverageUtilizationFromGrazingAnimals() / 100.0);
            return viewItem.PlantCarbonInAgriculturalProduct * returned;
        }

        return viewItem.PlantCarbonInAgriculturalProduct * productReturnedFraction;
    }

    public double CalculateCarbonInputFromProduct(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        CropViewItem nextYearViewItem,
        Farm farm)
    {
        if (currentYearViewItem.CropType.IsPerennial())
        {
            return this.CalculateAboveGroundCarbonInputFromPerennials(previousYearViewItem, currentYearViewItem, nextYearViewItem, farm);
        }

        return currentYearViewItem.PlantCarbonInAgriculturalProduct * (currentYearViewItem.PercentageOfProductYieldReturnedToSoil / 100.0);
    }

    public double CalculateCarbonInputFromStraw(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        Farm farm)
    {
        // No straw inputs from fallow, missing crop type, green-manure/swathing harvest (combined into product fraction), or perennials.
        if (currentYearViewItem.CropType.IsFallow() ||
            currentYearViewItem.CropType == CropType.NotSelected ||
            currentYearViewItem.HarvestMethod == HarvestMethods.GreenManure ||
            currentYearViewItem.HarvestMethod == HarvestMethods.Swathing ||
            currentYearViewItem.CropType.IsPerennial())
        {
            return 0;
        }

        if (Math.Abs(currentYearViewItem.BiomassCoefficientProduct) < double.Epsilon)
        {
            return 0;
        }

        return currentYearViewItem.PlantCarbonInAgriculturalProduct
               * (currentYearViewItem.BiomassCoefficientStraw / currentYearViewItem.BiomassCoefficientProduct)
               * (currentYearViewItem.PercentageOfStrawReturnedToSoil / 100);
    }

    public double CalculateCarbonInputFromRootsForPerennials(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        Farm farm)
    {
        if (currentYearViewItem.BiomassCoefficientProduct == 0)
        {
            return 0;
        }

        var carbonInput = currentYearViewItem.PlantCarbonInAgriculturalProduct
                          * (currentYearViewItem.BiomassCoefficientRoots / currentYearViewItem.BiomassCoefficientProduct);
        if (carbonInput < MinimumPerennialRootCarbonInput)
        {
            carbonInput = MinimumPerennialRootCarbonInput;
        }

        // Eq. 2.1.2-28
        var carbonInputFromRoots = carbonInput * (currentYearViewItem.PercentageOfRootsReturnedToSoil / 100.0);

        // Don't apply the year-over-year increase in the first or final year of the stand.
        if (currentYearViewItem.YearInPerennialStand == 1 || currentYearViewItem.IsFinalYearInPerennialStand())
        {
            return carbonInputFromRoots;
        }

        // Only carry over from a previous year if it grew the same perennial stand.
        if (previousYearViewItem != null && previousYearViewItem.PerennialStandGroupId.Equals(currentYearViewItem.PerennialStandGroupId))
        {
            // Eq. 2.1.2-30 — apply the 19.35% annual increase, then plateau after year 5.
            carbonInputFromRoots = currentYearViewItem.YearInPerennialStand > 5
                ? previousYearViewItem.CarbonInputFromRoots
                : previousYearViewItem.CarbonInputFromRoots * (1 + (PerennialRootCarbonAnnualIncreasePercent / 100.0));
        }

        return carbonInputFromRoots;
    }

    public double CalculateCarbonInputFromRoots(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        Farm farm)
    {
        if (currentYearViewItem.CropType.IsFallow() || currentYearViewItem.CropType == CropType.NotSelected)
        {
            return 0;
        }

        if (currentYearViewItem.CropType.IsPerennial() || currentYearViewItem.CropType == CropType.GrassSilage)
        {
            return this.CalculateCarbonInputFromRootsForPerennials(previousYearViewItem, currentYearViewItem, farm);
        }

        // Special case: annual crop used as green manure or swathed.
        if (currentYearViewItem.HarvestMethod == HarvestMethods.GreenManure || currentYearViewItem.HarvestMethod == HarvestMethods.Swathing)
        {
            return this.CalculateCarbonInputFromRootsForGreenManureOrSwathing(previousYearViewItem, currentYearViewItem, farm);
        }

        if (Math.Abs(currentYearViewItem.BiomassCoefficientProduct) < double.Epsilon)
        {
            return 0;
        }

        return currentYearViewItem.PlantCarbonInAgriculturalProduct
               * (currentYearViewItem.BiomassCoefficientRoots / currentYearViewItem.BiomassCoefficientProduct)
               * (currentYearViewItem.PercentageOfRootsReturnedToSoil / 100);
    }

    public double CalculateCarbonInputFromRootsForGreenManureOrSwathing(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        Farm farm)
    {
        var combinedBiomassCoefficient = currentYearViewItem.BiomassCoefficientProduct + currentYearViewItem.BiomassCoefficientStraw;
        if (combinedBiomassCoefficient == 0)
        {
            return 0;
        }

        return currentYearViewItem.PlantCarbonInAgriculturalProduct
               * (currentYearViewItem.BiomassCoefficientRoots / combinedBiomassCoefficient)
               * (currentYearViewItem.PercentageOfRootsReturnedToSoil / 100);
    }

    public double CalculateCarbonInputFromExtraroot(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        Farm farm)
    {
        if (currentYearViewItem.CropType.IsFallow() || currentYearViewItem.CropType == CropType.NotSelected)
        {
            return 0;
        }

        if (currentYearViewItem.CropType.IsPerennial() || currentYearViewItem.CropType == CropType.GrassSilage)
        {
            return this.CalculateCarbonInputFromExtrarootsForPerennials(previousYearViewItem, currentYearViewItem, farm);
        }

        if (currentYearViewItem.HarvestMethod == HarvestMethods.GreenManure || currentYearViewItem.HarvestMethod == HarvestMethods.Swathing)
        {
            return this.CalculateCarbonInputFromExtrarootsForGreenManureOrSwathing(previousYearViewItem, currentYearViewItem, farm);
        }

        if (Math.Abs(currentYearViewItem.BiomassCoefficientProduct) < double.Epsilon)
        {
            return 0;
        }

        return currentYearViewItem.PlantCarbonInAgriculturalProduct
               * (currentYearViewItem.BiomassCoefficientExtraroot / currentYearViewItem.BiomassCoefficientProduct);
    }

    public double CalculateCarbonInputFromExtrarootsForPerennials(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        Farm farm)
    {
        if (currentYearViewItem.BiomassCoefficientProduct == 0)
        {
            return 0;
        }

        // Eq. 2.1.2-29, Eq. 2.1.2-31
        return currentYearViewItem.PlantCarbonInAgriculturalProduct
               * (currentYearViewItem.BiomassCoefficientExtraroot / currentYearViewItem.BiomassCoefficientProduct)
               * (currentYearViewItem.PercentageOfExtraRootsReturnedToSoil / 100.0);
    }

    public double CalculateCarbonInputFromExtrarootsForGreenManureOrSwathing(
        CropViewItem previousYearViewItem,
        CropViewItem currentYearViewItem,
        Farm farm)
    {
        var combinedBiomassCoefficient = currentYearViewItem.BiomassCoefficientProduct + currentYearViewItem.BiomassCoefficientStraw;
        if (combinedBiomassCoefficient == 0)
        {
            return 0;
        }

        return currentYearViewItem.PlantCarbonInAgriculturalProduct
               * (currentYearViewItem.BiomassCoefficientExtraroot / combinedBiomassCoefficient);
    }

    public double CalculateTotalAboveGroundCarbonInput(CropViewItem cropViewItem, Farm farm)
    {
        // Green manure and swathing fold straw into the product fraction; no separate straw input.
        if (cropViewItem.HarvestMethod == HarvestMethods.GreenManure || cropViewItem.HarvestMethod == HarvestMethods.Swathing)
        {
            return cropViewItem.CarbonInputFromProduct;
        }

        return cropViewItem.IsSelectedCropTypeRootCrop
            ? cropViewItem.CarbonInputFromStraw
            : cropViewItem.CarbonInputFromProduct + cropViewItem.CarbonInputFromStraw;
    }

    public double CalculateTotalBelowGroundCarbonInput(CropViewItem cropViewItem, Farm farm)
    {
        return cropViewItem.IsSelectedCropTypeRootCrop
            ? cropViewItem.CarbonInputFromProduct + cropViewItem.CarbonInputFromExtraroots
            : cropViewItem.CarbonInputFromRoots + cropViewItem.CarbonInputFromExtraroots;
    }

    #endregion
}
