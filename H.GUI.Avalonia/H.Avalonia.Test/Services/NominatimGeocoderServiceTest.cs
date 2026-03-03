using H.Avalonia.Services;
using H.Core.Enumerations;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json.Linq;
using SharpKml.Dom.Xal;

#nullable disable


namespace H.Avalonia.Test.Services
{
    [TestClass]
    public class NominatimGeocoderServiceTest
    {
        private static NominatimGeocoderService _nominatimGeocoderService = null!;
        private Mock<ILogger> _mockLogger = null!;
        private ILogger _loggerMock = null!;
        private Mock<INotificationManagerService> _mockNotificationManagerService = null!;
        private INotificationManagerService _notificationManagerServiceMock = null!;
        private string _streetAddress = "5403 1 Ave South";
        private string _municipality = "Lethbridge";
        private Province _province = Province.Alberta;
        private string _postalCode = "T1J 4B1";
#pragma warning disable CS0414
        private string _country = "Canada";
#pragma warning restore CS0414

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Clean up cached file for future test runs
            var path = Path.GetTempPath();
            var invalidCharacters = Path.GetInvalidFileNameChars();
            var cleanedFileName = invalidCharacters.Aggregate("5403 1 Ave S, Lethbridge, Alberta, T1J 4B1, Canada", (current, c) => current.Replace(c, '_')).Replace(" ", "_").Replace(",", "");
            var filename = $"nominatim_geocoder_data_address_{cleanedFileName}".ToLower();
            var fullPath = Path.Combine(path, filename);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _mockLogger = new Mock<ILogger>();
            _loggerMock = _mockLogger.Object;

            _mockNotificationManagerService = new Mock<INotificationManagerService>();
            _notificationManagerServiceMock = _mockNotificationManagerService.Object;
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestConstructorValidParameters()
        {
            _nominatimGeocoderService = new NominatimGeocoderService(_loggerMock, _notificationManagerServiceMock);
            Assert.IsNotNull(_nominatimGeocoderService);
        }

        [TestMethod]
        public void TestConstructorNullLogger()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new NominatimGeocoderService(null, _notificationManagerServiceMock));
        }

        [TestMethod]
        public void TestConstructorNullNotificationManager()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new NominatimGeocoderService(_loggerMock, null));
        }

        [TestMethod]
        public async Task TestGeocoderLongLat()
        {
            _nominatimGeocoderService = new NominatimGeocoderService(_loggerMock, _notificationManagerServiceMock);
            var latitudeAndLongitude = await _nominatimGeocoderService.GetCoordinates(_streetAddress, _municipality, _province, _postalCode);
            Assert.AreEqual(latitudeAndLongitude.latitude, 49.697, 0.01);
            Assert.AreEqual(latitudeAndLongitude.longitude, -112.848, 0.01);
        }

        [TestMethod]
        public async Task TestGeocoderGetApiContent()
        {
            _nominatimGeocoderService = new NominatimGeocoderService(_loggerMock, _notificationManagerServiceMock);
            JObject apiContentJObject = await _nominatimGeocoderService.GetApiContent(_streetAddress, _municipality, _province, _postalCode);
            var lat = double.Parse(apiContentJObject["lat"]?.ToString());
            var lon = double.Parse(apiContentJObject["lon"]?.ToString());
            Assert.AreEqual(lat, 49.697, 0.01);
            Assert.AreEqual(lon, -112.848, 0.01);
        }

        [TestMethod]
        public async Task TestCachingOfGeocodedData()
        {
            // Test that after geocoding an address, it is cached
            _nominatimGeocoderService = new NominatimGeocoderService(_loggerMock, _notificationManagerServiceMock);
            var latitudeAndLongitude = await _nominatimGeocoderService.GetCoordinates(_streetAddress, _municipality, _province, _postalCode);
            Assert.IsTrue(_nominatimGeocoderService.IsCached(_streetAddress, _municipality, _province, _postalCode));
        }

        [TestMethod]
        public void TestIsReadyForRequest()
        {
            _nominatimGeocoderService = new NominatimGeocoderService(_loggerMock, _notificationManagerServiceMock);
            Assert.IsTrue(_nominatimGeocoderService.IsReadyForRequest());
        }
    }
}
