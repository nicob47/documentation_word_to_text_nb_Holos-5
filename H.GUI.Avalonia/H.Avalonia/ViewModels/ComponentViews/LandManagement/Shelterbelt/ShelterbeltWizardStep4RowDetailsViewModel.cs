using H.Avalonia.ViewModels.Wizard;
using H.Core.Models.LandManagement.Shelterbelt;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Shelterbelt
{
    /// <summary>
    /// Step 4 (repeated once per row) — The user names the row and enters its length.
    /// </summary>
    public class ShelterbeltWizardStep4RowDetailsViewModel : WizardStepViewModelBase
    {
        #region Fields

        private readonly RowData _rowData;
        private readonly int _rowIndex;
        private readonly int _totalRows;
        private string _rowName = string.Empty;
        private double _rowLength;

        #endregion

        #region Constructors

        /// <param name="rowData">The row to edit.</param>
        /// <param name="rowIndex">1-based index of this row.</param>
        /// <param name="totalRows">Total number of rows in the shelterbelt.</param>
        public ShelterbeltWizardStep4RowDetailsViewModel(RowData rowData, int rowIndex, int totalRows)
        {
            _rowData = rowData;
            _rowIndex = rowIndex;
            _totalRows = totalRows;

            _rowName = rowData.Name ?? $"Row {rowIndex}";
            _rowLength = rowData.Length > 0 ? rowData.Length : 100;

            // Sync back in case defaults were applied
            _rowData.Name = _rowName;
            _rowData.Length = _rowLength;

            Validate();
        }

        #endregion

        #region IWizardStepViewModel

        public override string StepLabel => $"Row {_rowIndex} of {_totalRows}";

        public override string StepTitle => $"Row Details — Row {_rowIndex} of {_totalRows}";

        #endregion

        #region Properties

        public string RowName
        {
            get => _rowName;
            set
            {
                if (SetProperty(ref _rowName, value))
                {
                    _rowData.Name = value;
                    Validate();
                }
            }
        }

        public double RowLength
        {
            get => _rowLength;
            set
            {
                if (SetProperty(ref _rowLength, value))
                {
                    _rowData.Length = value;
                    Validate();
                }
            }
        }

        #endregion

        #region Private Methods

        private void Validate()
        {
            CanGoNext = !string.IsNullOrWhiteSpace(_rowName) && _rowLength > 0;
        }

        #endregion
    }
}
