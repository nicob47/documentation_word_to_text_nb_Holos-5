using System.ComponentModel;
using System.Globalization;

namespace H.Localization;

/// <summary>
/// XAML-friendly wrapper for LocalizationService that can be instantiated as a resource.
/// This class delegates to the singleton LocalizationService.Instance.
/// </summary>
public class LocalizationProvider : INotifyPropertyChanged
{
    public LocalizationProvider()
    {
        // Subscribe to the singleton's property changes and forward them
        LocalizationService.Instance.PropertyChanged += (s, e) =>
            PropertyChanged?.Invoke(this, e);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Indexer for looking up localized strings by key.
    /// Usage in XAML: Text="{Binding [KeyName], Source={StaticResource Loc}}"
    /// </summary>
    public string this[string key] => LocalizationService.Instance[key];
}
