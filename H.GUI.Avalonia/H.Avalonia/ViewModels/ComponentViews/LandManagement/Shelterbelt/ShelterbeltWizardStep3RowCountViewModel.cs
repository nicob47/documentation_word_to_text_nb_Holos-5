using H.Avalonia.ViewModels.Wizard;
using H.Core.Models.LandManagement.Shelterbelt;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Shelterbelt
{
    /// <summary>
    /// Step 3 — The user specifies how many rows the shelterbelt has (1–10).
    /// </summary>
    public class ShelterbeltWizardStep3RowCountViewModel : WizardStepViewModelBase
    {
        #region Fields

        private readonly ShelterbeltComponent _component;
        private int _numberOfRows;

        #endregion

        #region Constructors

        public ShelterbeltWizardStep3RowCountViewModel(ShelterbeltComponent component)
        {
            _component = component;
            _numberOfRows = component.RowData.Count > 0 ? component.RowData.Count : 1;
            ValidateRowCount();
        }

        #endregion

        #region IWizardStepViewModel

        public override string StepLabel => "Step 3 of 6";
        public override string StepTitle => "Number of Rows";

        #endregion

        #region Properties

        public int NumberOfRows
        {
            get => _numberOfRows;
            set
            {
                if (SetProperty(ref _numberOfRows, value))
                    ValidateRowCount();
            }
        }

        public const int MinRows = 1;
        public const int MaxRows = 10;

        #endregion

        #region Public Methods

        /// <summary>
        /// Reconciles the <see cref="ShelterbeltComponent.RowData"/> collection with the
        /// chosen <see cref="NumberOfRows"/>. Call this when leaving the step.
        /// </summary>
        public void ApplyRowCount()
        {
            while (_component.RowData.Count < _numberOfRows)
                _component.NewRowData();

            while (_component.RowData.Count > _numberOfRows)
                _component.RowData.RemoveAt(_component.RowData.Count - 1);
        }

        #endregion

        #region Private Methods

        private void ValidateRowCount()
        {
            CanGoNext = _numberOfRows >= MinRows && _numberOfRows <= MaxRows;
        }

        #endregion
    }
}
