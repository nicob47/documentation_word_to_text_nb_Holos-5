using System.Collections.ObjectModel;
using System.ComponentModel;
using Prism.Commands;
using H.Avalonia.ViewModels;

namespace H.Avalonia.ViewModels.Wizard
{
    /// <summary>
    /// Generic host view model that drives a linear wizard (Back / Next / Finish).
    /// Derive from this class and populate <see cref="Steps"/> to build a concrete wizard.
    /// </summary>
    public abstract class WizardHostViewModelBase : ViewModelBase
    {
        #region Fields

        private int _currentStepIndex;
        private ObservableCollection<IWizardStepViewModel> _steps = new();
        private DelegateCommand? _goNextCommand;
        private DelegateCommand? _goBackCommand;

        #endregion

        #region Constructors

        /// <summary>Parameterless constructor for design-time support.</summary>
        protected WizardHostViewModelBase() { }

        protected WizardHostViewModelBase(
            Prism.Regions.IRegionManager regionManager,
            H.Core.Services.StorageService.IStorageService storageService)
            : base(regionManager, storageService) { }

        #endregion

        #region Properties

        public ObservableCollection<IWizardStepViewModel> Steps
        {
            get => _steps;
            protected set
            {
                if (SetProperty(ref _steps, value))
                {
                    // Computed properties that depend on Steps must also be notified
                    RaisePropertyChanged(nameof(CurrentStep));
                    RaisePropertyChanged(nameof(IsFirstStep));
                    RaisePropertyChanged(nameof(IsLastStep));
                    RaisePropertyChanged(nameof(NextButtonLabel));
                    RaisePropertyChanged(nameof(ProgressLabel));
                }
            }
        }

        public int CurrentStepIndex
        {
            get => _currentStepIndex;
            set
            {
                if (SetProperty(ref _currentStepIndex, value))
                {
                    RaisePropertyChanged(nameof(CurrentStep));
                    RaisePropertyChanged(nameof(IsFirstStep));
                    RaisePropertyChanged(nameof(IsLastStep));
                    RaisePropertyChanged(nameof(NextButtonLabel));
                    RaisePropertyChanged(nameof(ProgressLabel));
                    GoNextCommand?.RaiseCanExecuteChanged();
                    GoBackCommand?.RaiseCanExecuteChanged();

                    // Subscribe to the new step's CanGoNext changes
                    SubscribeToCurrentStep();
                }
            }
        }

        public IWizardStepViewModel? CurrentStep => Steps.Count > 0 ? Steps[CurrentStepIndex] : null;

        /// <summary>Global "Step N of M" counter shown in the progress badge.</summary>
        public string ProgressLabel => Steps.Count > 0
            ? $"Step {CurrentStepIndex + 1} of {Steps.Count}"
            : string.Empty;

        public bool IsFirstStep => CurrentStepIndex == 0;

        public bool IsLastStep => Steps.Count > 0 && CurrentStepIndex == Steps.Count - 1;

        public string NextButtonLabel => IsLastStep ? "Finish" : "Next";

        public DelegateCommand? GoNextCommand
        {
            get => _goNextCommand;
            protected set => SetProperty(ref _goNextCommand, value);
        }

        public DelegateCommand? GoBackCommand
        {
            get => _goBackCommand;
            protected set => SetProperty(ref _goBackCommand, value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Forces the step index to <paramref name="index"/> and raises all
        /// dependent property-changed notifications, even if the index value did
        /// not change. Use this after replacing the <see cref="Steps"/> collection.
        /// </summary>
        protected void ForceCurrentStepIndex(int index)
        {
            _currentStepIndex = -1;   // guarantee SetProperty detects a change
            CurrentStepIndex = index;
        }

        /// <summary>
        /// Initialises the commands. Call this at the end of the derived constructor
        /// after <see cref="Steps"/> has been populated.
        /// </summary>
        protected void InitializeWizardCommands()
        {
            GoNextCommand = new DelegateCommand(OnGoNext, CanExecuteGoNext);
            GoBackCommand = new DelegateCommand(OnGoBack, () => !IsFirstStep);
            SubscribeToCurrentStep();
        }

        #endregion

        #region Protected / Virtual Methods

        protected virtual void OnGoNext()
        {
            if (IsLastStep)
            {
                OnFinish();
            }
            else
            {
                CurrentStepIndex++;
            }
        }

        protected virtual void OnGoBack()
        {
            if (!IsFirstStep)
                CurrentStepIndex--;
        }

        protected virtual bool CanExecuteGoNext()
        {
            return CurrentStep?.CanGoNext ?? false;
        }

        /// <summary>
        /// Called when the user clicks Finish on the last step.
        /// Override to persist data or navigate away.
        /// </summary>
        protected virtual void OnFinish() { }

        #endregion

        #region Private Methods

        private IWizardStepViewModel? _subscribedStep;

        private void SubscribeToCurrentStep()
        {
            if (_subscribedStep is INotifyPropertyChanged oldNpc)
                oldNpc.PropertyChanged -= OnCurrentStepPropertyChanged;

            _subscribedStep = CurrentStep;

            if (_subscribedStep is INotifyPropertyChanged newNpc)
                newNpc.PropertyChanged += OnCurrentStepPropertyChanged;

            GoNextCommand?.RaiseCanExecuteChanged();
        }

        private void OnCurrentStepPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IWizardStepViewModel.CanGoNext))
                GoNextCommand?.RaiseCanExecuteChanged();
        }

        #endregion
    }
}
