using System.Collections.ObjectModel;
using System.Linq;
using H.Core.Models.LandManagement.Shelterbelt;
using H.Infrastructure;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Shelterbelt
{
    /// <summary>
    /// Read-only snapshot of a completed shelterbelt shown in the summary/neutral state
    /// after the wizard finishes. Rebuilt each time <see cref="ShelterbeltComponentViewModel.OnFinish"/> runs.
    /// </summary>
    public class ShelterbeltSummaryViewModel : ModelBase
    {
        public ShelterbeltSummaryViewModel(ShelterbeltComponent component)
        {
            ShelterbeltName    = component.Name ?? string.Empty;
            HardinessZoneDisplay = component.HardinessZone.GetDescription();
            EcoDistrictId      = component.EcoDistrictId;
            RowCount           = component.RowData.Count;

            Rows = new ObservableCollection<ReviewRowSummaryViewModel>(
                component.RowData.Select((row, idx) => new ReviewRowSummaryViewModel(row, idx + 1)));
        }

        /// <summary>User-supplied name for this shelterbelt.</summary>
        public string ShelterbeltName { get; }

        /// <summary>Human-readable hardiness zone (e.g. "H3b").</summary>
        public string HardinessZoneDisplay { get; }

        /// <summary>Eco-district identifier.</summary>
        public int EcoDistrictId { get; }

        /// <summary>Total number of configured rows.</summary>
        public int RowCount { get; }

        /// <summary>Per-row summaries (name, length, tree groups).</summary>
        public ObservableCollection<ReviewRowSummaryViewModel> Rows { get; }
    }
}
