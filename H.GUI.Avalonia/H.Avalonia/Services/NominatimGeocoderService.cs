using Avalonia.Controls.Notifications;
using H.Core.Enumerations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SharpKml.Dom.Xal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia.Animation;
using NLog.Config;
using Tmds.DBus.Protocol;
using Path = System.IO.Path;

namespace H.Avalonia.Services
{
    /// <summary>
    /// Geocoding service that uses the Nominatim API to get latitude and longitude coordinates for a given address.
    /// For best results as of February 2026, street searches return most reliable results. Specific address searches can yield inconsistent results especially for rural addresses.
    /// </summary>
    public class NominatimGeocoderService : IDefaultGeocoderService
    {
        #region Fields

        private enum InputValidationType
        {
            General,
            Address,
            Municipality,
            PostalCode,
            Country,
            County
        }

        private ILogger _logger;
        private INotificationManagerService _notificationManagerService;

        private const int ApiTimeout = 60;
        private const int ApiLockoutSeconds = 5;

        private int _searchAttemptsMade = 0;
        private const int _searchAttemptsLimit = 3;

        private DateTime _lastApiRequestTime = DateTime.MinValue;

        #endregion

        #region Properties

        #endregion

        #region Constructors

        public NominatimGeocoderService(ILogger logger, INotificationManagerService notificationManagerService)
        {
            if (logger != null)
            {
                _logger = logger;
            }
            else
            {
                throw new ArgumentNullException(nameof(logger));
            }

            if (notificationManagerService != null)
            {
                _notificationManagerService = notificationManagerService;
            }
            else
            {
                throw new ArgumentNullException(nameof(notificationManagerService));
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Checks if the address is already cached.
        /// </summary>
        /// <param name="street">The street address to check if geocode data is cached</param>
        /// <param name="municipality">The municipality of the address to check if geocode data is cached</param>
        /// <param name="province">The province of the address to check if geocode data is cached</param>
        /// <param name="postalCode">The postal code of the address to check if geocode data is cached</param>
        /// <param name="county">The county of the address to check if geocode data is cached</param>
        /// <param name="country">The country of the address to check if geocode data is cached, defaults to Canada.</param>
        /// <returns>True if cached file exists for this address, false otherwise</returns>
        public bool IsCached(string street, string municipality, Province province, string postalCode, string? county = null, string country = "Canada")
        {
            InputValidation(street, municipality, postalCode, county, country);
            street = PrepareStreetStringForApi(street);
            var path = this.GetCachePath(street, municipality, province, postalCode, county, country);
            return File.Exists(path);
        }

        /// <summary>
        /// Returns boolean on if the API request is ready or not due to previous request being made too recently or too many requests made without a successful API call.
        /// </summary>
        /// <returns>True if an API call can be made, false if an API call can't be made.</returns>
        public bool IsReadyForRequest()
        {
            return !((DateTime.Now - _lastApiRequestTime).TotalSeconds < ApiLockoutSeconds || (_searchAttemptsMade >= _searchAttemptsLimit));
        }

        /// <summary>
        /// Returns the latitude and longitude for the given address.
        /// </summary>
        /// <param name="street">The street address to geocode and get coordinates for</param>
        /// <param name="municipality">The municipality of the address to geocode and get coordinates for</param>
        /// <param name="province">The province of the address to geocode and get coordinates for</param>
        /// <param name="postalCode">The postal code of the address to geocode and get coordinates for</param>
        /// <param name="country">The country of the address to geocode and get coordinates for, defaults to Canada.</param>
        /// <param name="county">The county of the address to geocode and get coordinates</param>
        /// <returns>Longitude and latitude coordinates</returns>
        public async Task<(double latitude, double longitude)> GetCoordinates(string street, string municipality, Province province, string postalCode, string? county = null, string country = "Canada")
        {
            InputValidation(street, municipality, postalCode, county, country);
            street = PrepareStreetStringForApi(street);
            // If no cached data, get data from Nominatim API and cache it.
            string? content = this.GetCachedData(street, municipality, province, postalCode, county, country);
            if (string.IsNullOrWhiteSpace(content))
            {
                content = await GetAndCacheNominatimData(street, municipality, province, postalCode, county, country);
            }
            // If cached data or API data was available, parse it and return coordinates.
            if (!string.IsNullOrWhiteSpace(content))
            {
                return ParseNominatimApiContentForCoordinates(content);
            }
            // Hit if there was an error downloading data from the API and no cached data available.
            _logger.LogError($"{nameof(NominatimGeocoderService)}: there was an error while trying to download Nominatim coordinates data,");
            return (0, 0);
        }

        /// <summary>
        /// Returns a JObject of all the data returned from the Nominatim API for the given address
        /// </summary>
        /// <param name="street">The street address to geocode and get coordinates for</param>
        /// <param name="municipality">The municipality of the address to geocode and get coordinates for</param>
        /// <param name="province">The province of the address to geocode and get coordinates for</param>
        /// <param name="postalCode">The postal code of the address to geocode and get coordinates for</param>
        /// <param name="county">The county of the address to geocode and get coordinates</param>
        /// <param name="country">The country of the address to geocode and get coordinates for, defaults to Canada.</param>
        /// <returns>JObject containing all the data returned from the Nominatim API for the given address</returns>
        public async Task <JObject?> GetApiContent(string street, string municipality, Province province, string postalCode, string? county = null, string country = "Canada")
        {
            InputValidation(street, municipality, postalCode, county, country);
            street = PrepareStreetStringForApi(street);
            // If no cached data, get data from Nominatim API and cache it.
            string? content = this.GetCachedData(street, municipality, province, postalCode, county, country);
            if (string.IsNullOrWhiteSpace(content))
            {
                content = await GetAndCacheNominatimData(street, municipality, province, postalCode, county, country);
            }
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }
            return JArray.Parse(content).FirstOrDefault() as JObject;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Prepares the street address string for styling that works better with Nominatim 
        /// </summary>
        /// <param name="street">The street address string to reformat to a preferable format</param>
        /// <returns>String with sanitized address formatting specialized for Nominatim API Call.</returns>
        private string PrepareStreetStringForApi(string street)
        {
            // Dictionary containing directions that can be converted to their abbreviated counterpart
            // Nominatim requires street name direction suffix be abbreviated
            // Ex. "South Parkside Drive South" returns invalid, "South Parkside Drive S" or "S Parkside Drive S" returns valid coordinates
            var directions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"northwest", "NW"},
                {"northeast", "NE"},
                {"southwest", "SW"},
                {"southeast", "SE"},
                {"north", "N"},
                {"south", "S"},
                {"east", "E"},
                {"west", "W"}
            };
            // Only match pattern when word is isolated by space or punctuation. 
            // Ex. "StreetName South, Alberta" becomes "Streetname S, Alberta". "Southstreet Avenue, Alberta" remains the same.
            var pattern = @"\b(" + string.Join("|", directions.Keys.OrderByDescending(k => k.Length)) + @")\b";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);

            string convertedAddress = regex.Replace(street, match =>
            {
                var key = match.Value.ToLower();
                return directions.ContainsKey(key) ? directions[key] : match.Value;
            });
            return convertedAddress;
        }

        /// <summary>
        /// Gets the correct Nominatim API URL for the given address.
        /// </summary>
        /// <param name="street">The street address used in the api call</param>
        /// <param name="municipality">The municipality used in the api call</param>
        /// <param name="province">The province used in the api call</param>
        /// <param name="postalCode">The postal code used in the api call</param>
        /// <param name="county">The county used in the api call</param>
        /// <param name="country">The country used in the api call</param>
        /// <returns>Returns the url needed to access the API for the given address.</returns>
        private string GetCorrectApiUrl(string street, string municipality, Province province, string postalCode, string? county, string country)
        {
            street = Uri.EscapeDataString(street);
            string streetParameter = $"street={street}";

            municipality = Uri.EscapeDataString(municipality);
            string cityParameter = $"&city={municipality}";

            string provinceString = Uri.EscapeDataString(FormatProvinceStringForApiCall(province));
            string stateParameter = $"&state={provinceString}";

            postalCode = Uri.EscapeDataString(postalCode);
            string postalCodeParameter = $"&postalcode={postalCode}";

            string countyParameter = string.Empty;
            if (county != null)
            {
                county = Uri.EscapeDataString(county);
                countyParameter = $"&county={county}";
            }
            
            country = Uri.EscapeDataString(country);
            string countryParameter = $"&country={country}";


            string format = "json";


            string Url = $"https://nominatim.openstreetmap.org/search?{streetParameter}{cityParameter}{countyParameter}{stateParameter}{countryParameter}{postalCodeParameter}&format={format}&addressdetails=1&limit=1";
            return Url;
        }

        /// <summary>
        /// Takes in province enum and outputs province in string form containing spaces if province contains space in name (Nova Scotia) as converting enum directly to string lacks space and causes issue wih API call
        /// </summary>
        /// <param name="province">The province being converted to api formatted string</param>
        /// <returns>The formatted province string</returns>
        private string FormatProvinceStringForApiCall(Province province)
        {
            switch (province)
            {
                case Province.NovaScotia:
                    return "Nova Scotia";
                case Province.PrinceEdwardIsland:
                    return "Prince Edward Island";
                default:
                    return province.ToString();
            }

        }

        /// <summary>
        /// Returns geocoding data for the specified address from the Nominatim API and caches the result.
        /// </summary>
        /// <param name="street">The street address used in the api call</param>
        /// <param name="municipality">The municipality used in the api call</param>
        /// <param name="province">The province used in the api call</param>
        /// <param name="postalCode">The postal code used in the api call</param>
        /// <param name="county">The county used in the api call</param>
        /// <param name="country">The country used in the api call</param>
        /// <returns>A JSON string containing the geocoding data for the specified address if the API call is successful; otherwise, returns null.</returns>
        private async Task<string?> GetAndCacheNominatimData(string street, string municipality, Province province, string postalCode, string? county, string country)
        {
            // Check if request is locked out due to previous request being made too recently
            if (((DateTime.Now - _lastApiRequestTime).TotalSeconds < ApiLockoutSeconds))
            {
                _notificationManagerService.ShowToast("Too many requests made too quickly.", $"Please wait {ApiLockoutSeconds - (DateTime.Now - _lastApiRequestTime).TotalSeconds:F0} seconds before looking up another address.");
                _logger.LogWarning($"{nameof(NominatimGeocoderService)}: API request blocked due to lockout timer.");
                return null;
            }
            _lastApiRequestTime = DateTime.Now; // Update last API request time to now since we are making a request

            // Check if we have exceeded our search attempts limit
            if (_searchAttemptsMade >= _searchAttemptsLimit)
            {
                _notificationManagerService.ShowToast(H.Core.Properties.Resources.TooManyAddressSearches, H.Core.Properties.Resources.DescriptionTooManyAddressSearches, NotificationType.Warning);
                _logger.LogWarning($"{nameof(NominatimGeocoderService)}: API request blocked due too many requests being made.");
                return null;
            }

            string apiUrl = GetCorrectApiUrl(street, municipality, province, postalCode, county, country);
            string? content = null;
            try 
            {
                // Run a task that forces the Nominatim API to timeout if the timeout property isn't able to gracefully time out the API call. If the API
                // does not respond within this time (slow internet connection or API issues), we return null.
                var getNominatimApi = Task.Run(() => DownloadNominatimApiData(apiUrl));
                if (getNominatimApi.Wait(TimeSpan.FromSeconds(ApiTimeout)))
                {
                    _logger.LogInformation($"{nameof(NominatimGeocoderService)}.{nameof(GetCoordinates)}, Nominatim API Task Status: {getNominatimApi.Status}");
                    content = getNominatimApi.Result;
                    // Check if content returned by API is empty, if so do not return
                    if (content == "[]")
                    {
                        _searchAttemptsMade += 1; // Increment search attempts since we got an empty response
                        _logger.LogError($"{nameof(NominatimGeocoderService)}.{nameof(DownloadNominatimApiData)}: API content empty. Address not valid.");
                        _notificationManagerService.ShowToast(H.Core.Properties.Resources.InvalidAddress, H.Core.Properties.Resources.CantFindCoordinate, NotificationType.Error);
                        return null;
                    }
                    _searchAttemptsMade = 0; // Reset the search attempts since we were successful.
                    CacheData(content, street, municipality, province, postalCode, county, country);
                }
                else
                {
                    _notificationManagerService.ShowToast(H.Core.Properties.Resources.ErrorCouldNotReachNominatimApi, H.Core.Properties.Resources.ErrorCouldNotReachNominatimApiDescription, NotificationType.Error);
                    _logger.LogError($"{nameof(NominatimGeocoderService)}.{nameof(GetCoordinates)}, Nominatim API Task Status: {getNominatimApi.Status}");
                    throw new Exception("Nominatim API couldn't be reached or connection timed out.");
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Could not load data from Nominatim API. Exception thrown.");
                _logger.LogError($"Exception occurred in {nameof(NominatimGeocoderService)}.{nameof(GetCoordinates)}. Exception message: {e.Message}");
                _logger.LogError($"Inner Exception message: {e.InnerException}");
                _logger.LogInformation($"Returning null Data.");
            }
            return content;
        }

        /// <summary>
        /// Given a Nominatim API URL, downloads the data from the API.
        /// </summary>
        /// <param name="url">The URL used to access the Nominatim API.</param>
        /// <returns>Returns JSON array in string format from Nominatim's API.</returns>
        private async Task<string> DownloadNominatimApiData(string url)
        {
            // using HttpClient over WebClient as more features supported in HttpClient.
            HttpClient _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("holos-aafc-v5 (Holos@agr.gc.ca)");
            // Download the string content from the Nominatim API
            var content = await _httpClient.GetStringAsync(url);

            if (content != string.Empty)
            {
                _logger.LogInformation($"{nameof(NominatimGeocoderService)}.{nameof(DownloadNominatimApiData)} : API content downloaded successfully.");
            }
            else
            {
                _logger.LogError($"{nameof(NominatimGeocoderService)}.{nameof(DownloadNominatimApiData)}, API content could be not downloaded.");
                _logger.LogError($"{nameof(NominatimGeocoderService)}.{nameof(DownloadNominatimApiData)}, API url: {url}");
            }
            return content;
        }

        /// <summary>
        /// Based off the address, returns the cache path for the address.
        /// </summary>
        /// <param name="street">The street address used in the naming of the cache file</param>
        /// <param name="municipality">The municipality used in the naming of the cache file</param>
        /// <param name="province">The province used in the naming of the cache file</param>
        /// <param name="postalCode">The postal code used in the naming of the cache file</param>
        /// <param name="county">The county used in the naming of the cache file</param>
        /// <param name="country">The country used in the naming of the cache file</param>
        /// <returns>The path based off of the given address.</returns>
        private string GetCachePath(string street, string municipality, Province province, string postalCode, string? county, string country)
        {
            // If county is not null, we prefix it with an underscore to separate it from the municipality in the file name
            string countyStringAppend = string.Empty;
            if (county != null)
            {
                countyStringAppend = $"_{county}";
            }
            var joinedAddress = (street+"_"+municipality+countyStringAppend+"_"+province+"_"+postalCode+"_"+country).ToLower();
            // Sanitize address for file name, replace common address characters with underscores.
            var invalidCharacters = Path.GetInvalidFileNameChars();
            var cleanedFileName = invalidCharacters.Aggregate(joinedAddress, (current, c) => current.Replace(c, '_')).Replace(" ", "_").Replace(",", "");
            var filename = $"nominatim_geocoder_data_address_{cleanedFileName}";

            var path = Path.GetTempPath();
            return Path.Combine(path, filename);
        }

        /// <summary>
        /// Retrieves cached data for the given address if it exists.
        /// </summary>
        /// <param name="street">The street address used in the naming of the cache file to be retrieved</param>
        /// <param name="municipality">The municipality used in the naming of the cache file to be retrieved</param>
        /// <param name="province">The province used in the naming of the cache file to be retrieved</param>
        /// <param name="postalCode">The postal code used in the naming of the cache file to be retrieved</param>
        /// <param name="county">The county used in the naming of the cache file to be retrieved</param>
        /// <param name="country">The country used in the naming of the cache file to be retrieved, defaults to Canada.</param>
        /// <returns>Returns JSON array in string format from a previous Nominatim API call.</returns>
        private string? GetCachedData(string street, string municipality, Province province, string postalCode, string? county, string country)
        {
            var path = GetCachePath(street, municipality, province, postalCode, county, country);
            if (File.Exists(path))
            {
                _logger.LogInformation($"Loading cached Nominatim Geocoder data for address: {street} {municipality}, {province.ToString()}, {postalCode}, {country}, {county}");
                return File.ReadAllText(path);
            }
            return null;
        }

        /// <summary>
        /// Caches the Nominatim API data for the given address.
        /// </summary>
        /// <param name="content">The content received from an api call to be stored in a temp file to cache data for future access.</param>
        /// <param name="street">The street address used in the naming of the cache file to be retrieved</param>
        /// <param name="municipality">The municipality used in the naming of the cache file to be retrieved</param>
        /// <param name="province">The province used in the naming of the cache file to be retrieved</param>
        /// <param name="postalCode">The postal code used in the naming of the cache file to be retrieved</param>
        /// <param name="county">The county used in the naming of the cache file to be retrieved</param>
        /// <param name="country">The country used in the naming of the cache file to be retrieved</param>
        private void CacheData(string content, string street, string municipality, Province province, string postalCode, string? county, string country)
        {
            _logger.LogInformation($"Caching Nominatim Geocoder data for address: {street} {municipality}, {province}, {county}, {postalCode}, {country}");
            var path = GetCachePath(street, municipality, province, postalCode, county, country);
            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Parses the Nominatim API content for latitude and longitude coordinates.
        /// </summary>
        /// <param name="content">The content from the API call to be parsed for longitude and latitude data.</param>
        /// <returns>Two doubles representing the longitude and latitude.</returns>
        private (double latitude, double longitude) ParseNominatimApiContentForCoordinates(string content)
        {
            // Initially read as JArray since Nominatim returns an array of one JSON object.
            JObject? jObject = JArray.Parse(content).FirstOrDefault() as JObject;
            // Access properties from the JObject
            var lat = double.Parse(jObject?["lat"]?.ToString() ?? "0");
            var lon = double.Parse(jObject?["lon"]?.ToString() ?? "0");
            return (latitude: lat, longitude: lon);
        }

        /// <summary>
        /// Validates the input for the geocoding request to ensure it is in a format that can be processed and to prevent malicious input. Street, municipality, postal code, county, and country are all validated with different criteria to allow for the best possible formatting for the Nominatim API and to prevent malicious input.
        /// </summary>
        /// <param name="street">The street address to be validated.</param>
        /// <param name="municipality">The municipality to be validated.</param>
        /// <param name="postalCode">The postal code to be validated.</param>
        /// <param name="county">The county to be validated.</param>
        /// <param name="country">The country to be validated.</param>
        private void InputValidation(string street, string municipality, string postalCode, string? county, string country)
        {
            street = InputValidation(street, InputValidationType.Address);
            municipality = InputValidation(municipality, InputValidationType.Municipality);
            postalCode = InputValidation(postalCode, InputValidationType.PostalCode);
            county = InputValidation(county ?? string.Empty, InputValidationType.County);
            country = InputValidation(country, InputValidationType.Country);
        }

        /// <summary>
        /// Validates the input string based on an <see cref="InputValidationType"/> to validate string based on what type of address component it is
        /// </summary>
        /// <param name="inputString">The address component</param>
        /// <param name="inputType">The type of address component, defaults to general formatting</param>
        /// <returns>Sanitized input string based on parameters defined by <see cref="InputValidationType"/>.</returns>
        private string InputValidation(string inputString, InputValidationType inputType = InputValidationType.General)
        {
            // Basic null/whitespace check
            if (string.IsNullOrWhiteSpace(inputString))
            {
                return string.Empty;
            }

            // Remove potentially dangerous characters and sequences
            inputString = RemoveMaliciousPatterns(inputString);

            // Apply input-type specific validation
            return ApplySpecificValidation(inputString, inputType);
        }

        /// <summary>
        /// Check for blacklisted patterns existing in the string and remove them
        /// </summary>
        /// <param name="input">The string to be checked for malicious patterns</param>
        /// <returns>Input string sanitized of blacklisted patterns</returns>
        private string RemoveMaliciousPatterns(string input)
        {
            // Remove common injection patterns
            var dangerousPatterns = new[]
            {
                @"<script[^>]*>.*?</script>",  // Script tags
                @"javascript:",                // Javascript protocols
                @"vbscript:",                  // VBScript protocols
                @"on\w+\s*=",                 // Event handlers
                @"expression\s*\(",           // CSS expressions
                @"url\s*\(",                  // URL functions
                @"@import",                    // CSS imports
                @"<!--.*?-->",                 // HTML comments
                @"<[^>]*>",                   // All HTML/XML tags
                @"&[#\w]+;",                  // HTML entities
                @"\{\{.*?\}\}",               // Template injection patterns
                @"\$\{.*?\}",                 // Variable substitution
                @"<%.*?%>",                   // Server-side includes
                @"\[\[.*?\]\]",               // Wiki-style markup
                @"' OR '1'='1"                // SQL injection pattern
            };

            foreach (var pattern in dangerousPatterns)
            {
                input = Regex.Replace(input, pattern, "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            }

            // Remove control characters (except common whitespace)
            input = Regex.Replace(input, @"[\x00-\x08\x0B\x0C\x0E-\x1F\x7F]", "");

            // Normalize whitespace
            input = Regex.Replace(input, @"\s+", " ").Trim();

            return input;
        }

        /// <summary>
        /// Based on the type of <see cref="InputValidationType"/> validate the string to allow only certain characters
        /// </summary>
        /// <param name="input">The string to be validated</param>
        /// <param name="inputType">The type of validation to apply to the input string</param>
        /// <returns>Returns string containing only whitelisted characters</returns>
        private string ApplySpecificValidation(string input, InputValidationType inputType)
        {
            switch (inputType)
            {
                case InputValidationType.Address:
                    // Allow alphanumeric, spaces, hyphens, apostrophes, periods, commas, and common address characters
                    return Regex.Replace(input, @"[^a-zA-Z0-9\s\-'.,#/\\()&]", "");

                case InputValidationType.Municipality:
                    // Allow alphanumeric, spaces, hyphens, apostrophes, and periods
                    return Regex.Replace(input, @"[^a-zA-Z0-9\s\-'.]", "");

                case InputValidationType.PostalCode:
                {
                        // Allow alphanumeric and spaces only
                        // Canadian postal code pattern: Letter-Digit-Letter-Digit-Letter-Digit
                        var canadianRegexPattern = @"^[A-Z]\d[A-Z]\d[A-Z]\d$";

                        // Remove all whitespace and convert to uppercase
                        input = input.Replace(" ", "").ToUpperInvariant();
                        // Check if it matches Canadian format
                        if (Regex.IsMatch(input, canadianRegexPattern))
                        {
                            // Format Canadian postal code with space
                            if (input.Length == 6)
                            {
                                return $"{input.Substring(0, 3)} {input.Substring(3, 3)}";
                            }
                        }
                        return Regex.Replace(input, @"[^a-zA-Z0-9\s]", "");
                }

                case InputValidationType.Country:
                    // Allow only letters, spaces, and basic punctuation
                    return Regex.Replace(input, @"[^a-zA-Z\s\-'.]", "");

                case InputValidationType.County:
                    // Similar to municipality but may include "County", "Parish", etc.
                    return Regex.Replace(input, @"[^a-zA-Z0-9\s\-'.]", "");

                case InputValidationType.General:
                default:
                    // Allow only alphanumeric and basic punctuation
                    return Regex.Replace(input, @"[^a-zA-Z0-9\s\-'.,]", "");
            }
        }

        #endregion
    }
}