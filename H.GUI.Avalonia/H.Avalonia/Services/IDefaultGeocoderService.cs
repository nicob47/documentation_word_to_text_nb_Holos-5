using H.Core.Enumerations;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace H.Avalonia.Services
{
    public interface IDefaultGeocoderService
    {
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
        bool IsCached(string street, string municipality, Province province, string postalCode, string? county = null, string country = "Canada");
        /// <summary>
        /// Returns boolean on if the API request is ready or not due to previous request being made too recently or too many requests made without a successful API call.
        /// </summary>
        /// <returns>True if an API call can be made, false if an API call can't be made.</returns>
        bool IsReadyForRequest();
        /// <summary>
        /// Returns the latitude and longitude for the given address.
        /// </summary>
        /// <param name="street">The street address to geocode and get coordinates for</param>
        /// <param name="municipality">The municipality of the address to geocode and get coordinates for</param>
        /// <param name="province">The province of the address to geocode and get coordinates for</param>
        /// <param name="postalCode">The postal code of the address to geocode and get coordinates for</param>
        /// <param name="county">The county of the address to geocode and get coordinates</param>
        /// <param name="country">The country of the address to geocode and get coordinates for, defaults to Canada.</param>
        /// <returns>Longitude and latitude coordinates</returns>
        Task<(double latitude, double longitude)> GetCoordinates(string street, string municipality, Province province, string postalCode, string? county = null, string country = "Canada");
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
        Task<JObject?> GetApiContent(string street, string municipality, Province province, string postalCode, string? county = null, string country = "Canada");
    }
}
