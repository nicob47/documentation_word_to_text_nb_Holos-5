using H.Core.Emissions.Results;
using H.Core.Enumerations;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Providers.Plants;

namespace H.Core.Calculators.Carbon;

/// <summary>
/// Tier 2 input-calculation half: converts a crop's yield + crop-specific Table 9 coefficients
/// (slope, intercept, R:S ratio, lignin / N content) into per-hectare above-ground and
/// below-ground carbon inputs that the Tier 2 pool dynamics in
/// <see cref="IPCCTier2SoilCarbonCalculator"/> consume.
///
/// <para><b>Crop coverage:</b></para>
/// Table 9 only has slope / intercept values for a subset of Canadian crop types. For anything
/// else (e.g. obscure cover crops, exotic perennials) we fall back to the ICBM approach. Use
/// <see cref="CanCalculateInputsForCrop"/> to check before calling this calculator; if it returns
/// <c>false</c>, the dispatch in <c>FieldResultsService.AssignCarbonInputs</c> routes to
/// <see cref="ICBMSoilCarbonCalculator.SetCarbonInputs"/> instead.
///
/// <para><b>Constants:</b></para>
/// <c>AboveGroundCarbonContent</c> = <c>BelowGroundCarbonContent</c> = 0.42 — the IPCC default
/// fraction of dry-matter residue that is carbon (kg C / kg DM).
/// </summary>
public class IPCCTier2CarbonInputCalculator : CarbonInputCalculatorBase, IIPCCTier2CarbonInputCalculator
{
    /// <summary>IPCC default — fraction of above-ground dry-matter residue that is carbon (kg C / kg DM).</summary>
    private const double AboveGroundCarbonContent = 0.42;

    /// <summary>IPCC default — fraction of below-ground (root + extra-root) dry-matter residue that is carbon (kg C / kg DM).</summary>
    private const double BelowGroundCarbonContent = 0.42;

    #region Fields

    private readonly Table_9_Nitrogen_Lignin_Content_In_Crops_Provider _slopeProvider;

    #endregion

    #region Constructors

    public IPCCTier2CarbonInputCalculator()
    {
        _slopeProvider = new Table_9_Nitrogen_Lignin_Content_In_Crops_Provider();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// The IPCC Tier 2 approach can only estimate carbon inputs for a small set of crop types.
    /// If a crop type does not have values for intercept, slope, etc. from Table 9, we fall
    /// back to the ICBM approach.
    /// </summary>
    public bool CanCalculateInputsForCrop(CropViewItem cropViewItem)
    {
        return _slopeProvider.GetDataByCropType(cropViewItem.CropType).SlopeValue > 0;
    }

    /// <summary>
    /// Populates the per-crop, per-year Tier 2 carbon-input fields on <paramref name="viewItem"/>.
    /// Mirrors <see cref="ICBMSoilCarbonCalculator.SetCarbonInputs"/> in shape — the downstream
    /// DTO mapper reads the same <c>AboveGroundCarbonInput</c> / <c>BelowGroundCarbonInput</c>
    /// fields regardless of which calculator produced them.
    ///
    /// <para><b>The math:</b></para>
    /// <list type="number">
    ///   <item>Look up the crop's Table 9 row (slope, intercept, R:S ratio).</item>
    ///   <item>Compute the harvest index from yield + intercept + slope + moisture.</item>
    ///   <item>Compute above-ground residue dry matter, subtract burned / removed fractions.</item>
    ///   <item>Multiply by 0.42 (C content) and divide by area to get <c>AboveGroundCarbonInput</c> (kg C ha⁻¹).</item>
    ///   <item>Compute below-ground residue dry matter using R:S ratio + fractionRenewed (1 for annuals, 1/standLength for perennials).</item>
    ///   <item>Add supplemental hay fed to grazing animals + manure / digestate from animal components.</item>
    /// </list>
    /// </summary>
    /// <param name="viewItem">The CropViewItem to populate. Mutated in place.</param>
    /// <param name="farm">Farm context for grazing / animal lookups.</param>
    /// <param name="animalResults">Pre-computed per-animal-component emission results; primes manure / digestate services for this calculation.</param>
    public void AssignInputs(CropViewItem viewItem, Farm farm, List<AnimalComponentEmissionsResults> animalResults)
    {
        manureService.Initialize(farm, animalResults);
        digestateService.Initialize(farm, animalResults);

        var cropData = _slopeProvider.GetDataByCropType(viewItem.CropType);

        // Yield is converted to tons inside CalculateHarvestIndex (slope expects tons).
        var harvestIndex = this.CalculateHarvestIndex(
            slope: cropData.SlopeValue,
            freshWeightOfYield: viewItem.Yield,
            intercept: cropData.InterceptValue,
            moistureContentAsPercentage: viewItem.MoistureContentOfCropPercentage);

        if (viewItem.HarvestMethod == HarvestMethods.Swathing && farm.CropHasGrazingAnimals(viewItem))
        {
            viewItem.PercentageOfProductYieldReturnedToSoil = 100 - viewItem.GetAverageUtilizationFromGrazingAnimals();
        }

        viewItem.AboveGroundResidueDryMatter = this.CalculateAboveGroundResidueDryMatter(harvestIndex, viewItem);
        viewItem.AboveGroundResidueDryMatterExported = this.CalculateAboveGroundResidueDryMatterExported(harvestIndex, viewItem);

        var fractionRenewed = viewItem.CropType.IsAnnual() ? 1 : 1d / viewItem.PerennialStandLength;

        var finalAboveGroundResidue = this.CalculateAnnualAboveGroundResidue(
            aboveGroundResidueDryMatterForCrop: viewItem.AboveGroundResidueDryMatter,
            area: viewItem.Area,
            fractionBurned: 0,
            fractionRemoved: 0,
            combustionFactor: 0);

        // Eq. 2.2.3-3 produces residue for the entire field; we report per ha.
        viewItem.AboveGroundCarbonInput = (finalAboveGroundResidue * AboveGroundCarbonContent) / viewItem.Area;

        viewItem.AboveGroundCarbonInput += base.CalculateInputsFromSupplementalHayFedToGrazingAnimals(
            previousYearViewItem: null!,
            currentYearViewItem: viewItem,
            nextYearViewItems: null!,
            farm: farm);

        viewItem.BelowGroundResidueDryMatter = this.CalculateBelowGroundResidueDryMatter(
            shootToRootRatio: cropData.RSTRatio,
            fractionRenewed: fractionRenewed,
            harvestIndex: harvestIndex,
            cropViewItem: viewItem);

        // Eq. 2.2.3-4 produces residue for the entire field; we report per ha.
        viewItem.BelowGroundCarbonInput = (viewItem.BelowGroundResidueDryMatter * BelowGroundCarbonContent) / viewItem.Area;

        this.AssignManureCarbonInputs(viewItem, farm, animalResults);

        viewItem.DigestateCarbonInputsPerHectare = digestateService.GetTotalDigestateCarbonInputsForField(farm, viewItem.Year, viewItem);
        viewItem.DigestateCarbonInputsPerHectareFromApplicationsOnly =
            viewItem.GetTotalCarbonFromAppliedDigestate(ManureLocationSourceType.Livestock) / viewItem.Area;

        // Eq. 2.2.2-12: pool calculations use kg C (algorithm doc converts to tons before pool
        // calculations; we keep kg C so charts share a scale with ICBM output).
        viewItem.TotalCarbonInputs =
            viewItem.AboveGroundCarbonInput +
            viewItem.BelowGroundCarbonInput +
            viewItem.ManureCarbonInputsPerHectare +
            viewItem.DigestateCarbonInputsPerHectare;
    }

    /// <summary>
    /// Equation 2.2.2-1
    /// </summary>
    /// <param name="slope">(unitless)</param>
    /// <param name="freshWeightOfYield">The yield of the harvest (wet/fresh weight) (kg ha^-1)</param>
    /// <param name="intercept">(unitless)</param>
    /// <param name="moistureContentAsPercentage">The moisture content of the yield (%)</param>
    /// <returns>The harvest index</returns>
    public double CalculateHarvestIndex(
        double slope,
        double freshWeightOfYield,
        double intercept,
        double moistureContentAsPercentage)
    {
        return slope * ((freshWeightOfYield / 1000) * (1 - (moistureContentAsPercentage / 100.0))) + intercept;
    }

    /// <summary>
    /// Equation 2.2.2-2
    /// </summary>
    /// <returns>Above ground residue dry matter for crop (kg ha^-1)</returns>
    public double CalculateAboveGroundResidueDryMatter(double harvestIndex, CropViewItem viewItem)
    {
        if (harvestIndex <= 0)
        {
            return 0;
        }

        var moistureContentAsPercentage = viewItem.HasGrazingViewItems
            ? viewItem.GrazingViewItems.Average(x => x.MoistureContentAsPercentage)
            : viewItem.MoistureContentOfCropPercentage;

        var moistureFractionDifference = 1 - (moistureContentAsPercentage / 100.0);
        var dryMatterPerHectare = viewItem.Yield * moistureFractionDifference;

        var strawReturnedFraction = viewItem.PercentageOfStrawReturnedToSoil / 100.0;
        var productReturnedFraction = viewItem.PercentageOfProductYieldReturnedToSoil / 100.0;

        var leftResult = ((dryMatterPerHectare / harvestIndex) - dryMatterPerHectare) * strawReturnedFraction;
        var rightResult = dryMatterPerHectare * productReturnedFraction;

        return leftResult + rightResult;
    }

    /// <summary>
    /// Equation 2.2.2-4
    /// </summary>
    /// <returns>Annual total amount of above-ground residue (kg year^-1)</returns>
    public double CalculateAnnualAboveGroundResidue(
        double aboveGroundResidueDryMatterForCrop,
        double area,
        double fractionBurned,
        double fractionRemoved,
        double combustionFactor)
    {
        // Burned residues are not currently considered.
        return aboveGroundResidueDryMatterForCrop * area * (1 - fractionRemoved - (fractionBurned * combustionFactor));
    }

    /// <summary>
    /// Equation 2.2.2-3
    /// </summary>
    /// <returns>Above ground residue dry matter exported (kg ha^-1)</returns>
    public double CalculateAboveGroundResidueDryMatterExported(double harvestRatio, CropViewItem cropViewItem)
    {
        if (harvestRatio <= 0)
        {
            return 0;
        }

        var moistureContentFraction = cropViewItem.MoistureContentOfCropPercentage / 100.0;
        var strawFraction = cropViewItem.PercentageOfStrawReturnedToSoil / 100.0;
        var dryMatterPerHectare = cropViewItem.Yield * (1 - moistureContentFraction);

        var productFraction = cropViewItem.HasGrazingViewItems
            ? 1.0 - (cropViewItem.GrazingViewItems.Average(x => x.Utilization) / 100.0)
            : cropViewItem.PercentageOfProductYieldReturnedToSoil / 100.0;

        var notReturnedAsStraw = (dryMatterPerHectare / harvestRatio) - dryMatterPerHectare;

        return notReturnedAsStraw * (1 - strawFraction) + dryMatterPerHectare * (1 - productFraction);
    }

    /// <summary>
    /// Equation 2.2.2-5
    /// Equation 2.2.2-6
    /// </summary>
    /// <param name="shootToRootRatio">Ratio of below-ground root biomass to above-ground shoot biomass (RS(T))</param>
    /// <param name="fractionRenewed">(unitless)</param>
    /// <param name="harvestIndex">Harvest ratio/index (R_AG(T))</param>
    /// <returns>Annual total amount of below-ground residue (kg year^-1)</returns>
    public double CalculateBelowGroundResidueDryMatter(
        double shootToRootRatio,
        double fractionRenewed,
        double harvestIndex,
        CropViewItem cropViewItem)
    {
        var moistureContentDifference = 1 - (cropViewItem.MoistureContentOfCropPercentage / 100.0);
        var dryMatterPerHectare = cropViewItem.Yield * moistureContentDifference;

        if (harvestIndex <= 0)
        {
            harvestIndex = 1;
        }

        // Cash crop divides the dry matter by harvest index; swathing/silage/green-manure does not.
        var residue = cropViewItem.HarvestMethod == HarvestMethods.CashCrop
            ? dryMatterPerHectare / harvestIndex
            : dryMatterPerHectare;

        return residue * shootToRootRatio * cropViewItem.Area * fractionRenewed;
    }

    #endregion
}
