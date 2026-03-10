using H.Core.Enumerations;
using H.Core.Helpers;
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

        /// <summary>
        /// Sets the initial language when the Disclaimer screen is first displayed.
        /// Reads the persisted language from <see cref="ICountrySettings"/> (which loads
        /// from <c>app.config</c>) and applies it via <see cref="LanguageManager.SetLanguage"/>.
        /// </summary>
        private void UpdateDisplay()
        {
            // Restore the language that was persisted during the previous session
            _selectedLanguage = _countrySettings?.Language ?? Languages.English;

            // Apply the culture so LocalizationService returns the correct translations
            var culture = _selectedLanguage == Languages.French ? "fr" : "en";
            LanguageManager.SetLanguage(culture);

            // Keep legacy Settings in sync
            Settings.Default.DisplayLanguage = _selectedLanguage.GetDescription();

            // Load country-specific disclaimer strings
            RefreshLocalizedStrings();
        }

        /// <summary>
        /// Called when the user selects a different language on the Disclaimer screen.
        /// This method is responsible for <b>three things</b>:
        ///
        /// <list type="number">
        ///   <item>
        ///     <b>Immediate UI update</b> — <see cref="LanguageManager.SetLanguage"/>
        ///     sets thread cultures and <see cref="LocalizationService.Instance.CurrentCulture"/>,
        ///     which fires <c>PropertyChanged</c> on every <see cref="LocalizedString"/>
        ///     → all XAML bindings using <c>{Binding [Key].Value, Source={StaticResource Loc}}</c>
        ///     refresh automatically.
        ///   </item>
        ///   <item>
        ///     <b>Persistence</b> — The choice is saved in three places so it survives
        ///     app restarts: (a) <c>Settings.Default.DisplayLanguage</c>,
        ///     (b) the <see cref="ICountrySettings"/> singleton (used by <c>App.SetLanguage()</c>
        ///     on startup), and (c) <c>app.config</c> via
        ///     <see cref="ConfigurationFileHelper.UpdateLanguage"/> (the ultimate source
        ///     that <c>CountrySettings</c> reads during construction).
        ///   </item>
        ///   <item>
        ///     <b>Legacy sync</b> — Explicit <c>.Culture</c> assignments on
        ///     <c>H.Avalonia.Resources</c> and <c>H.Core.Properties.Resources</c> keep
        ///     any remaining <c>{x:Static}</c> bindings in sync.
        ///   </item>
        /// </list>
        /// </summary>
        private void OnLanguageChanged()
        {
            // 1. Immediate UI update — sets thread cultures + triggers LocalizedString refresh
            var culture = _selectedLanguage == Languages.French ? "fr" : "en";
            LanguageManager.SetLanguage(culture);

            // 2a. Persist to Settings.Default (legacy)
            Settings.Default.DisplayLanguage = _selectedLanguage.GetDescription();

            // 2b. Update the DI singleton so App.SetLanguage() sees the correct value
            //     during the current session (avoids requiring an app restart)
            if (_countrySettings != null)
            {
                _countrySettings.Language = _selectedLanguage;
            }

            // 2c. Persist to app.config so CountrySettings reads it back on next launch
            ConfigurationFileHelper.UpdateLanguage(_selectedLanguage == Languages.French ? "french" : "english");

            // 3. Keep legacy resource classes in sync for any {x:Static} bindings
            if (_selectedLanguage == Languages.French)
            {
                H.Avalonia.Resources.Culture = InfrastructureConstants.FrenchCultureInfo;
                H.Core.Properties.Resources.Culture = InfrastructureConstants.FrenchCultureInfo;
            }
            else
            {
                H.Avalonia.Resources.Culture = null;
                H.Core.Properties.Resources.Culture = null;
            }

            // Refresh any ViewModel-held strings (country-specific disclaimer text)
            RefreshLocalizedStrings();
        }

        /// <summary>
        /// Re-reads country-specific and common localized strings from the
        /// <see cref="LocalizationService"/> and assigns them to ViewModel properties.
        /// These properties are bound directly in the Disclaimer XAML (they are
        /// <b>not</b> part of the <c>LocalizedString / LocalizationProvider</c> pipeline
        /// because they require runtime branching on <see cref="CountryVersion"/>).
        /// </summary>
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
