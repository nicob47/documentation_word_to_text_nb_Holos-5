using System;
using System.ComponentModel;
using H.Avalonia.Events;
using H.Avalonia.ViewModels.OptionsViews.DataTransferObjects;
using H.Core.Enumerations;
using H.Core.Models;
using H.Avalonia.Services;
using H.Core.Services.StorageService;
using Prism.Events;
using Prism.Regions;

namespace H.Avalonia.ViewModels.OptionsViews
{
    public class SoilN2OBreakdownSettingsViewModel : ViewModelBase
    {
        #region Fields

        private SoilN2OBreakdownSettingsDTO _data = null!;
        private readonly IErrorHandlerService _errorHandlerService = null!;

        private bool _entriesAreValid;
        private double _totalUserEnteredPercentageOfAllMonths;
        private string _totalEnteredPercentageForAllMonthsMessage = string.Empty;

        #endregion

        #region Constructors

        public SoilN2OBreakdownSettingsViewModel(IStorageService storageService, IEventAggregator eventAggregator, IErrorHandlerService errorHandlerService) : base(storageService, eventAggregator)
        {
            if (errorHandlerService != null)
            {
                _errorHandlerService = errorHandlerService;
            }
            else
            {
                throw new ArgumentNullException(nameof(errorHandlerService));
            }
        }

        #endregion

        #region Properties

        public SoilN2OBreakdownSettingsDTO Data
        {
            get => _data;
            set => SetProperty(ref _data, value);
        }

        public double TotalUserEnteredPercentageOfAllMonths
        {
            get => _totalUserEnteredPercentageOfAllMonths;
            set => SetProperty(ref _totalUserEnteredPercentageOfAllMonths, value);
        }

        public string TotalEnteredPercentForAllMonthsMessage
        {
            get => _totalEnteredPercentageForAllMonthsMessage;
            set => SetProperty(ref _totalEnteredPercentageForAllMonthsMessage, value);
        }

        public bool AreEntriesValid
        {
            get => _entriesAreValid;
            set => SetProperty(ref _entriesAreValid, value);
        }

        #endregion

        #region Public Methods

        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (!IsInitialized)
            {
                Data = new SoilN2OBreakdownSettingsDTO(StorageService!);
                IsInitialized = true;
            }
            CalculateTotal();
            Data.PropertyChanged += ValidateTotalEquals100;
        }

        public override void OnNavigatedFrom(NavigationContext navigationContext)
        {
            if (_entriesAreValid)
            {
                Data.PropertyChanged -= ValidateTotalEquals100;
            }
        }

        #endregion

        #region Private Methods

        private void CalculateTotal()
        {
            double previousTotal = TotalUserEnteredPercentageOfAllMonths;
            TotalUserEnteredPercentageOfAllMonths = 0;
            foreach (Months month in Enum.GetValues(typeof(Months)))
            {
                TotalUserEnteredPercentageOfAllMonths += this.Data.MonthlyValues.GetValueByMonth(month);
                TotalEnteredPercentForAllMonthsMessage = String.Format(H.Core.Properties.Resources.CurrentMonthlyN2OValuesEqual, TotalUserEnteredPercentageOfAllMonths);
            }
            // If total of monthly N2O inputs equals 100%, publish validation pass event to release navigation lock
            if (TotalUserEnteredPercentageOfAllMonths == 100)
            {
                EventAggregator?.GetEvent<ValidationPassOccurredEvent>().Publish(new ErrorInformation(string.Format(H.Core.Properties.Resources.SumOfMonthlyN2OInputsPercent, TotalUserEnteredPercentageOfAllMonths)));
                AreEntriesValid = true;
                return;
            }
            // To avoid multiple warnings sent to ErrorHandlerService when user adjusts values multiple times above or below 100% threshold
            // E.g., If user adjusts from 101% to 102% a second warning should not be sent
            if (previousTotal == 100)
            {
                _errorHandlerService.HandleValidationWarning(H.Core.Properties.Resources.VerifyBeforeProceed, H.Core.Properties.Resources.CorrectN2OValuesBeforeNavigation);
            }

            AreEntriesValid = false;
        }

        #endregion

        #region Event Handlers

        private void ValidateTotalEquals100(object? sender, PropertyChangedEventArgs e)
        {
            CalculateTotal();
        }

        #endregion
    }
}
