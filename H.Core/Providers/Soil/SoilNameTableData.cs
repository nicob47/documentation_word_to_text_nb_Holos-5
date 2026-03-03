#region Imports

#endregion

using H.Core.Enumerations;

namespace H.Core.Providers.Soil
{
    /// <summary>
    /// http://sis.agr.gc.ca/cansis/nsdb/soil/v2/snt/index.html
    /// </summary>
    internal class SoilNameTableData
    {
        public string SoilNameIdentifier { get; set; } = string.Empty;
        public string ProvinceCode { get; set; } = string.Empty;
        public string SoilCode { get; set; } = string.Empty;
        public string SoilCodeModifier { get; set; } = string.Empty;
        public string TypeOfSoilProfile { get; set; } = string.Empty;
        public string SoilName { get; set; } = string.Empty;
        public string KindOfSurfaceMaterial { get; set; } = string.Empty;
        public string WaterTableCharacteristics { get; set; } = string.Empty;
        public string SoilLayerThatRestrictsRootsGrowth { get; set; } = string.Empty;
        public string TypeOfRootRestrictingLayer { get; set; } = string.Empty;
        public string SoilDrainageClass { get; set; } = string.Empty;
        public string FirstParentMaterialTexture { get; set; } = string.Empty;
        public string SecondParentMaterialTexture { get; set; } = string.Empty;
        public string ThirdParentMaterialTexture { get; set; } = string.Empty;
        public string FirstParentMaterialChemicalProperty { get; set; } = string.Empty;
        public string SecondParentMaterialChemicalProperty { get; set; } = string.Empty;
        public string ThirdParentMaterialChemicalProperty { get; set; } = string.Empty;
        public string FirstModeOfDeposition { get; set; } = string.Empty;
        public string SecondModeOfDeposition { get; set; } = string.Empty;
        public string ThirdModeOfDeposition { get; set; } = string.Empty;
        public string SoilOrderSecondEdition { get; set; } = string.Empty;
        public string SoilGreatGroupSecondEdition { get; set; } = string.Empty;
        public string SoilSubgroupSecondEdition { get; set; } = string.Empty;
        public string SoilOrderThirdEdition { get; set; } = string.Empty;
        public string SoilGreatGroupThirdEdition { get; set; } = string.Empty;
        public string SoilSubgroupThirdEdition { get; set; } = string.Empty;
        public Province Province { get; set; }
    }
}
