using H.Core.Factories.Climate;
using H.Core.Models.Climate;
using H.Core.Providers.Climate;
using H.Core.Services.Climate;
using H.Core.Services.Animals;
using Microsoft.Extensions.Logging;
using Moq;

#nullable disable

namespace H.Core.Test.Services.Climate
{
    [TestClass]
    public class ClimateServiceTest
    {
        #region Fields

        private ClimateService _climateService = null!;
        private Mock<IDailyClimateDataFactory> _mockFactory = null!;
        private Mock<ITransferService<DailyClimateData, DailyClimateDto>> _mockTransferService = null!;
        private Mock<ILogger> _mockLogger = null!;
        private Mock<IClimateProvider> _mockClimateProvider = null!;

        #endregion

        #region Initialization

        [TestInitialize]
        public void TestInitialize()
        {
            _mockFactory = new Mock<IDailyClimateDataFactory>();
            _mockTransferService = new Mock<ITransferService<DailyClimateData, DailyClimateDto>>();
            _mockLogger = new Mock<ILogger>();
            _mockClimateProvider = new Mock<IClimateProvider>();

            _climateService = new ClimateService(
                _mockFactory.Object,
                _mockTransferService.Object,
                _mockLogger.Object,
                _mockClimateProvider.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _climateService = null;
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullFactory_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                // Act
                new ClimateService(null, _mockTransferService.Object, _mockLogger.Object, _mockClimateProvider.Object);
            });
        }

        [TestMethod]
        public void Constructor_WithNullTransferService_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                // Act
                new ClimateService(_mockFactory.Object, null, _mockLogger.Object, _mockClimateProvider.Object);
            });
        }

        [TestMethod]
        public void Constructor_WithNullLogger_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                // Act
                new ClimateService(_mockFactory.Object, _mockTransferService.Object, null, _mockClimateProvider.Object);
            });
        }

        [TestMethod]
        public void Constructor_WithNullClimateProvider_ThrowsArgumentNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() =>
            {
                // Act
                new ClimateService(_mockFactory.Object, _mockTransferService.Object, _mockLogger.Object, null);
            });
        }

        [TestMethod]
        public void Constructor_WithAllValidParameters_CreatesInstance()
        {
            // Act
            var service = new ClimateService(_mockFactory.Object, _mockTransferService.Object, _mockLogger.Object, _mockClimateProvider.Object);
            
            // Assert
            Assert.IsNotNull(service);
        }

        #endregion

        #region TransferDailyClimateDataToDto Tests

        [TestMethod]
        public void TransferDailyClimateDataToDto_WithValidData_ReturnsDto()
        {
            // Arrange
            var dailyClimateData = new DailyClimateData { Year = 2023, MeanDailyPET = 5.0, MeanDailyPrecipitation = 2.5 };
            var expectedDto = new DailyClimateDto { Year = 2023, TotalPET = 5.0, TotalPPT = 2.5 };
            
            _mockTransferService.Setup(x => x.TransferDomainObjectToDto(dailyClimateData)).Returns(expectedDto);

            // Act
            var result = _climateService.TransferDailyClimateDataToDto(dailyClimateData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedDto, result);
            _mockTransferService.Verify(x => x.TransferDomainObjectToDto(dailyClimateData), Times.Once);
        }

        [TestMethod]
        public void TransferDailyClimateDataToDto_WithNullData_CallsTransferService()
        {
            // Arrange
            DailyClimateData dailyClimateData = null;
            
            _mockTransferService.Setup(x => x.TransferDomainObjectToDto(null)).Returns((DailyClimateDto)null);

            // Act
            var result = _climateService.TransferDailyClimateDataToDto(dailyClimateData);

            // Assert
            Assert.IsNull(result);
            _mockTransferService.Verify(x => x.TransferDomainObjectToDto(null), Times.Once);
        }

        #endregion

        #region TransferClimateDtoToSystem Tests

        [TestMethod]
        public void TransferClimateDtoToSystem_WithValidData_ReturnsUpdatedData()
        {
            // Arrange
            var dto = new DailyClimateDto { Year = 2023, TotalPET = 5.0, TotalPPT = 2.5 };
            var existingData = new DailyClimateData { Year = 2023, MeanDailyPET = 0, MeanDailyPrecipitation = 0 };
            var expectedData = new DailyClimateData { Year = 2023, MeanDailyPET = 5.0, MeanDailyPrecipitation = 2.5 };
            
            _mockTransferService.Setup(x => x.TransferDtoToDomainObject(dto, existingData)).Returns(expectedData);

            // Act
            var result = _climateService.TransferClimateDtoToSystem(dto, existingData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedData, result);
            _mockTransferService.Verify(x => x.TransferDtoToDomainObject(dto, existingData), Times.Once);
        }

        #endregion

        #region CreateDataFromDto Tests

        [TestMethod]
        public void CreateDataFromDto_WithValidDto_ReturnsNewData()
        {
            // Arrange
            var dto = new DailyClimateDto { Year = 2023, TotalPET = 5.0, TotalPPT = 2.5 };
            var expectedData = new DailyClimateData { Year = 2023, MeanDailyPET = 5.0, MeanDailyPrecipitation = 2.5 };
            
            _mockFactory.Setup(x => x.CreateData(dto)).Returns(expectedData);

            // Act
            var result = _climateService.CreateDataFromDto(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedData, result);
            _mockFactory.Verify(x => x.CreateData(dto), Times.Once);
        }

        [TestMethod]
        public void CreateDataFromDto_WithNullDto_ReturnsNull()
        {
            // Act
            var result = _climateService.CreateDataFromDto(null);

            // Assert
            Assert.IsNull(result);
            _mockFactory.Verify(x => x.CreateData(It.IsAny<DailyClimateDto>()), Times.Never);
        }

        #endregion
    }
}
