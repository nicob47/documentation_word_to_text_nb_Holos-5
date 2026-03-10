using System.Collections.Concurrent;
using System.ComponentModel;

namespace H.Localization;

/// <summary>
/// XAML-friendly resource that exposes localized strings to Avalonia data bindings.
///
/// <para><b>Registration:</b></para>
/// <para>
/// A single instance is declared in <c>App.axaml</c> as an application-level resource:
/// <code>
///   &lt;loc:LocalizationProvider x:Key="Loc"/&gt;
/// </code>
/// All views in the application share this one instance.
/// </para>
///
/// <para><b>XAML binding pattern:</b></para>
/// <code>
///   Text="{Binding [Label_Calf].Value, Source={StaticResource Loc}}"
/// </code>
/// <para>
/// The indexer returns a cached <see cref="LocalizedString"/> wrapper whose
/// <see cref="LocalizedString.Value"/> property holds the translated text.
/// Because each <c>LocalizedString</c> fires <c>PropertyChanged("Value")</c>
/// when the language changes, Avalonia 11's binding engine correctly refreshes
/// the UI text — something it cannot do reliably with raw indexer bindings.
/// </para>
///
/// <para><b>Why a wrapper is needed:</b></para>
/// <para>
/// Avalonia 11's reflection-mode binding engine does not reliably re-evaluate
/// indexer bindings when the source raises <c>PropertyChanged("Item[]")</c>.
/// By returning a <see cref="LocalizedString"/> object with a named <c>Value</c>
/// property, we give the binding engine a concrete property to observe.
/// </para>
///
/// <para><b>Caching:</b></para>
/// <para>
/// <see cref="LocalizedString"/> instances are cached per key in a
/// <see cref="ConcurrentDictionary{TKey,TValue}"/> so the same object is
/// reused across all views that reference the same resource key. This keeps
/// memory usage constant regardless of how many views bind to a given key.
/// </para>
/// </summary>
public class LocalizationProvider : INotifyPropertyChanged
{
    private readonly ConcurrentDictionary<string, LocalizedString> _cache = new();

    /// <summary>
    /// Subscribes to <see cref="LocalizationService.Instance"/> change notifications
    /// and forwards them so that any legacy code listening directly to
    /// <see cref="PropertyChanged"/> on this provider continues to work.
    /// </summary>
    public LocalizationProvider()
    {
        LocalizationService.Instance.PropertyChanged += (s, e) =>
            PropertyChanged?.Invoke(this, e);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Returns a cached <see cref="LocalizedString"/> for <paramref name="key"/>.
    ///
    /// <para>
    /// In XAML, bind to the <c>Value</c> sub-property:
    /// <code>
    ///   {Binding [MyKey].Value, Source={StaticResource Loc}}
    /// </code>
    /// The <c>LocalizedString.Value</c> getter delegates to
    /// <see cref="LocalizationService.Instance"/>'s indexer with the current
    /// culture, so it always returns the correct translation.
    /// </para>
    /// </summary>
    /// <param name="key">
    /// A resource key defined in <c>AppStrings.resx</c> (and its culture-specific variants).
    /// </param>
    public LocalizedString this[string key] =>
        _cache.GetOrAdd(key, static k => new LocalizedString(k));
}
