using System.ComponentModel;
using System.Globalization;

namespace H.Localization
{
    /// <summary>
    /// Singleton service that manages the current culture and provides localized string lookups.
    ///
    /// <para><b>Translation flow overview:</b></para>
    /// <list type="number">
    ///   <item>
    ///     User selects a language on the Disclaimer screen
    ///     → <see cref="H.Localization.LanguageManager.SetLanguage"/> is called.
    ///   </item>
    ///   <item>
    ///     <c>LanguageManager.SetLanguage("fr")</c> sets the thread cultures
    ///     <b>and</b> assigns <see cref="CurrentCulture"/> on this singleton.
    ///   </item>
    ///   <item>
    ///     The <see cref="CurrentCulture"/> setter calls <see cref="Refresh"/>,
    ///     which fires <see cref="PropertyChanged"/> with <c>"Item[]"</c> and <c>""</c>.
    ///   </item>
    ///   <item>
    ///     Every <see cref="LocalizedString"/> instance is subscribed to this event.
    ///     When it fires, each <c>LocalizedString</c> raises its own
    ///     <c>PropertyChanged("Value")</c>, which Avalonia's binding engine observes.
    ///   </item>
    ///   <item>
    ///     Avalonia re-reads <see cref="LocalizedString.Value"/>, which calls
    ///     the <see cref="this[string]">indexer</see> with the new culture
    ///     → the UI updates to the selected language.
    ///   </item>
    /// </list>
    ///
    /// <para>
    /// The indexer delegates to <c>AppStrings.ResourceManager.GetString(key, culture)</c>,
    /// which loads translations from the embedded <c>.resx</c> satellite assemblies
    /// (e.g. <c>fr/H.Localization.resources.dll</c>).
    /// </para>
    /// </summary>
    public sealed class LocalizationService : INotifyPropertyChanged
    {
        /// <summary>
        /// The application-wide singleton. Created lazily on first access.
        /// </summary>
        public static LocalizationService Instance { get; } = new();

        private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

        private LocalizationService() { }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the culture used for all subsequent resource lookups.
        /// Setting this property automatically calls <see cref="Refresh"/> so
        /// that every active <see cref="LocalizedString"/> re-publishes its value.
        /// </summary>
        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                if (!Equals(_currentCulture, value))
                {
                    _currentCulture = value;
                    Refresh();
                }
            }
        }

        /// <summary>
        /// Looks up a localized string by resource key using the current culture.
        /// Returns <c>!key!</c> if the key is not found (makes missing keys obvious in the UI).
        /// </summary>
        /// <param name="key">
        /// A resource key defined in <c>AppStrings.resx</c> (and its culture-specific variants).
        /// </param>
        public string this[string key]
        {
            get
            {
                return Resources.Strings.AppStrings.ResourceManager
                           .GetString(key, _currentCulture)
                       ?? $"!{key}!";
            }
        }

        /// <summary>
        /// Notifies all subscribers (primarily <see cref="LocalizedString"/> instances
        /// and <see cref="LocalizationProvider"/>) that the culture has changed and
        /// every cached string value must be re-read.
        /// </summary>
        public void Refresh()
        {
            // "Item[]" tells the binding engine that all indexer values have changed.
            // "" (empty) is the INPC convention for "everything changed".
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }
}
