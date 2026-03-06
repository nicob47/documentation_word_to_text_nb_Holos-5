using H.Core.Enumerations;
using H.Core.Properties;
using H.Infrastructure;
using H.Localization;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using H.Core.Services;
using Prism.Commands;

namespace H.Avalonia.ViewModels.SupportingViews.Disclaimer
{
    public class DisclaimerViewModel : ViewModelBase
    {
        #region Fields

        private Languages _selectedLanguage;

        private string _aboutHolosString;
        private string _toBeKeptInformedString;
        private string _disclaimerTitle;
        private string _disclaimerText;
        private string _versionString;

        private DelegateCommand<object> _okCommand = null!;

        private readonly ICountrySettings _countrySettings = null!;

        #endregion

        #region Constructors

        public DisclaimerViewModel()
        {
            this.Construct();
        }

        public DisclaimerViewModel(IRegionManager regionManager,
                                   IEventAggregator eventAggregator,
                                   ICountrySettings countrySettings) : base(regionManager, eventAggregator)
        {
            if (countrySettings != null)
            {
                _countrySettings = countrySettings; 
            }
            else
            {
                throw new ArgumentNullException(nameof(countrySettings));
            }

            LanguageCollection = new ObservableCollection<Languages>(EnumHelper.GetValues<Languages>());
            this.Construct();
        }

        #endregion

        #region Properties
        public ObservableCollection<Languages> LanguageCollection { get; set; } = null!;

        public Languages SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                if (SetProperty(ref _selectedLanguage, value))
                {
                    OnLanguageChanged();
                }
            }
        }

        public string AboutHolosString
        {
            get { return _aboutHolosString; }
            set { SetProperty(ref _aboutHolosString, value); }
        }

        public string ToBeKeptInformedString
        {
            get { return _toBeKeptInformedString; }
            set { SetProperty(ref _toBeKeptInformedString, value); }
        }

        public string DisclaimerTitle
        {
            get { return _disclaimerTitle; }
            set { SetProperty(ref _disclaimerTitle, value); }
        }

        public string DisclaimerText
        {
            get { return _disclaimerText; }
            set { SetProperty(ref _disclaimerText, value); }
        }

        public string VersionString
        {
            get { return _versionString; }
            set { _versionString = value; }
        }

        public DelegateCommand<object> OkCommand
        {
            get => _okCommand;
            set => SetProperty(ref _okCommand, value);
        }

        #endregion

        #region Public Methods

        public void Construct()
        {
            this.UpdateDisplay();
            this.VersionString = GuiConstants.GetVersionString();

            this.OkCommand = new DelegateCommand<object>(OnOkExecute, OkCanExecute);
        }

        #endregion

        #region Private Methods

        private void UpdateDisplay()
        {
            // Set initial selected language
            _selectedLanguage = _countrySettings?.Language ?? Languages.English;

            // Set language culture for localization
            var culture = _selectedLanguage == Languages.French ? "fr" : "en";
            LanguageManager.SetLanguage(culture);

            // Set language setting
            Settings.Default.DisplayLanguage = _selectedLanguage.GetDescription();

            // Get country-specific strings from localization
            RefreshLocalizedStrings();
        }

        private void OnLanguageChanged()
        {
            // Update culture for localization
            var culture = _selectedLanguage == Languages.French ? "fr" : "en";
            LanguageManager.SetLanguage(culture);

            // Update settings
            Settings.Default.DisplayLanguage = _selectedLanguage.GetDescription();

            // Refresh country-specific strings
            RefreshLocalizedStrings();
        }

        private void RefreshLocalizedStrings()
        {
            // Get country-specific strings from localization
            if (_countrySettings?.Version == CountryVersion.Canada)
            {
                this.AboutHolosString = LocalizationService.Instance["AboutHolos"];
                this.ToBeKeptInformedString = LocalizationService.Instance["ToBeKeptInformed"];
            }
            else
            {
                // Ireland version
                this.AboutHolosString = LocalizationService.Instance["AboutHolosIE"];
                this.ToBeKeptInformedString = LocalizationService.Instance["ToBeKeptInformedIE"];
            }

            // Common localized strings
            this.DisclaimerTitle = LocalizationService.Instance["DisclaimerTitle"];
            this.DisclaimerText = LocalizationService.Instance["DisclaimerText"];
        }

        #endregion

        #region Event Handlers

        private void OnOkExecute(object obj)
        {                                        
            // Navigate to next view
            base.RegionManager?.RequestNavigate(UiRegions.ContentRegion, nameof(Views.SupportingViews.Start.StartView));
        }

        private bool OkCanExecute(object arg)
        {
            return true;
        }

        #endregion
    }
}
