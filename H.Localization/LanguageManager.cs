using System.Globalization;

namespace H.Localization;

public static class LanguageManager
{
    public static void SetLanguage(string culture)
    {
        var ci = new CultureInfo(culture);

        // Set thread cultures
        CultureInfo.CurrentCulture = ci;
        CultureInfo.CurrentUICulture = ci;

        // Set the LocalizationService culture (this will trigger Refresh automatically)
        LocalizationService.Instance.CurrentCulture = ci;
    }

    /// <summary>
    /// Gets the current language culture code (e.g., "en", "fr").
    /// </summary>
    public static string CurrentLanguage => LocalizationService.Instance.CurrentCulture.TwoLetterISOLanguageName;
}
