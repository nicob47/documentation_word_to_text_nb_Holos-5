using System.Collections.ObjectModel;
using System.Linq;
using H.Avalonia.ViewModels.Wizard;
using H.Core.Models.LandManagement.Shelterbelt;
using H.Infrastructure;
using Prism.Commands;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Shelterbelt
{
    /// <summary>
    /// Step 6 — Read-only summary of everything the user entered.
    /// "Finish" is always enabled on this step.
    /// </summary>
    public class ShelterbeltWizardStep6ReviewViewModel : WizardStepViewModelBase
    {
        #region Constructors

        public ShelterbeltWizardStep6ReviewViewModel(
            ShelterbeltComponent component,
            System.Action<int> jumpToStep)
        {
            ShelterbeltName = component.Name ?? string.Empty;
            HardinessZoneDisplay = component.HardinessZone.GetDescription();
            EcoDistrictId = component.EcoDistrictId;

            Rows = new ObservableCollection<ReviewRowSummaryViewModel>(
                component.RowData
                    .Select((row, idx) => new ReviewRowSummaryViewModel(row, idx + 1)));

            JumpToStepCommand = new DelegateCommand<string?>(stepStr =>
            {
                if (int.TryParse(stepStr, out var step))
                    jumpToStep(step);
            });

            CanGoNext = true;
        }

        #endregion

        #region IWizardStepViewModel

        public override string StepLabel => "Step 6 of 6";
        public override string StepTitle => "Review & Confirm";

        #endregion

        #region Properties

        public string ShelterbeltName { get; }
        public string HardinessZoneDisplay { get; }
        public int EcoDistrictId { get; }
        public ObservableCollection<ReviewRowSummaryViewModel> Rows { get; }

        /// <summary>
        /// Navigates back to an earlier step for editing.
        /// CommandParameter is the 0-based step index passed as a string (e.g. "0").
        /// </summary>
        public DelegateCommand<string?> JumpToStepCommand { get; }

        #endregion
    }
}
