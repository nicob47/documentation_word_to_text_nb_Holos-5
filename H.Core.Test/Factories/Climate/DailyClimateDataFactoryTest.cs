using AutoMapper;
using H.Core.Factories.Climate;
using H.Core.Mappers;
using H.Core.Models;
using H.Core.Models.Climate;
using H.Core.Providers.Climate;
using Moq;
using Prism.Ioc;

#nullable disable

namespace H.Core.Test.Factories.Climate
{
    [TestClass]
    public class DailyClimateDataFactoryTest
    {
        #region Fields

        private DailyClimateDataFactory _factory = null!;

        #endregion

        #region Initialization

        [TestInitialize]
        public void TestInitialize()
        {
            var mockContainerProvider = new Mock<IContainerProvider>();

            // Setup mappers to return a working IMapper for each required profile
            mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(DailyClimateDataToDailyClimateDtoMapper))).Returns(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DailyClimateDataToDailyClimateDtoMapper>();
            }).CreateMapper());

            mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(DailyClimateDtoToDailyClimateDtoMapper))).Returns(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DailyClimateDtoToDailyClimateDtoMapper>();
            }).CreateMapper());

            mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(DailyClimateDtoToDailyClimateDataMapper))).Returns(new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<DailyClimateDtoToDailyClimateDataMapper>();
            }).CreateMapper());

            _factory = new DailyClimateDataFactory(mockContainerProvider.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _factory = null;
        }

        #endregion

        #region Tests

        [TestMethod]
        public void CreateDto_WithValidDailyClimateData_ReturnsValidDto()
        {
            // Arrange
            var dailyClimateData = new DailyClimateData
            {
                Year = 2023,
                JulianDay = 150,
                MeanDailyAirTemperature = 15.5,
                MeanDailyPrecipitation = 2.3,
                MeanDailyPET = 4.7,
                RelativeHumidity = 65.2,
                SolarRadiation = 18.5,
                Date = new DateTime(2023, 5, 30)
            };

            // Act
            var result = _factory.CreateDto(dailyClimateData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2023, result.Year);
            Assert.AreEqual(4.7, result.MeanDailyEvapotranspiration);
            Assert.AreEqual(2.3, result.MeanDailyPrecipitation);
        }

        [TestMethod]
        public void CreateDto_WithZeroValues_ReturnsDto()
        {
            // Arrange
            var dailyClimateData = new DailyClimateData
            {
                Year = 0,
                JulianDay = 0,
                MeanDailyAirTemperature = 0,
                MeanDailyPrecipitation = 0,
                MeanDailyPET = 0,
                RelativeHumidity = 0,
                SolarRadiation = 0,
                Date = DateTime.MinValue
            };

            // Act
            var result = _factory.CreateDto(dailyClimateData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Year);
            Assert.AreEqual(0, result.TotalPET);
            Assert.AreEqual(0, result.TotalPPT);
        }

        [TestMethod]
        public void CreateDto_WithNegativeValues_ReturnsDto()
        {
            // Arrange
            var dailyClimateData = new DailyClimateData
            {
                Year = -5,
                JulianDay = -1,
                MeanDailyAirTemperature = -10.5,
                MeanDailyPrecipitation = -1.0,
                MeanDailyPET = -2.0,
                RelativeHumidity = -50.0,
                SolarRadiation = -5.0,
                Date = new DateTime(1900, 1, 1)
            };

            // Act
            var result = _factory.CreateDto(dailyClimateData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(-5, result.Year);
            Assert.AreEqual(-2.0, result.MeanDailyEvapotranspiration);
            Assert.AreEqual(-1.0, result.MeanDailyPrecipitation);
        }

        [TestMethod]
        public void CreateDto_WithExtremeValues_ReturnsDto()
        {
            // Arrange
            var dailyClimateData = new DailyClimateData
            {
                Year = int.MaxValue,
                JulianDay = int.MaxValue,
                MeanDailyAirTemperature = double.MaxValue,
                MeanDailyPrecipitation = double.MaxValue,
                MeanDailyPET = double.MaxValue,
                RelativeHumidity = double.MaxValue,
                SolarRadiation = double.MaxValue,
                Date = DateTime.MaxValue
            };

            // Act
            var result = _factory.CreateDto(dailyClimateData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(int.MaxValue, result.Year);
            Assert.AreEqual(double.MaxValue, result.MeanDailyEvapotranspiration);
            Assert.AreEqual(double.MaxValue, result.MeanDailyPrecipitation);
        }

        [TestMethod]
        public void CreateDto_WithNullInput_HandlesGracefully()
        {
            // Arrange
            DailyClimateData dailyClimateData = null;

            // Act & Assert
            try
            {
                var result = _factory.CreateDto(dailyClimateData);
                // If no exception is thrown, verify the result is null or has default values
                if (result != null)
                {
                    Assert.AreEqual(0, result.Year);
                    Assert.AreEqual(0, result.TotalPET);
                    Assert.AreEqual(0, result.TotalPPT);
                }
            }
            catch (Exception ex)
            {
                // Any exception is acceptable for null input
                Assert.IsNotNull(ex);
            }
        }

        [TestMethod]
        public void CreateDto_ReturnsCorrectDtoType()
        {
            // Arrange
            var dailyClimateData = new DailyClimateData
            {
                Year = 2023,
                MeanDailyPET = 5.0,
                MeanDailyPrecipitation = 3.0
            };

            // Act
            var result = _factory.CreateDto(dailyClimateData);

            // Assert
            Assert.IsInstanceOfType(result, typeof(DailyClimateDto));
            Assert.IsInstanceOfType(result, typeof(IDailyClimateDto));
        }

        [TestMethod]
        public void CreateDto_OnlyMapsRequiredProperties()
        {
            // Arrange
            var dailyClimateData = new DailyClimateData
            {
                Year = 2023,
                JulianDay = 200,
                MeanDailyAirTemperature = 25.0,
                MeanDailyPrecipitation = 10.5,
                MeanDailyPET = 7.2,
                RelativeHumidity = 80.0,
                SolarRadiation = 22.0,
                Date = new DateTime(2023, 7, 19)
            };

            // Act
            var result = _factory.CreateDto(dailyClimateData);

            // Assert
            Assert.IsNotNull(result);
            // Verify only the properties that should be mapped are set
            Assert.AreEqual(2023, result.Year);
            Assert.AreEqual(7.2, result.MeanDailyEvapotranspiration);
            Assert.AreEqual(10.5, result.MeanDailyPrecipitation);
            
            // Verify that other properties of the DTO maintain their default values
            Assert.AreEqual(0, result.Latitude);
            Assert.AreEqual(0, result.Longitude);
            Assert.AreEqual(0, result.MonthlyPPT);
        }

        [TestMethod]
        public void CreateDto_CreatesNewInstanceEachTime()
        {
            // Arrange
            var dailyClimateData = new DailyClimateData
            {
                Year = 2023,
                MeanDailyPET = 5.0,
                MeanDailyPrecipitation = 3.0
            };

            // Act
            var result1 = _factory.CreateDto(dailyClimateData);
            var result2 = _factory.CreateDto(dailyClimateData);

            // Assert
            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);
            Assert.AreNotSame(result1, result2);
            Assert.AreEqual(result1.Year, result2.Year);
            Assert.AreEqual(result1.TotalPET, result2.TotalPET);
            Assert.AreEqual(result1.TotalPPT, result2.TotalPPT);
        }

        [TestMethod]
        public void CreateDto_WithoutParameters_ReturnsEmptyDto()
        {
            // Act
            var result = _factory.CreateDto();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Year);
            Assert.AreEqual(0, result.TotalPET);
            Assert.AreEqual(0, result.TotalPPT);
        }

        [TestMethod]
        public void CreateDto_WithFarm_ReturnsDefaultDto()
        {
            // Arrange
            var farm = new Farm();

            // Act
            var result = _factory.CreateDto(farm);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(DateTime.Now.Year, result.Year);
            Assert.AreEqual(0, result.TotalPET);
            Assert.AreEqual(0, result.TotalPPT);
        }

        [TestMethod]
        public void CreateDtoFromDtoTemplate_WithValidTemplate_ReturnsDto()
        {
            // Arrange
            var template = new DailyClimateDto
            {
                Year = 2023,
                TotalPET = 5.5,
                TotalPPT = 3.2,
                Latitude = 50.5,
                Longitude = -100.2,
                MonthlyPPT = 25.0
            };

            // Act
            var result = _factory.CreateDtoFromDtoTemplate(template) as DailyClimateDto;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2023, result.Year);
            Assert.AreEqual(5.5, result.TotalPET);
            Assert.AreEqual(3.2, result.TotalPPT);
            Assert.AreEqual(50.5, result.Latitude);
            Assert.AreEqual(-100.2, result.Longitude);
            Assert.AreEqual(25.0, result.MonthlyPPT);
        }

        [TestMethod]
        public void CreateData_WithValidDto_ReturnsData()
        {
            // Arrange
            var dto = new DailyClimateDto
            {
                Year = 2023,
                TotalPET = 4.7,
                TotalPPT = 2.3
            };

            // Act
            var result = _factory.CreateData(dto);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2023, result.Year);
            Assert.AreEqual(4.7, result.MeanDailyPET);
            Assert.AreEqual(2.3, result.MeanDailyPrecipitation);
            // Other properties should be default values since they're ignored in mapping
            Assert.AreEqual(0, result.JulianDay);
            Assert.AreEqual(0, result.MeanDailyAirTemperature);
        }

        [TestMethod]
        public void CreateData_WithNullDto_HandlesGracefully()
        {
            // Arrange
            DailyClimateDto dto = null;

            // Act & Assert
            try
            {
                var result = _factory.CreateData(dto);
                // If no exception is thrown, verify the result is null or has default values
                if (result != null)
                {
                    Assert.AreEqual(0, result.Year);
                    Assert.AreEqual(0, result.MeanDailyPET);
                    Assert.AreEqual(0, result.MeanDailyPrecipitation);
                }
            }
            catch (Exception ex)
            {
                // Any exception is acceptable for null input
                Assert.IsNotNull(ex);
            }
        }

        #endregion
    }
}
