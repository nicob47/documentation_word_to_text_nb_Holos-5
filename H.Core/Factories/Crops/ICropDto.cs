using System.Collections.Generic;
using System.Collections.ObjectModel;
using H.Core.Enumerations;
using H.Core.Models.LandManagement.Fields;
using System.Windows.Input;

namespace H.Core.Factories.Crops;

/// <summary>
/// A data transfer object used to validate and collect information about a <see cref="CropViewItem"/>
/// </summary>
public interface ICropDto : IDto
{
    // ── Identity / Selection ──

    CropType CropType { get; set; }
    ObservableCollection<CropType> ValidCropTypes { get; set; }
    IReadOnlyList<object> GroupedCropItems { get; }
    object? SelectedCropTypeItem { get; set; }
    int Year { get; set; }
    bool IsSelected { get; set; }
    bool CopyToSimilarCrops { get; set; }

    // ── General ──

    double WetYield { get; set; }
    double DryYield { get; set; }
    double MoistureContentOfCropPercentage { get; set; }
    double AmountOfIrrigation { get; set; }
    bool HerbicideUsed { get; set; }
    int NumberOfPesticidePasses { get; set; }
    TillageType TillageType { get; set; }
    HarvestMethods HarvestMethod { get; set; }

    // ── Fertilizer ──

    double NitrogenFertilizerRate { get; set; }
    double PhosphorusFertilizerRate { get; set; }
    double PotassiumFertilizerRate { get; set; }
    double SulphurFertilizerRate { get; set; }
    NitrogenFertilizerType NitrogenFertilizerType { get; set; }
    FertilizerBlends FertilizerBlend { get; set; }
    SoilReductionFactors SoilReductionFactor { get; set; }
    double CustomReductionFactor { get; set; }

    // ── Manure ──

    bool ManureApplied { get; set; }
    double AmountOfManureApplied { get; set; }
    ManureLocationSourceType ManureLocationSourceType { get; set; }
    ManureAnimalSourceTypes ManureAnimalSourceType { get; set; }
    ManureApplicationTypes ManureApplicationType { get; set; }
    ManureStateType ManureStateType { get; set; }

    // ── Grazing ──

    bool CropIsGrazed { get; set; }
    double ForageUtilizationRate { get; set; }

    // ── Soil ──

    double ClimateParameter { get; set; }
    double TillageFactor { get; set; }
    double ManagementFactor { get; set; }

    // ── Residue ──

    double BiomassCoefficientProduct { get; set; }
    double BiomassCoefficientStraw { get; set; }
    double BiomassCoefficientRoots { get; set; }
    double BiomassCoefficientExtraroot { get; set; }
    double PercentageOfProductYieldReturnedToSoil { get; set; }
    double PercentageOfStrawReturnedToSoil { get; set; }
    double PercentageOfRootsReturnedToSoil { get; set; }
    double NitrogenContentInProduct { get; set; }
    double NitrogenContentInStraw { get; set; }
    double NitrogenContentInRoots { get; set; }
    double NitrogenContentInExtraroot { get; set; }
    double CarbonConcentration { get; set; }
    double LigninContent { get; set; }

    // ── Economics ──

    bool CropEconomicDataApplied { get; set; }

    // ── Sequence ──

    /// <summary>
    /// 1-based position in the rotation/field history sequence. Updated by the VM after add/remove/move.
    /// </summary>
    int SequenceNumber { get; set; }

    // ── Cover Crop ──

    bool IsSecondaryCrop { get; set; }
    CoverCropTerminationType CoverCropTerminationType { get; set; }
    ICropDto? CoverCropDto { get; set; }
    bool HasCoverCrop { get; }
}
