namespace H.Core.Enumerations
{
    /// <summary>
    /// Whitelist of the only <see cref="Province"/> values that Holos v5 can run the
    /// Canadian carbon / N2O analysis pipeline against. The <see cref="Province"/> enum
    /// itself still carries the 26 Irish counties from the v4 multi-country era; if any
    /// of those reach the analysis, every Canadian-keyed data provider (Table_4
    /// irrigation, Table_63 indoor temperature, EcodistrictDefaults, SLC climate normals,
    /// ...) returns null/0 for the lookup and the downstream ICBM steady-state math
    /// divides by a zero climate parameter, producing NaN/Infinity that the chart
    /// silently drops.
    /// </summary>
    public static class CanadianProvinces
    {
        private static readonly HashSet<Province> _canadianSet = new()
        {
            Province.Alberta,
            Province.BritishColumbia,
            Province.Manitoba,
            Province.NewBrunswick,
            Province.Newfoundland,
            Province.NorthwestTerritories,
            Province.NovaScotia,
            Province.Ontario,
            Province.Nunavut,
            Province.PrinceEdwardIsland,
            Province.Quebec,
            Province.Saskatchewan,
            Province.Yukon,
        };

        /// <summary>
        /// True when the province is one of the 13 Canadian provinces / territories that
        /// the Canadian data tables have entries for. Returns false for Ireland counties,
        /// <see cref="Province.SelectProvince"/>, and any future non-Canadian additions.
        /// </summary>
        public static bool IsCanadian(Province province) => _canadianSet.Contains(province);

        /// <summary>
        /// The set of supported Canadian provinces / territories.
        /// </summary>
        public static IReadOnlyCollection<Province> All => _canadianSet;
    }
}
