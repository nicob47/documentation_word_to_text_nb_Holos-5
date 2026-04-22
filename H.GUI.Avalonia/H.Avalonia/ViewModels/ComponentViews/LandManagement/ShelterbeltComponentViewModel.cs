using System.Collections.ObjectModel;
using H.Avalonia.ViewModels.ComponentViews.LandManagement.Shelterbelt;
using H.Avalonia.ViewModels.Wizard;
using H.Core.Models;
using H.Core.Models.LandManagement.Shelterbelt;
using H.Core.Services.StorageService;
using Prism.Commands;
using Prism.Regions;

namespace H.Avalonia.ViewModels.ComponentViews.LandManagement
{
    /// <summary>
    /// View model for the Shelterbelt component guided wizard.
    /// Inherits <see cref="WizardHostViewModelBase"/> and builds the flat step list:
    /// Welcome → Location → RowCount → [RowDetails + Species] × N → Review.
    /// </summary>
    public class ShelterbeltComponentViewModel : WizardHostViewModelBase
    {
        #region Fields

        private ShelterbeltComponent? _component;
        private bool _isSummaryMode;
        private ShelterbeltSummaryViewModel? _summary;
        private DelegateCommand? _editCommand;

        #endregion

        #region Constructors

        /// <summary>Parameterless constructor for Avalonia design-time support.</summary>
        public ShelterbeltComponentViewModel() { }

        public ShelterbeltComponentViewModel(
            IRegionManager regionManager,
            IStorageService storageService)
            : base(regionManager, storageService) { }

        #endregion

        #region Properties

        /// <summary>
        /// True when the wizard is complete and the neutral summary panel is shown.
        /// False (default) while the step-by-step wizard is active.
        /// </summary>
        public bool IsSummaryMode
        {
            get => _isSummaryMode;
            private set => SetProperty(ref _isSummaryMode, value);
        }

        /// <summary>
        /// Snapshot of the completed shelterbelt data, populated by <see cref="OnFinish"/>.
        /// Null while the wizard is still in progress.
        /// </summary>
        public ShelterbeltSummaryViewModel? Summary
        {
            get => _summary;
            private set => SetProperty(ref _summary, value);
        }

        /// <summary>
        /// Returns to the wizard at Step 1 so the user can make changes.
        /// Only relevant (and visible) when <see cref="IsSummaryMode"/> is true.
        /// </summary>
        public DelegateCommand? EditCommand
        {
            get => _editCommand;
            private set => SetProperty(ref _editCommand, value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Called by Prism when the user selects this component from the component list.
        /// The <see cref="ShelterbeltComponent"/> is passed as a navigation parameter.
        /// </summary>
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters.ContainsKey(GuiConstants.ComponentKey))
            {
                var parameter = navigationContext.Parameters[GuiConstants.ComponentKey];
                if (parameter is ShelterbeltComponent shelterbeltComponent)
                {
                    InitializeViewModel(shelterbeltComponent);
                }
            }
        }

        public override void InitializeViewModel(ComponentBase component)
        {
            if (component is ShelterbeltComponent shelterbeltComponent)
            {
                _component = shelterbeltComponent;
                EditCommand ??= new DelegateCommand(OnEdit);
                IsSummaryMode = false;
                BuildStepList();
            }
        }

        #endregion

        #region Protected Overrides

        protected override void OnFinish()
        {
            // Materialise TrannumData from the populated RowData collection
            _component?.BuildTrannums();

            // Build a fresh summary snapshot and switch to the neutral summary view
            if (_component != null)
                Summary = new ShelterbeltSummaryViewModel(_component);

            IsSummaryMode = true;
        }

        /// <summary>
        /// When leaving Step 3 (row count), reconcile the model's RowData collection
        /// before expanding the step list to include per-row steps.
        /// When leaving the last per-row step (the step immediately before Review),
        /// rebuild the Review VM so its tree-group snapshots reflect the current state.
        /// </summary>
        protected override void OnGoNext()
        {
            if (CurrentStep is ShelterbeltWizardStep3RowCountViewModel step3)
            {
                step3.ApplyRowCount();
                BuildStepList(advancePastStep3: true);
                return;
            }

            // One step before Review: replace the stale snapshot with a fresh one.
            // CurrentStepIndex + 1 == Steps.Count - 1 means the next step is Review.
            if (_component != null && CurrentStepIndex == Steps.Count - 2)
            {
                Steps[Steps.Count - 1] =
                    new ShelterbeltWizardStep6ReviewViewModel(_component, JumpToStep);
            }

            base.OnGoNext();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Switches back to wizard mode and resets to Step 1 (Welcome),
        /// rebuilding the step list so all step VMs reflect the current component data.
        /// </summary>
        private void OnEdit()
        {
            IsSummaryMode = false;
            BuildStepList();          // Rebuilds all step VMs from current component state
            ForceCurrentStepIndex(0); // Override BuildStepList's position restore → go to Welcome
        }

        private void BuildStepList(bool advancePastStep3 = false)
        {
            if (_component == null) return;

            var currentIndex = CurrentStepIndex;

            var steps = new ObservableCollection<IWizardStepViewModel>();

            // Static steps 1–3
            steps.Add(new ShelterbeltWizardStep1WelcomeViewModel(_component));
            steps.Add(new ShelterbeltWizardStep2LocationViewModel(_component));
            steps.Add(new ShelterbeltWizardStep3RowCountViewModel(_component));  // index 2

            // Per-row steps (steps 4 and 5 repeat for each row)
            var totalRows = _component.RowData.Count;
            for (int i = 0; i < totalRows; i++)
            {
                var row = _component.RowData[i];
                steps.Add(new ShelterbeltWizardStep4RowDetailsViewModel(row, i + 1, totalRows));
                steps.Add(new ShelterbeltWizardStep5SpeciesViewModel(row, i + 1, totalRows));
            }

            // Final review step — passes the JumpToStep callback so the review
            // can send the host back to any step for editing
            steps.Add(new ShelterbeltWizardStep6ReviewViewModel(_component, JumpToStep));

            Steps = steps;
            InitializeWizardCommands();

            if (advancePastStep3)
            {
                // Move to first RowDetails step (index 3 in the flat list)
                ForceCurrentStepIndex(totalRows > 0 ? 3 : steps.Count - 1);
            }
            else
            {
                // Restore prior position, clamped to valid range; force notification
                // even if the numeric index value didn't change
                ForceCurrentStepIndex(currentIndex < steps.Count ? currentIndex : 0);
            }
        }

        private void JumpToStep(int stepIndex)
        {
            if (stepIndex >= 0 && stepIndex < Steps.Count)
                CurrentStepIndex = stepIndex;
        }

        #endregion
    }
}
