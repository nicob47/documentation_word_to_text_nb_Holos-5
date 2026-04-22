namespace H.Avalonia.ViewModels.Wizard
{
    /// <summary>
    /// Represents a single step inside a guided wizard.
    /// </summary>
    public interface IWizardStepViewModel
    {
        /// <summary>The short heading displayed in the step badge (e.g. "Step 2 of 6").</summary>
        string StepLabel { get; }

        /// <summary>The descriptive title shown beside the badge (e.g. "Location").</summary>
        string StepTitle { get; }

        /// <summary>
        /// When <c>false</c> the host's Next / Finish button should be disabled.
        /// </summary>
        bool CanGoNext { get; }
    }
}
