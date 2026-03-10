using System.ComponentModel;

namespace H.Localization;

/// <summary>
/// A lightweight wrapper that exposes a single localized string as a bindable
/// <see cref="Value"/> property with full <see cref="INotifyPropertyChanged"/> support.
///
/// <para><b>Why this class exists:</b></para>
/// <para>
/// Avalonia 11's reflection-mode binding engine does not reliably re-evaluate
/// indexer bindings (e.g. <c>{Binding [Key], Source={StaticResource Loc}}</c>)
/// when the source raises <c>PropertyChanged("Item[]")</c>.
/// By wrapping each key in this class, we give Avalonia a <b>named property</b>
/// (<c>Value</c>) to observe. Named-property change notifications are handled
/// correctly by Avalonia 11.
/// </para>
///
/// <para><b>XAML usage:</b></para>
/// <code>
///   Text="{Binding [Label_Calf].Value, Source={StaticResource Loc}}"
/// </code>
///
/// <para><b>How updates propagate:</b></para>
/// <list type="number">
///   <item>
///     <see cref="LocalizationService.Refresh"/> fires
///     <c>PropertyChanged("Item[]")</c> and <c>PropertyChanged("")</c>.
///   </item>
///   <item>
///     <see cref="OnServicePropertyChanged"/> catches the event and fires
///     <c>PropertyChanged("Value")</c> on <b>this</b> object.
///   </item>
///   <item>
///     Avalonia re-reads <see cref="Value"/>, which delegates to
///     <see cref="LocalizationService.Instance"/>'s indexer using the
///     now-updated culture → the UI shows the new language.
///   </item>
/// </list>
///
/// <para>
/// Instances are cached per-key inside <see cref="LocalizationProvider"/>,
/// so the same <c>LocalizedString</c> is reused across all views that
/// reference the same resource key.
/// </para>
/// </summary>
public sealed class LocalizedString : INotifyPropertyChanged
{
    private readonly string _key;

    /// <summary>
    /// Creates a new wrapper for the given resource <paramref name="key"/> and
    /// subscribes to <see cref="LocalizationService.Instance"/> so that
    /// language changes are automatically propagated.
    /// </summary>
    public LocalizedString(string key)
    {
        _key = key;

        // Subscribe to the singleton so that every language change triggers
        // a named PropertyChanged("Value") notification on THIS object.
        LocalizationService.Instance.PropertyChanged += OnServicePropertyChanged;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// The current localized text for the key this instance was created with.
    /// This property is re-evaluated on every access (no caching), so it always
    /// reflects the current <see cref="LocalizationService.Instance.CurrentCulture"/>.
    /// </summary>
    public string Value => LocalizationService.Instance[_key];

    /// <summary>
    /// Returns the localized string value. Acts as a fallback if the binding
    /// engine displays the object directly instead of resolving <see cref="Value"/>.
    /// </summary>
    public override string ToString() => Value;

    /// <summary>
    /// Called whenever <see cref="LocalizationService.Instance"/> raises
    /// <see cref="INotifyPropertyChanged.PropertyChanged"/>. We respond by
    /// re-publishing <see cref="Value"/> so that all XAML bindings refresh.
    /// </summary>
    private void OnServicePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
    }
}
