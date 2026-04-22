using H.Infrastructure;

namespace H.Avalonia.ViewModels.Wizard
{
    /// <summary>
    /// Base class for wizard step view models.
    /// Provides default INPC plumbing and a <see cref="CanGoNext"/> helper.
    /// </summary>
    public abstract class WizardStepViewModelBase : ModelBase, IWizardStepViewModel
    {
        private bool _canGoNext = true;

        /// <inheritdoc/>
        public abstract string StepLabel { get; }

        /// <inheritdoc/>
        public abstract string StepTitle { get; }

        /// <inheritdoc/>
        public bool CanGoNext
        {
            get => _canGoNext;
            protected set => SetProperty(ref _canGoNext, value);
        }

        /// <summary>
        /// Called by derived classes when validation state changes so the host
        /// can update the Next / Finish button enabled state.
        /// </summary>
        protected void RaiseCanGoNextChanged() => RaisePropertyChanged(nameof(CanGoNext));
    }
}
