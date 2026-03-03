using H.Core.Enumerations;

namespace H.Core.Providers.Shelterbelt
{
    public class ShelterbeltDomProviderData
    {
        /// <summary>
        /// (Mg C km^-1 year^-1)
        /// </summary>
        public double DeadOrganicMatterCarbonPerKilometerPerYear { get; set; }

        /// <summary>
        /// (Mg C km^-1)
        /// </summary>
        public double DeadOrganicMatterCarbonPerKilometer { get; set; }

        /// <summary>
        /// (Mg C km^-1 year^-1)
        /// </summary>
        public double BiomassCarbonPerKilometerPerYear { get; set; }

        /// <summary>
        /// (Mg C km^-1)
        /// </summary>
        public double BiomassCarbonPerKilometer { get; set; }

        /// <summary>
        /// (Mg C km^-1 year^-1)
        /// </summary>
        public double TecPerKilometerPerYear { get; set; }

        /// <summary>
        /// (Mg C km^-1)
        /// </summary>
        public double TecPerKilometer { get; set; }

        public TreeSpecies Species { get; set; }
        public string ClusterId { get; set; } = string.Empty;
        public double PercentageMortality { get; set; }
        public int Age { get; set; }
    }
}