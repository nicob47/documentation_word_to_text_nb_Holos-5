using System.ComponentModel;
using AutoMapper;
using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Enumerations;
using H.Core.Factories;
using H.Core.Factories.Crops;
using H.Core.Models;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services.Animals;
using H.Infrastructure;
using Moq;

#nullable disable

namespace H.Core.Test.Services.Animals
{
    public class TestModel : ModelBase 
    {
        public double Weight { get; set; }
    }

    public class TestDto : IDto
    {
        public string Name { get; set; } = string.Empty;
        public Guid Guid { get; set; }
        public double Weight { get; set; }
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
    }

    [TestClass]
    public class TransferServiceTests
    {
        #region Fields

        private Mock<IUnitsOfMeasurementCalculator> _mockUnitsCalculator;
        private Mock<IFactory<CropDto>> _mockCropDtoFactory;
        private Mock<IFactory<TestDto>> _mockTestDtoFactory;
        private IMapper _cropViewItemToCropDtoMapper;
        private IMapper _cropDtoToCropViewItemMapper;
        private IMapper _testModelToTestDtoMapper;
        private IMapper _testDtoToTestModelMapper;

        #endregion

        #region Initialization

        [TestInitialize]
        public void Setup()
        {
            _mockUnitsCalculator = new Mock<IUnitsOfMeasurementCalculator>();
            _mockUnitsCalculator.Setup(x => x.GetUnitsOfMeasurement())
                .Returns(MeasurementSystemType.Metric);

            _mockCropDtoFactory = new Mock<IFactory<CropDto>>();
            _mockCropDtoFactory.Setup(f => f.CreateDto(It.IsAny<Farm>())).Returns(new CropDto());

            _mockTestDtoFactory = new Mock<IFactory<TestDto>>();
            _mockTestDtoFactory.Setup(f => f.CreateDto(It.IsAny<Farm>())).Returns(new TestDto());

            // Setup mappers for CropViewItem <-> CropDto
            var cropViewItemToCropDtoConfig = new MapperConfiguration(cfg => cfg.CreateMap<CropViewItem, CropDto>());
            var cropDtoToCropViewItemConfig = new MapperConfiguration(cfg => cfg.CreateMap<CropDto, CropViewItem>());
            _cropViewItemToCropDtoMapper = cropViewItemToCropDtoConfig.CreateMapper();
            _cropDtoToCropViewItemMapper = cropDtoToCropViewItemConfig.CreateMapper();

            // Setup mappers for TestModel <-> TestDto
            var testModelToTestDtoConfig = new MapperConfiguration(cfg => cfg.CreateMap<TestModel, TestDto>());
            var testDtoToTestModelConfig = new MapperConfiguration(cfg => cfg.CreateMap<TestDto, TestModel>());
            _testModelToTestDtoMapper = testModelToTestDtoConfig.CreateMapper();
            _testDtoToTestModelMapper = testDtoToTestModelConfig.CreateMapper();
        }

        #endregion

        #region Tests

        [TestMethod]
        public void TransferCropDtoToSystemUsingMetricSetsCorrectValue()
        {
            // Display units are metric
            _mockUnitsCalculator.Setup(x => x.GetUnitsOfMeasurement()).Returns(MeasurementSystemType.Metric);
            ITransferService<CropViewItem, CropDto> service = new TransferService<CropViewItem, CropDto>(
                _mockUnitsCalculator.Object,
                _mockCropDtoFactory.Object,
                _cropDtoToCropViewItemMapper,
                _cropViewItemToCropDtoMapper
            );

            var cropViewItem = new CropViewItem()
            {
                AmountOfIrrigation = 100,
            };

            var result = service.TransferDomainObjectToDto(cropViewItem);

            Assert.AreEqual(100, result.AmountOfIrrigation);
        }

        [TestMethod]
        public void TransferCropDtoToSystemUsingImperialSetsCorrectValue()
        {
            // Display units are imperial
            _mockUnitsCalculator.Setup(x => x.GetUnitsOfMeasurement()).Returns(MeasurementSystemType.Imperial);
            ITransferService<CropViewItem, CropDto> service = new TransferService<CropViewItem, CropDto>(
                _mockUnitsCalculator.Object,
                _mockCropDtoFactory.Object,
                _cropDtoToCropViewItemMapper,
                _cropViewItemToCropDtoMapper
            );

            var cropViewItem = new CropViewItem()
            {
                AmountOfIrrigation = 100,
            };

            var result = service.TransferDomainObjectToDto(cropViewItem);

            // Convert 100 millimeters to inches
            var expected = 100 / 25.4;

            Assert.AreEqual(expected, result.AmountOfIrrigation, 0.01);
        }

        [TestMethod]
        public void TransferToAnimalComponentDto_MapsPropertiesCorrectly()
        {
            // Arrange
            ITransferService<TestModel, TestDto> service = new TransferService<TestModel, TestDto>(
                _mockUnitsCalculator.Object,
                _mockTestDtoFactory.Object,
                _testDtoToTestModelMapper,
                _testModelToTestDtoMapper
            );
            var component = new TestModel { Name = "Cow", Weight = 500.0 };

            // Act
            var dto = service.TransferDomainObjectToDto(component);

            // Assert
            Assert.IsNotNull(dto);
            Assert.AreEqual("Cow", dto.Name);
            Assert.AreEqual(500.0, dto.Weight);
        }

        [TestMethod]
        public void TransferToAnimalComponentDto_HandlesNullProperties()
        {
            // Arrange
            ITransferService<TestModel, TestDto> service = new TransferService<TestModel, TestDto>(
                _mockUnitsCalculator.Object,
                _mockTestDtoFactory.Object,
                _testDtoToTestModelMapper,
                _testModelToTestDtoMapper
            );
            var component = new TestModel { Name = null, Weight = 0.0 };

            // Act
            var dto = service.TransferDomainObjectToDto(component);

            // Assert
            Assert.IsNull(dto.Name);
            Assert.AreEqual(0.0, dto.Weight);
        }

        [TestMethod]
        public void TransferToCropDto_UsesAdditionalConfig()
        {
            // Arrange
            var config = new MapperConfiguration(cfg =>
                cfg.CreateMap<CropViewItem, CropDto>()
                    .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name + "_Mapped"))
            );
            var modelToDtoMapper = config.CreateMapper();

            // For this test, the other mapper can be a default one
            var defaultConfig = new MapperConfiguration(cfg => cfg.CreateMap<CropDto, CropViewItem>());
            var dtoToModelMapper = defaultConfig.CreateMapper();

            ITransferService<CropViewItem, CropDto> service = new TransferService<CropViewItem, CropDto>(
                _mockUnitsCalculator.Object,
                _mockCropDtoFactory.Object,
                dtoToModelMapper,
                modelToDtoMapper
            );
            var component = new CropViewItem() { Name = "Corn", Year = 1999 };

            // Act
            var dto = service.TransferDomainObjectToDto(component);

            // Assert
            Assert.AreEqual("Corn_Mapped", dto.Name);
            Assert.AreEqual(1999, dto.Year);
        } 

        #endregion
    }
}
