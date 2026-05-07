using System.Configuration;
using H.Core.Enumerations;

namespace H.Core.Helpers
{
    public class ConfigurationFileHelper
    {
        #region Fields

        #endregion

        #region Public Methods

        /// <summary>
        /// Reads application settings file to see if a boolean value has been set
        /// </summary>
        /// <param name="settingName">The name of the setting to check</param>
        /// <returns><c>true</c> if the setting exists and is enabled, <c>false</c> if the setting does not exist or is not enabled</returns>
        public static bool BooleanAppSettingIsEnabled(string settingName)
        {
            var result = false;

            try
            {
                ConfigurationManager.RefreshSection("appSettings");

                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings.AllKeys.Contains(settingName))
                {
                    var setting = config.AppSettings.Settings[settingName]?.Value;
                    if (setting != null)
                    {
                        result = setting.Equals("true", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("t", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("1", StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }
            catch (Exception)
            {
            }

            return result;
        }

        /// <summary>
        /// Returns the system version being used. This is not the same as the version number
        /// </summary>
        /// <returns>A value from the <see cref="CountryVersion"/> indicating which system version is enabled</returns>
        public static CountryVersion GetCountryVersion()
        {
            return CountryVersion.Canada;
        }

        /// <summary>
        /// Reads the persisted language preference from <c>app.config</c>.
        /// This is called by <see cref="H.Core.Services.CountrySettings"/> during construction
        /// to initialize <see cref="H.Core.Services.ICountrySettings.Language"/>, which in turn
        /// is read by <c>App.SetLanguage()</c> on startup to restore the user's language choice.
        /// </summary>
        /// <returns>
        /// <see cref="Languages.French"/> if the <c>"Language"</c> appSetting equals
        /// <c>"french"</c> (case-insensitive); otherwise <see cref="Languages.English"/>.
        /// </returns>
        public static Languages GetLanguage()
        {
            Languages language = Languages.English;
            const string settingName = "Language";

            try
            {
                ConfigurationManager.RefreshSection("appSettings");

                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings.AllKeys.Contains(settingName))
                {
                    var setting = config.AppSettings.Settings[settingName]?.Value;
                    if (setting != null && setting.Equals("french", StringComparison.InvariantCultureIgnoreCase))
                    {
                        language = Languages.French;
                    }
                }
            }
            catch (Exception)
            {
            }

            return language;
        }

        /// <summary>
        /// Persists the user's language choice to the <c>"Language"</c> appSetting in
        /// <c>app.config</c>. Called from <c>DisclaimerViewModel.OnLanguageChanged()</c>
        /// whenever the user selects a different language.
        ///
        /// <para>
        /// On the next application launch, <see cref="GetLanguage"/> reads this value
        /// back, which populates <see cref="H.Core.Services.ICountrySettings.Language"/>,
        /// and <c>App.SetLanguage()</c> uses it to call
        /// <see cref="H.Localization.LanguageManager.SetLanguage"/> — completing the
        /// round-trip persistence cycle.
        /// </para>
        /// </summary>
        /// <param name="language">
        /// The language identifier to persist — <c>"english"</c> or <c>"french"</c>.
        /// </param>
        public static void UpdateLanguage(string language)
        {
            const string settingName = "Language";
            try
            {
                ConfigurationManager.RefreshSection("appSettings");

                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings.AllKeys.Contains(settingName))
                {
                    config.AppSettings.Settings[settingName].Value = language;
                }
                else
                {
                    config.AppSettings.Settings.Add(settingName, language);
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception)
            {
                // Handle exceptions as needed
            }
        }

        /// <summary>
        /// Updates the CountryVersion setting in the App.config file.
        /// </summary>
        /// <param name="countryVersion">The country version to set.</param>
        public static void UpdateCountryVersion(string countryVersion)
        {
            try
            {
                ConfigurationManager.RefreshSection("appSettings");

                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                if (config.AppSettings.Settings.AllKeys.Contains("CountryVersion"))
                {
                    config.AppSettings.Settings["CountryVersion"].Value = countryVersion;
                }
                else
                {
                    config.AppSettings.Settings.Add("CountryVersion", countryVersion);
                }

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception)
            {
                // Handle exceptions as needed
            }
        }

        #endregion
    }
}