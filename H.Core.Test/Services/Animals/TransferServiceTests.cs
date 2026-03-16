using System.ComponentModel;
using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Enumerations;
using H.Core.Factories;
using H.Core.Factories.Crops;
using H.Core.Mappers;
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
        public Guid Guid { get; set; } = Guid.NewGuid();
        public Guid DomainObjectGuid { get; set; }
        public double Weight { get; set; }
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067
    }

    /// <summary>
    /// Simple mapper for tests: TestDto -> TestModel
    /// </summary>
    public class TestDtoToTestModelMapper : IModelMapper<TestDto, TestModel>
    {
        public TestModel Map(TestDto source) => PropertyMapper.Map<TestDto, TestModel>(source);
    }

    /// <summary>
    /// Simple mapper for tests: TestModel -> TestDto
    /// </summary>
    public class TestModelToTestDtoMapper : IModelMapper<TestModel, TestDto>
    {
        public TestDto Map(TestModel source) => PropertyMapper.Map<TestModel, TestDto>(source);
    }

    [TestClass]
    public class TransferServiceTests
    {
        #region Fields

        private Mock<IUnitsOfMeasurementCalculator> _mockUnitsCalculator;
        private Mock<IFactory<CropDto>> _mockCropDtoFactory;
        private Mock<IFactory<TestDto>> _mockTestDtoFactory;
        private IModelMapper<CropViewItem, CropDto> _cropViewItemToCropDtoMapper;
        private IModelMapper<CropDto, CropViewItem> _cropDtoToCropViewItemMapper;
        private IModelMapper<TestModel, TestDto> _testModelToTestDtoMapper;
        private IModelMapper<TestDto, TestModel> _testDtoToTestModelMapper;

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
            _cropViewItemToCropDtoMapper = new CropViewItemToCropDtoMapper();
            _cropDtoToCropViewItemMapper = new CropDtoToCropViewItemMapper();

            // Setup mappers for TestModel <-> TestDto
            _testModelToTestDtoMapper = new TestModelToTestDtoMapper();
            _testDtoToTestModelMapper = new TestDtoToTestModelMapper();
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
        public void TransferToCropDto_MapsPropertiesCorrectly()
        {
            // Arrange
            ITransferService<CropViewItem, CropDto> service = new TransferService<CropViewItem, CropDto>(
                _mockUnitsCalculator.Object,
                _mockCropDtoFactory.Object,
                _cropDtoToCropViewItemMapper,
                _cropViewItemToCropDtoMapper
            );
            var component = new CropViewItem() { Name = "Corn", Year = 1999 };

            // Act
            var dto = service.TransferDomainObjectToDto(component);

            // Assert
            Assert.AreEqual("Corn", dto.Name);
            Assert.AreEqual(1999, dto.Year);
        }

        [TestMethod]
        public void TransferDomainObjectToDto_SetsDomainObjectGuid()
        {
            // Arrange
            ITransferService<TestModel, TestDto> service = new TransferService<TestModel, TestDto>(
                _mockUnitsCalculator.Object,
                _mockTestDtoFactory.Object,
                _testDtoToTestModelMapper,
                _testModelToTestDtoMapper
            );
            var model = new TestModel { Name = "Test", Weight = 100.0 };
            var modelGuid = model.Guid;

            // Act
            var dto = service.TransferDomainObjectToDto(model);

            // Assert
            Assert.AreEqual(modelGuid, dto.DomainObjectGuid, "DomainObjectGuid should be set to the domain model's Guid");
            Assert.AreNotEqual(modelGuid, dto.Guid, "DTO identity Guid should remain its own value");
        }

        [TestMethod]
        public void TransferDomainObjectToDto_DtoGuidIsNotDomainGuid()
        {
            // Arrange
            ITransferService<TestModel, TestDto> service = new TransferService<TestModel, TestDto>(
                _mockUnitsCalculator.Object,
                _mockTestDtoFactory.Object,
                _testDtoToTestModelMapper,
                _testModelToTestDtoMapper
            );
            var model = new TestModel { Name = "Test" };

            // Act
            var dto = service.TransferDomainObjectToDto(model);

            // Assert — DTO gets its own identity, domain correlation is via DomainObjectGuid
            Assert.AreNotEqual(model.Guid, dto.Guid, "DTO should have its own identity Guid, not the domain model's");
            Assert.AreEqual(model.Guid, dto.DomainObjectGuid, "DomainObjectGuid should track the source domain object");
        }

        [TestMethod]
        public void TransferDomainObjectToDto_MultipleDtosFromSameModel_ShareDomainObjectGuid()
        {
            // Arrange
            ITransferService<TestModel, TestDto> service = new TransferService<TestModel, TestDto>(
                _mockUnitsCalculator.Object,
                _mockTestDtoFactory.Object,
                _testDtoToTestModelMapper,
                _testModelToTestDtoMapper
            );
            var model = new TestModel { Name = "Shared" };

            // Act
            _mockTestDtoFactory.Setup(f => f.CreateDto(It.IsAny<Farm>())).Returns(new TestDto());
            var dto1 = service.TransferDomainObjectToDto(model);
            _mockTestDtoFactory.Setup(f => f.CreateDto(It.IsAny<Farm>())).Returns(new TestDto());
            var dto2 = service.TransferDomainObjectToDto(model);

            // Assert — both DTOs point back to the same domain object but have different identity Guids
            Assert.AreEqual(dto1.DomainObjectGuid, dto2.DomainObjectGuid, "Both DTOs should reference the same domain object");
            Assert.AreNotEqual(dto1.Guid, dto2.Guid, "Each DTO should have its own unique identity Guid");
        }

        [TestMethod]
        public void TransferDtoToDomainObject_DoesNotOverwriteModelGuid()
        {
            // Arrange
            _mockTestDtoFactory.Setup(f => f.CreateDtoFromDtoTemplate(It.IsAny<IDto>()))
                .Returns<IDto>(template =>
                {
                    var testTemplate = (TestDto)template;
                    var copy = new TestDto
                    {
                        Name = testTemplate.Name,
                        Weight = testTemplate.Weight,
                        DomainObjectGuid = testTemplate.DomainObjectGuid
                    };
                    return copy;
                });

            ITransferService<TestModel, TestDto> service = new TransferService<TestModel, TestDto>(
                _mockUnitsCalculator.Object,
                _mockTestDtoFactory.Object,
                _testDtoToTestModelMapper,
                _testModelToTestDtoMapper
            );

            var model = new TestModel { Name = "Original", Weight = 50.0 };
            var originalModelGuid = model.Guid;

            var dto = new TestDto { Name = "Updated", Weight = 75.0, DomainObjectGuid = originalModelGuid };

            // Act
            service.TransferDtoToDomainObject(dto, model);

            // Assert — model keeps its own Guid, name is updated via PropertyMapper
            Assert.AreEqual(originalModelGuid, model.Guid, "Domain model Guid should not be overwritten by transfer");
            Assert.AreEqual("Updated", model.Name);
        }

        #endregion
    }
}
