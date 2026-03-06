using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace H.Localization
{
    public sealed class LocalizationService : INotifyPropertyChanged
    {
        public static LocalizationService Instance { get; } = new();

        private CultureInfo _currentCulture = CultureInfo.CurrentUICulture;

        private LocalizationService() { }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Gets or sets the current culture used for resource lookups.
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
        /// Indexer for looking up localized strings by key.
        /// Usage in XAML: Text="{Binding [KeyName], Source={StaticResource Loc}}"
        /// </summary>
        public string this[string key]
        {
            get
            {
                return Resources.Strings.AppStrings.ResourceManager
                           .GetString(key, _currentCulture)
                       ?? $"!{key}!";
            }
        }

        public void Refresh()
        {
            // Refresh ALL bindings - "Item[]" is required for indexer bindings in Avalonia
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(string.Empty));
        }
    }
}
