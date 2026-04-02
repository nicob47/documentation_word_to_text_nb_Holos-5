using CsvHelper.Configuration;
using H.Core.Models.Climate;

namespace H.Avalonia.Models.ClassMaps
{
    /// <summary>
    /// A mapping class for CsvReader extension. The class maps properties in <see cref="DailyClimateDto"/> class
    /// so that CsvReader is able to identify these properties and handle them when exporting/importing data.
    /// </summary>
    public sealed class ClimateResultsViewItemMap : ClassMap<ClimateViewItem>
    {
        public ClimateResultsViewItemMap()
        {
            Map(map => map.Latitude).Index(0).Name(H.Core.Properties.Resources.LabelLatitude);
            Map(map => map.Longitude).Index(1).Name(H.Core.Properties.Resources.LabelLongitude);
            Map(map => map.Year).Index(2).Name(H.Core.Properties.Resources.LabelYear);
            Map(map => map.TotalPET).Index(3).Name(H.Core.Properties.Resources.LabelTotalPET);
            Map(map => map.TotalPPT).Index(4).Name(H.Core.Properties.Resources.LabelTotalPPT);
            Map(map => map.MonthlyPPT).Index(5).Name(H.Core.Properties.Resources.LabelMonthlyPPT);
        }
    }
}
