using System.Globalization;

namespace H.Localization;

/// <summary>
/// High-level entry point for switching the application language at runtime.
///
/// <para><b>Call sites:</b></para>
/// <list type="bullet">
///   <item>
///     <see cref="H.Avalonia.App.SetLanguage"/> — called once during startup to
///     restore the previously chosen language from <c>app.config</c>.
///   </item>
///   <item>
///     <c>DisclaimerViewModel.OnLanguageChanged()</c> — called each time the
///     user selects a new language on the Disclaimer screen.
///   </item>
/// </list>
///
/// <para><b>What happens when <see cref="SetLanguage"/> is called:</b></para>
/// <list type="number">
///   <item>
///     Thread cultures (<see cref="CultureInfo.CurrentCulture"/> and
///     <see cref="CultureInfo.CurrentUICulture"/>) are updated so that
///     .NET resource managers resolve the correct satellite assembly.
///   </item>
///   <item>
///     <see cref="LocalizationService.Instance.CurrentCulture"/> is set,
///     which triggers <see cref="LocalizationService.Refresh"/>.
///   </item>
///   <item>
///     <c>Refresh()</c> fires <c>PropertyChanged("Item[]")</c> and
///     <c>PropertyChanged("")</c>, causing every <see cref="LocalizedString"/>
///     to re-publish its <c>Value</c> property — and the UI updates.
///   </item>
/// </list>
/// </summary>
public static class LanguageManager
{
    /// <summary>
    /// Switches the entire application to the specified culture.
    /// This is the <b>single method</b> that all language-change paths should call.
    /// </summary>
    /// <param name="culture">
    /// An IETF language tag such as <c>"en"</c> or <c>"fr"</c>.
    /// </param>
    public static void SetLanguage(string culture)
    {
        var ci = new CultureInfo(culture);

        // 1. Update the .NET thread cultures so that ResourceManager.GetString()
        //    resolves strings from the correct satellite assembly (e.g. fr/H.Localization.resources.dll).
        CultureInfo.CurrentCulture = ci;
        CultureInfo.CurrentUICulture = ci;

        // 2. Assign the new culture to LocalizationService. The setter automatically
        //    calls Refresh(), which fires PropertyChanged → every LocalizedString
        //    re-publishes its Value → Avalonia re-reads the translated text.
        LocalizationService.Instance.CurrentCulture = ci;
    }

    /// <summary>
    /// Gets the current language culture code (e.g., <c>"en"</c>, <c>"fr"</c>).
    /// </summary>
    public static string CurrentLanguage => LocalizationService.Instance.CurrentCulture.TwoLetterISOLanguageName;
}
