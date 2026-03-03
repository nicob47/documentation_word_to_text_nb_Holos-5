using AutoMapper;
using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Enumerations;
using H.Core.Factories;
using H.Core.Factories.Animals;
using H.Core.Mappers;
using H.Core.Models.Animals;
using H.Core.Services.Animals;
using Microsoft.Extensions.Logging;
using Moq;
using Prism.Ioc;

namespace H.Core.Test.Services.Animals
{
    [TestClass]
    public class ManagementPeriodServiceTests
    {
        #region Fields

        private ManagementPeriodService _service = null!;
        private Mock<IManagementPeriodFactory> _mockManagementPeriodFactory = null!;

        #endregion

        #region Initialization

        [TestInitialize]
        public void TestInitialize()
        {
            _mockManagementPeriodFactory = new Mock<IManagementPeriodFactory>();
            var mockLogger = new Mock<ILogger>();
            var mockContainerProvider = new Mock<IContainerProvider>();
            var mockUnitsOfMeasurementCalculator = new Mock<IUnitsOfMeasurementCalculator>();

            mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(ManagementPeriodDtoToManagementPeriodMapper)))
                .Returns(new MapperConfiguration(cfg => cfg.AddProfile<ManagementPeriodDtoToManagementPeriodMapper>()).CreateMapper());

            mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(ManagementPeriodToManagementPeriodDtoMapper)))
                .Returns(new MapperConfiguration(cfg => cfg.AddProfile<ManagementPeriodToManagementPeriodDtoMapper>()).CreateMapper());

            _service = new ManagementPeriodService(mockLogger.Object, mockContainerProvider.Object, 
                _mockManagementPeriodFactory.Object, mockUnitsOfMeasurementCalculator.Object);
        }

        #endregion

        [TestMethod]
        public void GetValidBeddingMaterialTypes_BeefProduction_ReturnsExpectedTypes()
        {
            var result = _service.GetValidBeddingMaterialTypes(AnimalType.Beef);

            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.None);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.WoodChip);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.Straw);
        }

        [TestMethod]
        public void GetValidBeddingMaterialTypes_Dairy_ReturnsExpectedTypes()
        {
            var result = _service.GetValidBeddingMaterialTypes(AnimalType.Dairy);

            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.None);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.Sand);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.SeparatedManureSolid);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.StrawLong);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.StrawChopped);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.Shavings);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.Sawdust);
        }

        [TestMethod]
        public void GetValidBeddingMaterialTypes_Swine_ReturnsExpectedTypes()
        {
            var result = _service.GetValidBeddingMaterialTypes(AnimalType.Swine);

            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.None);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.StrawLong);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.StrawChopped);
        }

        [TestMethod]
        public void GetValidBeddingMaterialTypes_Sheep_ReturnsExpectedTypes()
        {
            var result = _service.GetValidBeddingMaterialTypes(AnimalType.Sheep);

            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.None);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.Straw);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.Shavings);
        }

        [TestMethod]
        public void GetValidBeddingMaterialTypes_Poultry_ReturnsExpectedTypes()
        {
            var result = _service.GetValidBeddingMaterialTypes(AnimalType.Poultry);

            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.None);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.Straw);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.Shavings);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.Sawdust);
        }

        [TestMethod]
        public void GetValidBeddingMaterialTypes_OtherLivestock_ReturnsExpectedTypes()
        {
            var result = _service.GetValidBeddingMaterialTypes(AnimalType.OtherLivestock);

            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.None);
            CollectionAssert.Contains((List<BeddingMaterialType>)result, BeddingMaterialType.Straw);
        }

        [TestMethod]
        public void TransferToManagementPeriodDto_ReturnsDto_WhenGivenValidManagementPeriod()
        {
            // Arrange
            var managementPeriod = new ManagementPeriod
            {
                Start = new DateTime(2023, 1, 1),
                End = new DateTime(2023, 12, 31),
                NumberOfDays = 365,
                StartWeight = 100.0,
                EndWeight = 150.0,
                MilkProduction = 25.0
            };

            // Act
            var result = _service.TransferToManagementPeriodDto(managementPeriod);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(managementPeriod.Start, result.Start);
            Assert.AreEqual(managementPeriod.End, result.End);
            Assert.AreEqual(managementPeriod.NumberOfDays, result.NumberOfDays);
            Assert.AreEqual(managementPeriod.StartWeight, result.StartWeight);
            Assert.AreEqual(managementPeriod.EndWeight, result.EndWeight);
            Assert.AreEqual(managementPeriod.MilkProduction, result.MilkProduction);
        }

        [TestMethod]
        public void TransferManagementPeriodDtoToSystem_ReturnsDto_WhenGivenValidDto()
        {
            // Arrange
            var dto = new ManagementPeriodDto
            {
                Start = new DateTime(2023, 1, 1),
                End = new DateTime(2023, 12, 31),
                NumberOfDays = 365,
                StartWeight = 100.0,
                EndWeight = 150.0,
                MilkProduction = 25.0
            };
            var managementPeriod = new ManagementPeriod();

            _mockManagementPeriodFactory.Setup(x => x.CreateDtoFromDtoTemplate(It.IsAny<IManagementPeriodDto>()))
                .Returns(dto);

            // Act
            var result = _service.TransferManagementPeriodDtoToSystem(dto, managementPeriod);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(dto.Start, result.Start);
            Assert.AreEqual(dto.End, result.End);
            Assert.AreEqual(dto.NumberOfDays, result.NumberOfDays);
        }

        [TestMethod]
        public void ManagementPeriodDto_HasUnitsAttributes_OnPropertiesWithMeasurements()
        {
            // Arrange
            var dto = new ManagementPeriodDto();
            var propertyConverter = new H.Core.Converters.PropertyConverter<IManagementPeriodDto>(dto);

            // Act & Assert
            Assert.IsNotNull(propertyConverter.PropertyInfos, "PropertyInfos should not be null");
            Assert.IsTrue(propertyConverter.PropertyInfos.Count > 0, "Should find properties with Units attributes");

            // Verify specific properties exist with Units attributes
            var startWeightProperty = propertyConverter.PropertyInfos.FirstOrDefault(p => p.Name == nameof(IManagementPeriodDto.StartWeight));
            Assert.IsNotNull(startWeightProperty, "StartWeight should have Units attribute");

            var milkProductionProperty = propertyConverter.PropertyInfos.FirstOrDefault(p => p.Name == nameof(IManagementPeriodDto.MilkProduction));
            Assert.IsNotNull(milkProductionProperty, "MilkProduction should have Units attribute");
        }
    }
}
