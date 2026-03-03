namespace H.Core.Providers.Soil
{
    /// <summary>
    /// http://sis.agr.gc.ca/cansis/nsdb/slc/v3.2/cmp/index.html
    /// </summary>
    internal class ComponentTableData
    {
        private string _soilNameIdentifier = string.Empty;


        public int PolygonId { get; set; }
        public int ComponentNumber { get; set; }
        public int PercentageOfPolygonOccupiedByComponent { get; set; }
        public string SlopeGradient { get; set; } = string.Empty;
        public string Stone { get; set; } = string.Empty;
        public string LocalSurfaceForm { get; set; } = string.Empty;
        public string ProvinceCode { get; set; } = string.Empty;
        public string SoilCode { get; set; } = string.Empty;
        public string SoilCodeModifier { get; set; } = string.Empty;
        public string TypeOfSoilProfile { get; set; } = string.Empty;

        /// <summary>
        /// Component table uses 'NF' to indicate Newfoundland but soil layer and soil name tables use 'NL'.
        /// </summary>
        public string SoilNameIdentifier
        {
            get
            {
                if (this._soilNameIdentifier.StartsWith("NF"))
                {                    
                    return this._soilNameIdentifier.Replace("NF", "NL");
                }
                
                return _soilNameIdentifier;
            }
            set { _soilNameIdentifier = value; }
        }

        public int PolygonComponentId { get; set; }
    }
}