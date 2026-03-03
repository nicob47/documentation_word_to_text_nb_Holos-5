using H.Core.Enumerations;
using H.Core.Properties;
using H.Infrastructure;
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

        private string _aboutHolosString = string.Empty;
        private string _toBeKeptInformedString = string.Empty;
        private string _disclaimerRtfString = string.Empty;
        private string _versionString = string.Empty;
        private string _disclaimerWordString = string.Empty;

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
            set { SetProperty(ref _selectedLanguage, value); }
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

        public string DisclaimerRtfString
        {
            get { return _disclaimerRtfString; }
            set { SetProperty(ref _disclaimerRtfString, value); }
        }
        public string DisclaimerWordString
        {
            get { return _disclaimerWordString; }
            set { SetProperty(ref _disclaimerWordString, value); }
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
            if (_countrySettings.Version == CountryVersion.Canada)
            {
                if (_countrySettings.Language == Languages.English)
                {
                    this.AboutHolosString = "HOLOS - a tool to estimate and reduce greenhouse gas emissions from farms";
                    this.ToBeKeptInformedString = "To be kept informed about  future versions, please send your contact information (including email address) to holos@agr.gc.ca";
                    this.DisclaimerRtfString = Resources.Disclaimer_English_TXT;

                    this.DisclaimerWordString = "Disclaimer";
                    Settings.Default.DisplayLanguage = Languages.English.GetDescription();
                }
                else
                {
                    this.AboutHolosString = "Holos - outil d'évaluation et de réduction des émissions de gaz à effet de serre des fermes agricoles";
                    this.ToBeKeptInformedString = "Pour être informé de la publication des prochaines versions du logiciel, faites parvenir vos coordonnées (y compris votre adresse électronique) à holos@agr.gc.ca";
                    this.DisclaimerRtfString = Resources.Disclaimer_French_TXT;
                    this.DisclaimerWordString = "Avis de non-responsabilité";

                    Settings.Default.DisplayLanguage = Languages.French.GetDescription();
                }
            }
            else
            {
                this.AboutHolosString = "HOLOS-IE - a tool to estimate and reduce greenhouse gas emissions from farms";
                this.ToBeKeptInformedString = "To be kept informed about  future versions, please send your contact information (including email address) to ibrahim.khalil1@ucd.ie";
                this.DisclaimerRtfString = Resources.Disclaimer_English_TXT;
            }
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
