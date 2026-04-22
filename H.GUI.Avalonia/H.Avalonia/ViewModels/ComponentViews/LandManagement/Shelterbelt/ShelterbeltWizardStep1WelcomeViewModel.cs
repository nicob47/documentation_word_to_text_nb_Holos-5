using H.Avalonia.ViewModels.Wizard;
using H.Core.Models.LandManagement.Shelterbelt;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement.Shelterbelt
{
    /// <summary>
    /// Step 1 — The user gives the shelterbelt a name.
    /// </summary>
    public class ShelterbeltWizardStep1WelcomeViewModel : WizardStepViewModelBase
    {
        #region Fields

        private readonly ShelterbeltComponent _component;
        private string _shelterbeltName = string.Empty;

        #endregion

        #region Constructors

        public ShelterbeltWizardStep1WelcomeViewModel(ShelterbeltComponent component)
        {
            _component = component;
            _shelterbeltName = component.Name ?? string.Empty;
            ValidateName();
        }

        #endregion

        #region IWizardStepViewModel

        public override string StepLabel => "Step 1 of 6";
        public override string StepTitle => "Welcome";

        #endregion

        #region Properties

        /// <summary>The user-entered name for this shelterbelt.</summary>
        public string ShelterbeltName
        {
            get => _shelterbeltName;
            set
            {
                if (SetProperty(ref _shelterbeltName, value))
                {
                    _component.Name = value;
                    ValidateName();
                }
            }
        }

        #endregion

        #region Private Methods

        private void ValidateName()
        {
            CanGoNext = !string.IsNullOrWhiteSpace(_shelterbeltName);
        }

        #endregion
    }
}
