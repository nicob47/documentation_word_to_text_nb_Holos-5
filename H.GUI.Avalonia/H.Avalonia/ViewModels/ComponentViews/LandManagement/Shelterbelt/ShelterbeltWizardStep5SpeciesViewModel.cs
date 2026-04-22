using System;
using System.Collections.ObjectModel;
using System.Linq;
using H.Avalonia.ViewModels.Wizard;
using H.Core.Enumerations;
using H.Core.Models.LandManagement.Shelterbelt;
using Prism.Commands;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Shelterbelt
{
    /// <summary>
    /// Step 5 (repeated once per row) — The user picks species and fills in tree group detail cards.
    /// </summary>
    public class ShelterbeltWizardStep5SpeciesViewModel : WizardStepViewModelBase
    {
        #region Fields

        private readonly RowData _rowData;
        private readonly int _rowIndex;
        private readonly int _totalRows;

        #endregion

        #region Constructors

        /// <param name="rowData">The row whose tree groups are being configured.</param>
        /// <param name="rowIndex">1-based index of this row.</param>
        /// <param name="totalRows">Total number of rows in the shelterbelt.</param>
        public ShelterbeltWizardStep5SpeciesViewModel(RowData rowData, int rowIndex, int totalRows)
        {
            _rowData = rowData;
            _rowIndex = rowIndex;
            _totalRows = totalRows;

            // Build species selection chips from all TreeSpecies values
            AvailableSpecies = new ObservableCollection<SpeciesSelectionViewModel>(
                Enum.GetValues<TreeSpecies>()
                    .Select(s => new SpeciesSelectionViewModel(
                        s,
                        rowData.TreeGroupData.Any(tg => tg.TreeSpecies == s))));

            // Build cards for already-existing tree groups
            TreeGroupCards = new ObservableCollection<TreeGroupCardViewModel>(
                rowData.TreeGroupData.Select(tg => new TreeGroupCardViewModel(tg)));

            ToggleSpeciesCommand = new DelegateCommand<SpeciesSelectionViewModel?>(OnToggleSpecies);

            ValidateStep();
        }

        #endregion

        #region IWizardStepViewModel

        public override string StepLabel => $"Row {_rowIndex} of {_totalRows}";
        public override string StepTitle => $"Species & Tree Groups — Row {_rowIndex} of {_totalRows}";

        #endregion

        #region Properties

        /// <summary>Species chips displayed in the picker area.</summary>
        public ObservableCollection<SpeciesSelectionViewModel> AvailableSpecies { get; }

        /// <summary>Expanded detail cards — one per selected species.</summary>
        public ObservableCollection<TreeGroupCardViewModel> TreeGroupCards { get; }

        /// <summary>Toggles a species on/off and adds/removes the matching tree group card.</summary>
        public DelegateCommand<SpeciesSelectionViewModel?> ToggleSpeciesCommand { get; }

        #endregion

        #region Private Methods

        private void OnToggleSpecies(SpeciesSelectionViewModel? chip)
        {
            if (chip == null) return;

            if (chip.IsSelected)
            {
                // Remove the tree group for this species
                var existing = _rowData.TreeGroupData.FirstOrDefault(tg => tg.TreeSpecies == chip.Species);
                if (existing != null)
                {
                    _rowData.RemoveTreeGroupData(existing);
                    var card = TreeGroupCards.FirstOrDefault(c => c.Species == chip.Species);
                    if (card != null) TreeGroupCards.Remove(card);
                }
                chip.IsSelected = false;
            }
            else
            {
                // Add a new tree group for this species
                var newGroup = _rowData.NewTreeGroupData();
                newGroup.TreeSpecies = chip.Species;
                TreeGroupCards.Add(new TreeGroupCardViewModel(newGroup));
                chip.IsSelected = true;
            }

            ValidateStep();
        }

        private void ValidateStep()
        {
            CanGoNext = TreeGroupCards.Count > 0;
        }

        #endregion
    }
}
