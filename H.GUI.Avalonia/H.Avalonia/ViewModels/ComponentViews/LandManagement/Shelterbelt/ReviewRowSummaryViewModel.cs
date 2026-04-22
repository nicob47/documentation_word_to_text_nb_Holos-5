using System.Collections.ObjectModel;
using System.Linq;
using H.Core.Models.LandManagement.Shelterbelt;
using H.Infrastructure;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Shelterbelt
{
    /// <summary>
    /// Read-only row summary displayed on Step 6.
    /// </summary>
    public class ReviewRowSummaryViewModel : ModelBase
    {
        public ReviewRowSummaryViewModel(RowData rowData, int rowIndex)
        {
            RowIndex = rowIndex;
            RowName = rowData.Name ?? $"Row {rowIndex}";
            RowLength = rowData.Length;
            TreeGroups = new ObservableCollection<string>(
                rowData.TreeGroupData.Select(tg =>
                    $"{tg.TreeSpecies.GetDescription()} — planted: {tg.PlantedTreeCount:N0}, alive: {tg.LiveTreeCount:N0}, spacing: {tg.PlantedTreeSpacing:N1} m, year: {tg.PlantYear}"));
        }

        public int RowIndex { get; }
        public string RowName { get; }

        /// <summary>Pre-formatted header string, e.g. "Row 2 — North side".</summary>
        public string RowHeader => $"Row {RowIndex} — {RowName}";

        public double RowLength { get; }
        public ObservableCollection<string> TreeGroups { get; }
    }
}
