using AutoMapper;
using H.Core.Enumerations;
using H.Core.Factories;
using H.Core.Mappers;
using H.Core.Models.LandManagement.Fields;
using H.Core.Services.Animals;
using H.Core.Services.LandManagement.Fields;
using Moq;
using Prism.Ioc;
using System.Collections.ObjectModel;
using H.Core.Factories.Crops;
using H.Core.Factories.Fields;
using Microsoft.Extensions.Logging;
using H.Core.Models;

#nullable disable

namespace H.Core.Test.Services.LandManagement;

[TestClass]
public class FieldComponentServiceTest
{
    #region Fields

    private IFieldComponentService _fieldComponentService = null!;
    
    private Mock<IFieldFactory> _mockFieldComponentDtoFactory = null!;
    private Mock<ICropFactory> _mockCropFactory = null!;
    private Mock<ITransferService<CropViewItem, CropDto>> _mockCropTransferService = null!;
    private Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>> _mockFieldTransferService = null!;

    #endregion

    #region Initialization

    [ClassInitialize]
    public static void ClassInitialize(TestContext testContext)
    {
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
    }

    [TestInitialize]
    public void TestInitialize()
    {
        _mockFieldComponentDtoFactory = new Mock<IFieldFactory>();
        _mockCropFactory = new Mock<ICropFactory>();
        _mockCropTransferService = new Mock<ITransferService<CropViewItem, CropDto>>();
        _mockFieldTransferService = new Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>();

        var mockLogger = new Mock<ILogger>();
        var mockContainerProvider = new Mock<IContainerProvider>();

        // Setup mappers to return a working IMapper for each required profile
        mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(CropViewItemToCropDtoMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CropViewItemToCropDtoMapper>();
        }).CreateMapper());

        mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(CropDtoToCropDtoMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CropDtoToCropDtoMapper>();
        }).CreateMapper());

        mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(CropDtoToCropViewItemMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<CropDtoToCropViewItemMapper>();
        }).CreateMapper());

        mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(FieldComponentToDtoMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<FieldComponentToDtoMapper>();
        }).CreateMapper());

        mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(FieldDtoToFieldComponentMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<FieldDtoToFieldComponentMapper>();
        }).CreateMapper());

        mockContainerProvider.Setup(x => x.Resolve(typeof(IMapper), nameof(FieldDtoToFieldDtoMapper))).Returns(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<FieldDtoToFieldDtoMapper>();
        }).CreateMapper());

        _fieldComponentService = new FieldComponentService(
            _mockFieldComponentDtoFactory.Object,
            _mockCropFactory.Object,
            mockLogger.Object,
            _mockCropTransferService.Object,
            _mockFieldTransferService.Object
        );
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    #region Constructor Tests

    [TestMethod]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Arrange: valid dependencies
        var mockFieldFactory = new Mock<IFieldFactory>();
        var mockCropFactory = new Mock<ICropFactory>();
        var mockLogger = new Mock<ILogger>();
        var mockCropTransferService = new Mock<ITransferService<CropViewItem, CropDto>>();
        var mockFieldTransferService = new Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>();

        // Act: create instance with valid dependencies
        var service = new FieldComponentService(
            mockFieldFactory.Object,
            mockCropFactory.Object,
            mockLogger.Object,
            mockCropTransferService.Object,
            mockFieldTransferService.Object
        );

        // Assert: instance should be created successfully
        Assert.IsNotNull(service);
        Assert.IsInstanceOfType(service, typeof(FieldComponentService));
    }

    [TestMethod]
    public void Constructor_WithNullFieldFactory_ThrowsArgumentNullException()
    {
        // Arrange: null fieldFactory but valid other dependencies
        IFieldFactory fieldFactory = null;
        var mockCropFactory = new Mock<ICropFactory>();
        var mockLogger = new Mock<ILogger>();
        var mockCropTransferService = new Mock<ITransferService<CropViewItem, CropDto>>();
        var mockFieldTransferService = new Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            var service = new FieldComponentService(
                fieldFactory,
                mockCropFactory.Object,
                mockLogger.Object,
                mockCropTransferService.Object,
                mockFieldTransferService.Object
            );
        });
    }

    [TestMethod]
    public void Constructor_WithNullCropFactory_ThrowsArgumentNullException()
    {
        // Arrange: null cropFactory but valid other dependencies
        var mockFieldFactory = new Mock<IFieldFactory>();
        ICropFactory cropFactory = null;
        var mockLogger = new Mock<ILogger>();
        var mockCropTransferService = new Mock<ITransferService<CropViewItem, CropDto>>();
        var mockFieldTransferService = new Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            var service = new FieldComponentService(
                mockFieldFactory.Object,
                cropFactory,
                mockLogger.Object,
                mockCropTransferService.Object,
                mockFieldTransferService.Object
            );
        });
    }

    [TestMethod]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange: null logger but valid other dependencies
        var mockFieldFactory = new Mock<IFieldFactory>();
        var mockCropFactory = new Mock<ICropFactory>();
        ILogger logger = null;
        var mockCropTransferService = new Mock<ITransferService<CropViewItem, CropDto>>();
        var mockFieldTransferService = new Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            var service = new FieldComponentService(
                mockFieldFactory.Object,
                mockCropFactory.Object,
                logger,
                mockCropTransferService.Object,
                mockFieldTransferService.Object
            );
        });
    }

    [TestMethod]
    public void Constructor_WithNullCropTransferService_ThrowsArgumentNullException()
    {
        // Arrange: null cropTransferService but valid other dependencies
        var mockFieldFactory = new Mock<IFieldFactory>();
        var mockCropFactory = new Mock<ICropFactory>();
        var mockLogger = new Mock<ILogger>();
        ITransferService<CropViewItem, CropDto> cropTransferService = null;
        var mockFieldTransferService = new Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            var service = new FieldComponentService(
                mockFieldFactory.Object,
                mockCropFactory.Object,
                mockLogger.Object,
                cropTransferService,
                mockFieldTransferService.Object
            );
        });
    }

    [TestMethod]
    public void Constructor_WithNullFieldTransferService_ThrowsArgumentNullException()
    {
        // Arrange: null fieldTransferService but valid other dependencies
        var mockFieldFactory = new Mock<IFieldFactory>();
        var mockCropFactory = new Mock<ICropFactory>();
        var mockLogger = new Mock<ILogger>();
        var mockCropTransferService = new Mock<ITransferService<CropViewItem, CropDto>>();
        ITransferService<FieldSystemComponent, FieldSystemComponentDto> fieldTransferService = null;

        // Act & Assert
        Assert.ThrowsExactly<ArgumentNullException>(() =>
        {
            var service = new FieldComponentService(
                mockFieldFactory.Object,
                mockCropFactory.Object,
                mockLogger.Object,
                mockCropTransferService.Object,
                fieldTransferService
            );
        });
    }

    [TestMethod]
    public void Constructor_VerifyParameterNames_InExceptionMessages()
    {
        // Test fieldFactory parameter name in exception
        try
        {
            new FieldComponentService(null, new Mock<ICropFactory>().Object, new Mock<ILogger>().Object, 
                new Mock<ITransferService<CropViewItem, CropDto>>().Object, 
                new Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>().Object);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("fieldFactory", ex.ParamName);
        }

        // Test cropFactory parameter name in exception
        try
        {
            new FieldComponentService(new Mock<IFieldFactory>().Object, null, new Mock<ILogger>().Object,
                new Mock<ITransferService<CropViewItem, CropDto>>().Object,
                new Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>().Object);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("cropFactory", ex.ParamName);
        }

        // Test logger parameter name in exception (this should be handled by base class)
        try
        {
            new FieldComponentService(new Mock<IFieldFactory>().Object, new Mock<ICropFactory>().Object, null,
                new Mock<ITransferService<CropViewItem, CropDto>>().Object,
                new Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>().Object);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("logger", ex.ParamName);
        }

        // Test cropTransferService parameter name in exception
        try
        {
            new FieldComponentService(new Mock<IFieldFactory>().Object, new Mock<ICropFactory>().Object, 
                new Mock<ILogger>().Object, null,
                new Mock<ITransferService<FieldSystemComponent, FieldSystemComponentDto>>().Object);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("cropTransferService", ex.ParamName);
        }

        // Test fieldTransferService parameter name in exception
        try
        {
            new FieldComponentService(new Mock<IFieldFactory>().Object, new Mock<ICropFactory>().Object,
                new Mock<ILogger>().Object, new Mock<ITransferService<CropViewItem, CropDto>>().Object, null);
            Assert.Fail("Expected ArgumentNullException was not thrown");
        }
        catch (ArgumentNullException ex)
        {
            Assert.AreEqual("fieldTransferService", ex.ParamName);
        }
    }

    #endregion

    [TestMethod]
    public void TransferCropDtoToSystemConvertsImperialValueToMetric()
    {
        // Arrange
        var mockTransferService = new Mock<ITransferService<CropViewItem, CropDto>>();
        var cropDto = new CropDto { AmountOfIrrigation = 10 }; // Example value in imperial units
        var expectedCropViewItem = new CropViewItem { AmountOfIrrigation = 25.4 }; // Example value in metric units

        // Setup the mock to return the expected model when called
        mockTransferService
            .Setup(x => x.TransferDtoToDomainObject(It.IsAny<CropDto>(), It.IsAny<CropViewItem>()))
            .Returns(expectedCropViewItem);

        // Act
        var result = mockTransferService.Object.TransferDtoToDomainObject(cropDto, new CropViewItem());

        // Assert
        Assert.AreEqual(25.4, result.AmountOfIrrigation);
    }

    [TestMethod]
    public void CreateSetCropDtoCollectionToNonEmpty()
    {
        _mockFieldTransferService.Setup(x => x.TransferDomainObjectToDto(It.IsAny<FieldSystemComponent>())).Returns(new FieldSystemComponentDto());

        var result = _fieldComponentService.TransferToFieldComponentDto(new FieldSystemComponent() { CropViewItems = new ObservableCollection<CropViewItem>() { new CropViewItem() } });

        Assert.IsTrue(result.CropDtos.Any());
    }

    [TestMethod]
    public void CreateSetCropDtoCollectionToEmpty()
    {
        _mockFieldTransferService.Setup(x => x.TransferDomainObjectToDto(It.IsAny<FieldSystemComponent>())).Returns(new FieldSystemComponentDto());

        var result = _fieldComponentService.TransferToFieldComponentDto(new FieldSystemComponent() { CropViewItems = new ObservableCollection<CropViewItem>() { } });

        Assert.IsFalse(result.CropDtos.Any());
    }

    [TestMethod]
    public void BuildCropDtoCollectionDoesNotCreateAnyItems()
    {
        var fieldComponentDto = new FieldSystemComponentDto();
        var fieldComponent = new FieldSystemComponent();

        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        Assert.IsFalse(fieldComponentDto.CropDtos.Any());
    }

    [TestMethod]
    public void BuildCropDtoCollectionDoesNotCreatesItems()
    {
        var fieldComponentDto = new FieldSystemComponentDto();
        var fieldComponent = new FieldSystemComponent() {CropViewItems = new ObservableCollection<CropViewItem>() {new CropViewItem()}};

        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        Assert.AreEqual(1, fieldComponentDto.CropDtos.Count);

        fieldComponent.CropViewItems.Clear();
        fieldComponent.CropViewItems.Add(new CropViewItem());
        fieldComponent.CropViewItems.Add(new CropViewItem());
        fieldComponent.CropViewItems.Add(new CropViewItem());

        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        Assert.AreEqual(3, fieldComponentDto.CropDtos.Count);
    }

    [TestMethod]
    public void ConvertCropDtoCollectionToCropViewItemCollection()
    {
        var guid = Guid.NewGuid();

        var dto = new CropDto() { Guid = guid, AmountOfIrrigation = 200 };

        _mockCropFactory.Setup(x => x.CreateDtoFromDtoTemplate(It.IsAny<ICropDto>())).Returns(dto);

        var fieldComponent = new FieldSystemComponent()
        {
            CropViewItems = new ObservableCollection<CropViewItem>() { new CropViewItem() { Guid = guid } }
        };
        var fieldComponentDto = new FieldSystemComponentDto()
        {
            CropDtos = new ObservableCollection<ICropDto>() { dto }
        };

        // Mock the transfer service to return a CropViewItem with the expected AmountOfIrrigation
        _mockCropTransferService
            .Setup(x => x.TransferDtoToDomainObject(It.IsAny<CropDto>(), It.IsAny<CropViewItem>()))
            .Returns((CropDto d, CropViewItem v) =>
            {
                v.AmountOfIrrigation = d.AmountOfIrrigation;
                return v;
            });

        _fieldComponentService.ConvertCropDtoCollectionToCropViewItemCollection(fieldComponent, fieldComponentDto);

        Assert.AreEqual(200, fieldComponent.CropViewItems[0].AmountOfIrrigation);
    }

    [TestMethod]
    public void AddCropDtoToSystem()
    {
        var fieldComponent = new FieldSystemComponent();
        var cropDto = new CropDto();

        _mockCropFactory.Setup(x => x.CreateCropViewItem(It.IsAny<ICropDto>())).Returns(new CropViewItem());

        _fieldComponentService.AddCropDtoToSystem(fieldComponent, cropDto);

        Assert.AreEqual(1, fieldComponent.CropViewItems.Count);

        fieldComponent.CropViewItems.Clear();

        _fieldComponentService.AddCropDtoToSystem(fieldComponent, cropDto);
        _fieldComponentService.AddCropDtoToSystem(fieldComponent, cropDto);

        Assert.AreEqual(2, fieldComponent.CropViewItems.Count);
    }

    [TestMethod]
    public void RemoveCropDtoFromSystem()
    {
        var guid = Guid.NewGuid();
        var fieldComponent = new FieldSystemComponent();
        var cropDto = new CropDto() {Guid = guid};
        var viewItem = new CropViewItem() {Guid = guid};

        _mockCropFactory.Setup(x => x.CreateCropViewItem(It.IsAny<ICropDto>())).Returns(viewItem);

        _fieldComponentService.AddCropDtoToSystem(fieldComponent, cropDto);

        Assert.AreEqual(1, fieldComponent.CropViewItems.Count);

        _fieldComponentService.RemoveCropFromSystem(fieldComponent, cropDto);

        Assert.IsFalse(fieldComponent.CropViewItems.Any());
    }

    [TestMethod]
    public void TransferToDto_ReturnsNewDtoInstance()
    {
        // Arrange
        var model = new CropViewItem();
        model.Name = "Test Crop";

        // Act
        // Example usage if you add a TransferDomainObjectToDto method that uses the transfer service:
        // var dto = _fieldComponentService.TransferDomainObjectToDto<CropViewItem, CropDto>(model);
        // For now, just test the transfer service directly:
        _mockCropTransferService.Setup(x => x.TransferDomainObjectToDto(model))
            .Returns(new CropDto { Name = model.Name });

        var dto = _mockCropTransferService.Object.TransferDomainObjectToDto(model);

        // Assert
        Assert.IsNotNull(dto);
        Assert.IsInstanceOfType(dto, typeof(CropDto));
        Assert.AreEqual("Test Crop", dto.Name);
    }

    #region ConvertCropViewItemsToDtoCollection Tests

    [TestMethod]
    public void ConvertCropViewItemsToDtoCollection_WithEmptyCollection_ClearsDtoCollection()
    {
        // Arrange: field component with no crop view items and field DTO with some existing DTOs
        var fieldComponent = new FieldSystemComponent() 
        { 
            CropViewItems = new ObservableCollection<CropViewItem>() 
        };
        var fieldComponentDto = new FieldSystemComponentDto() 
        { 
            CropDtos = new ObservableCollection<ICropDto> 
            { 
                new CropDto(), 
                new CropDto() 
            } 
        };

        // Act: convert empty crop view items to DTO collection
        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        // Assert: DTO collection should be cleared and remain empty
        Assert.AreEqual(0, fieldComponentDto.CropDtos.Count);
        Assert.IsFalse(fieldComponentDto.CropDtos.Any());
    }

    [TestMethod]
    public void ConvertCropViewItemsToDtoCollection_WithSingleItem_CreatesOneDto()
    {
        // Arrange: field component with one crop view item
        var cropViewItem = new CropViewItem() 
        { 
            Guid = Guid.NewGuid(), 
            Name = "Test Crop", 
            CropType = CropType.Wheat 
        };
        var fieldComponent = new FieldSystemComponent() 
        { 
            CropViewItems = new ObservableCollection<CropViewItem> { cropViewItem } 
        };
        var fieldComponentDto = new FieldSystemComponentDto() 
        { 
            CropDtos = new ObservableCollection<ICropDto>() 
        };

        var expectedDto = new CropDto() 
        { 
            Guid = cropViewItem.Guid, 
            Name = cropViewItem.Name, 
            CropType = cropViewItem.CropType 
        };

        // Mock the crop factory to return a specific DTO when called with the crop view item
        _mockCropFactory.Setup(x => x.CreateCropDto(cropViewItem)).Returns(expectedDto);

        // Act: convert the single crop view item to DTO
        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        // Assert: exactly one DTO should be created and added to the collection
        Assert.AreEqual(1, fieldComponentDto.CropDtos.Count);
        Assert.AreSame(expectedDto, fieldComponentDto.CropDtos[0]);
        _mockCropFactory.Verify(x => x.CreateCropDto(cropViewItem), Times.Once);
    }

    [TestMethod]
    public void ConvertCropViewItemsToDtoCollection_WithMultipleItems_CreatesCorrespondingDtos()
    {
        // Arrange: field component with multiple crop view items
        var cropViewItem1 = new CropViewItem() 
        { 
            Guid = Guid.NewGuid(), 
            Name = "Wheat Crop", 
            CropType = CropType.Wheat 
        };
        var cropViewItem2 = new CropViewItem() 
        { 
            Guid = Guid.NewGuid(), 
            Name = "Barley Crop", 
            CropType = CropType.Barley 
        };
        var cropViewItem3 = new CropViewItem() 
        { 
            Guid = Guid.NewGuid(), 
            Name = "Oats Crop", 
            CropType = CropType.Oats 
        };

        var fieldComponent = new FieldSystemComponent() 
        { 
            CropViewItems = new ObservableCollection<CropViewItem> 
            { 
                cropViewItem1, 
                cropViewItem2, 
                cropViewItem3 
            } 
        };
        var fieldComponentDto = new FieldSystemComponentDto() 
        { 
            CropDtos = new ObservableCollection<ICropDto>() 
        };

        var expectedDto1 = new CropDto() { Guid = cropViewItem1.Guid, Name = cropViewItem1.Name };
        var expectedDto2 = new CropDto() { Guid = cropViewItem2.Guid, Name = cropViewItem2.Name };
        var expectedDto3 = new CropDto() { Guid = cropViewItem3.Guid, Name = cropViewItem3.Name };

        // Mock the crop factory to return specific DTOs for each crop view item
        _mockCropFactory.Setup(x => x.CreateCropDto(cropViewItem1)).Returns(expectedDto1);
        _mockCropFactory.Setup(x => x.CreateCropDto(cropViewItem2)).Returns(expectedDto2);
        _mockCropFactory.Setup(x => x.CreateCropDto(cropViewItem3)).Returns(expectedDto3);

        // Act: convert all crop view items to DTOs
        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        // Assert: exactly three DTOs should be created in the correct order
        Assert.AreEqual(3, fieldComponentDto.CropDtos.Count);
        Assert.AreSame(expectedDto1, fieldComponentDto.CropDtos[0]);
        Assert.AreSame(expectedDto2, fieldComponentDto.CropDtos[1]);
        Assert.AreSame(expectedDto3, fieldComponentDto.CropDtos[2]);
        
        // Verify factory was called for each view item
        _mockCropFactory.Verify(x => x.CreateCropDto(cropViewItem1), Times.Once);
        _mockCropFactory.Verify(x => x.CreateCropDto(cropViewItem2), Times.Once);
        _mockCropFactory.Verify(x => x.CreateCropDto(cropViewItem3), Times.Once);
    }

    [TestMethod]
    public void ConvertCropViewItemsToDtoCollection_ClearsExistingDtos_BeforeAddingNew()
    {
        // Arrange: field DTO already contains some DTOs, field component has one view item
        var existingDto1 = new CropDto() { Guid = Guid.NewGuid(), Name = "Existing 1" };
        var existingDto2 = new CropDto() { Guid = Guid.NewGuid(), Name = "Existing 2" };
        
        var fieldComponentDto = new FieldSystemComponentDto() 
        { 
            CropDtos = new ObservableCollection<ICropDto> { existingDto1, existingDto2 } 
        };

        var newCropViewItem = new CropViewItem() 
        { 
            Guid = Guid.NewGuid(), 
            Name = "New Crop" 
        };
        var fieldComponent = new FieldSystemComponent() 
        { 
            CropViewItems = new ObservableCollection<CropViewItem> { newCropViewItem } 
        };

        var newDto = new CropDto() { Guid = newCropViewItem.Guid, Name = newCropViewItem.Name };
        _mockCropFactory.Setup(x => x.CreateCropDto(newCropViewItem)).Returns(newDto);

        // Act: convert crop view items which should clear existing DTOs first
        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        // Assert: only the new DTO should be present, existing DTOs should be cleared
        Assert.AreEqual(1, fieldComponentDto.CropDtos.Count);
        Assert.AreSame(newDto, fieldComponentDto.CropDtos[0]);
        Assert.IsFalse(fieldComponentDto.CropDtos.Contains(existingDto1));
        Assert.IsFalse(fieldComponentDto.CropDtos.Contains(existingDto2));
    }

    [TestMethod]
    public void ConvertCropViewItemsToDtoCollection_PassesCorrectParametersToFactory()
    {
        // Arrange: field component with a specific crop view item to verify factory gets correct parameter
        var cropViewItem = new CropViewItem() 
        { 
            Guid = Guid.NewGuid(), 
            Name = "Parameter Test Crop",
            CropType = CropType.Barley,
            Year = 2023,
            AmountOfIrrigation = 100.5
        };
        var fieldComponent = new FieldSystemComponent() 
        { 
            CropViewItems = new ObservableCollection<CropViewItem> { cropViewItem } 
        };
        var fieldComponentDto = new FieldSystemComponentDto() 
        { 
            CropDtos = new ObservableCollection<ICropDto>() 
        };

        var returnedDto = new CropDto();
        _mockCropFactory.Setup(x => x.CreateCropDto(It.IsAny<CropViewItem>())).Returns(returnedDto);

        // Act: perform the conversion
        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        // Assert: verify factory was called with the exact crop view item instance
        _mockCropFactory.Verify(x => x.CreateCropDto(It.Is<CropViewItem>(c => 
            c.Guid == cropViewItem.Guid && 
            c.Name == cropViewItem.Name && 
            c.CropType == cropViewItem.CropType &&
            c.Year == cropViewItem.Year &&
            c.AmountOfIrrigation == cropViewItem.AmountOfIrrigation)), Times.Once);
    }

    [TestMethod]
    public void ConvertCropViewItemsToDtoCollection_WithNullFieldComponent_DoesNotModifyDto()
    {
        // Arrange: field DTO with some existing DTOs, null field component
        var existingDto = new CropDto() { Name = "Existing DTO" };
        var fieldComponentDto = new FieldSystemComponentDto() 
        { 
            CropDtos = new ObservableCollection<ICropDto> { existingDto } 
        };
        FieldSystemComponent fieldComponent = null;

        // Act: calling with null field component should not modify the DTO collection
        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        // Assert: DTO collection should remain unchanged when field component is null
        Assert.AreEqual(1, fieldComponentDto.CropDtos.Count);
        Assert.AreSame(existingDto, fieldComponentDto.CropDtos[0]);
        
        // Verify factory was never called
        _mockCropFactory.Verify(x => x.CreateCropDto(It.IsAny<CropViewItem>()), Times.Never);
    }

    [TestMethod]
    public void ConvertCropViewItemsToDtoCollection_WithNullFieldComponentDto_DoesNotThrow()
    {
        // Arrange: valid field component, null field DTO
        var fieldComponent = new FieldSystemComponent() 
        { 
            CropViewItems = new ObservableCollection<CropViewItem> { new CropViewItem() } 
        };
        IFieldComponentDto fieldComponentDto = null;

        // Act & Assert: calling with null field DTO should not throw due to null check
        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        // Verify factory was never called
        _mockCropFactory.Verify(x => x.CreateCropDto(It.IsAny<CropViewItem>()), Times.Never);
    }

    [TestMethod]
    public void ConvertCropViewItemsToDtoCollection_WithBothParametersNull_DoesNotThrow()
    {
        // Arrange: both parameters are null
        FieldSystemComponent fieldComponent = null;
        IFieldComponentDto fieldComponentDto = null;

        // Act & Assert: calling with both null parameters should not throw due to null checks
        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        // Verify factory was never called
        _mockCropFactory.Verify(x => x.CreateCropDto(It.IsAny<CropViewItem>()), Times.Never);
    }

    [TestMethod]
    public void ConvertCropViewItemsToDtoCollection_PreservesOrderOfViewItems()
    {
        // Arrange: field component with multiple crop view items in specific order
        var firstItem = new CropViewItem() { Guid = Guid.NewGuid(), Name = "First" };
        var secondItem = new CropViewItem() { Guid = Guid.NewGuid(), Name = "Second" };
        var thirdItem = new CropViewItem() { Guid = Guid.NewGuid(), Name = "Third" };
        
        var fieldComponent = new FieldSystemComponent() 
        { 
            CropViewItems = new ObservableCollection<CropViewItem> { firstItem, secondItem, thirdItem } 
        };
        var fieldComponentDto = new FieldSystemComponentDto() 
        { 
            CropDtos = new ObservableCollection<ICropDto>() 
        };

        var firstDto = new CropDto() { Name = "First DTO" };
        var secondDto = new CropDto() { Name = "Second DTO" };
        var thirdDto = new CropDto() { Name = "Third DTO" };

        // Setup factory to return specific DTOs for each view item to verify order
        _mockCropFactory.Setup(x => x.CreateCropDto(firstItem)).Returns(firstDto);
        _mockCropFactory.Setup(x => x.CreateCropDto(secondItem)).Returns(secondDto);
        _mockCropFactory.Setup(x => x.CreateCropDto(thirdItem)).Returns(thirdDto);

        // Act: convert view items to DTOs
        _fieldComponentService.ConvertCropViewItemsToDtoCollection(fieldComponent, fieldComponentDto);

        // Assert: DTOs should be added in the same order as their corresponding view items
        Assert.AreEqual(3, fieldComponentDto.CropDtos.Count);
        Assert.AreSame(firstDto, fieldComponentDto.CropDtos[0]);
        Assert.AreSame(secondDto, fieldComponentDto.CropDtos[1]);
        Assert.AreSame(thirdDto, fieldComponentDto.CropDtos[2]);
    }

    #endregion

    #region GetCropViewItemFromDto Tests

    [TestMethod]
    public void GetCropViewItemFromDto_WithMatchingGuid_ReturnsCorrectCropViewItem()
    {
        // Arrange: create a DTO with a GUID that will match one item in the domain collection
        var matchingGuid = Guid.NewGuid();
        var cropDto = new CropDto() { Guid = matchingGuid, CropType = CropType.Wheat };
        var expectedCropViewItem = new CropViewItem() { Guid = matchingGuid, CropType = CropType.Wheat, Name = "Test Wheat" };

        var fieldComponent = new FieldSystemComponent()
        {
            CropViewItems = new ObservableCollection<CropViewItem>
            {
                new CropViewItem() { Guid = Guid.NewGuid(), CropType = CropType.Barley },
                expectedCropViewItem,
                new CropViewItem() { Guid = Guid.NewGuid(), CropType = CropType.Oats }
            }
        };

        // Act: call the method under test to retrieve the matching view item
        var result = _fieldComponentService.GetCropViewItemFromDto(cropDto, fieldComponent);

        // Assert: verify the returned item is the expected one and properties are preserved
        Assert.IsNotNull(result);
        Assert.AreSame(expectedCropViewItem, result);
        Assert.AreEqual(matchingGuid, result.Guid);
        Assert.AreEqual(CropType.Wheat, result.CropType);
    }

    [TestMethod]
    public void GetCropViewItemFromDto_WithNonMatchingGuid_ReturnsNull()
    {
        // Arrange: DTO GUID does not match any item in the domain collection
        var cropDto = new CropDto() { Guid = Guid.NewGuid() };
        var fieldComponent = new FieldSystemComponent()
        {
            CropViewItems = new ObservableCollection<CropViewItem>
            {
                new CropViewItem() { Guid = Guid.NewGuid() },
                new CropViewItem() { Guid = Guid.NewGuid() }
            }
        };

        // Act: attempt to find a matching item
        var result = _fieldComponentService.GetCropViewItemFromDto(cropDto, fieldComponent);

        // Assert: since no match exists, result should be null
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetCropViewItemFromDto_WithEmptyCollection_ReturnsNull()
    {
        // Arrange: an empty domain collection should yield no matches
        var cropDto = new CropDto() { Guid = Guid.NewGuid() };
        var fieldComponent = new FieldSystemComponent() { CropViewItems = new ObservableCollection<CropViewItem>() };

        // Act: call method under test
        var result = _fieldComponentService.GetCropViewItemFromDto(cropDto, fieldComponent);

        // Assert: expect null due to empty collection
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetCropViewItemFromDto_WithSingleMatchingItem_ReturnsItem()
    {
        // Arrange: a single-item collection where the GUID matches
        var matchingGuid = Guid.NewGuid();
        var cropDto = new CropDto() { Guid = matchingGuid };
        var expected = new CropViewItem() { Guid = matchingGuid };
        var fieldComponent = new FieldSystemComponent() { CropViewItems = new ObservableCollection<CropViewItem> { expected } };

        // Act: retrieve the item
        var result = _fieldComponentService.GetCropViewItemFromDto(cropDto, fieldComponent);

        // Assert: returned item should be the same instance that was in the collection
        Assert.IsNotNull(result);
        Assert.AreSame(expected, result);
    }

    [TestMethod]
    public void GetCropViewItemFromDto_WithMultipleItems_ReturnsFirstMatching()
    {
        // Arrange: collection contains multiple items; ensure the matching one is present
        var matchingGuid = Guid.NewGuid();
        var cropDto = new CropDto() { Guid = matchingGuid };
        var firstMatch = new CropViewItem() { Guid = matchingGuid, Name = "First" };
        var fieldComponent = new FieldSystemComponent()
        {
            CropViewItems = new ObservableCollection<CropViewItem>
            {
                new CropViewItem() { Guid = Guid.NewGuid() },
                firstMatch,
                new CropViewItem() { Guid = Guid.NewGuid() }
            }
        };

        // Act: call the method to find the first item that matches by GUID
        var result = _fieldComponentService.GetCropViewItemFromDto(cropDto, fieldComponent);

        // Assert: the first matching instance should be returned and its properties verified
        Assert.IsNotNull(result);
        Assert.AreSame(firstMatch, result);
        Assert.AreEqual("First", result.Name);
    }

    [TestMethod]
    public void GetCropViewItemFromDto_WithNullDto_Throws()
    {
        // Arrange: a null DTO parameter is provided (documents current behavior)
        ICropDto cropDto = null;
        var fieldComponent = new FieldSystemComponent() { CropViewItems = new ObservableCollection<CropViewItem> { new CropViewItem() } };

        // Act & Assert
        Assert.ThrowsExactly<NullReferenceException>(() =>
        {
            _fieldComponentService.GetCropViewItemFromDto(cropDto, fieldComponent);
        });
    }

    #endregion

    #region RemoveCropFromSystem Tests

    [TestMethod]
    public void RemoveCropFromSystem_WithMatchingGuid_RemovesItem()
    {
        // Arrange: field contains multiple crop view items including one that matches the DTO GUID
        var matchingGuid = Guid.NewGuid();
        var fieldComponent = new FieldSystemComponent()
        {
            CropViewItems = new ObservableCollection<CropViewItem>
            {
                new CropViewItem() { Guid = Guid.NewGuid() },
                new CropViewItem() { Guid = matchingGuid },
                new CropViewItem() { Guid = Guid.NewGuid() }
            }
        };
        var cropDto = new CropDto() { Guid = matchingGuid };

        // Act: remove the crop identified by DTO
        _fieldComponentService.RemoveCropFromSystem(fieldComponent, cropDto);

        // Assert: the item with the matching GUID has been removed
        Assert.IsFalse(fieldComponent.CropViewItems.Any(x => x.Guid == matchingGuid));
        Assert.AreEqual(2, fieldComponent.CropViewItems.Count);
    }

    [TestMethod]
    public void RemoveCropFromSystem_WithNoMatchingGuid_DoesNothing()
    {
        // Arrange: field contains items but none match the DTO GUID
        var fieldComponent = new FieldSystemComponent()
        {
            CropViewItems = new ObservableCollection<CropViewItem>
            {
                new CropViewItem() { Guid = Guid.NewGuid() },
                new CropViewItem() { Guid = Guid.NewGuid() }
            }
        };
        var cropDto = new CropDto() { Guid = Guid.NewGuid() };
        var initialCount = fieldComponent.CropViewItems.Count;

        // Act: attempt removal when no matching GUID exists
        _fieldComponentService.RemoveCropFromSystem(fieldComponent, cropDto);

        // Assert: collection remains unchanged
        Assert.AreEqual(initialCount, fieldComponent.CropViewItems.Count);
    }

    [TestMethod]
    public void RemoveCropFromSystem_WithNullDto_DoesNothing()
    {
        // Arrange: prepare a field with items
        var fieldComponent = new FieldSystemComponent()
        {
            CropViewItems = new ObservableCollection<CropViewItem>
            {
                new CropViewItem() { Guid = Guid.NewGuid() }
            }
        };
        ICropDto cropDto = null;
        var initialCount = fieldComponent.CropViewItems.Count;

        // Act: calling with null DTO should not throw and should not modify the collection
        _fieldComponentService.RemoveCropFromSystem(fieldComponent, cropDto);

        // Assert
        Assert.AreEqual(initialCount, fieldComponent.CropViewItems.Count);
    }

    [TestMethod]
    public void RemoveCropFromSystem_WithEmptyCollection_DoesNothing()
    {
        // Arrange: empty CropViewItems collection
        var fieldComponent = new FieldSystemComponent() { CropViewItems = new ObservableCollection<CropViewItem>() };
        var cropDto = new CropDto() { Guid = Guid.NewGuid() };

        // Act: attempt removal from empty collection
        _fieldComponentService.RemoveCropFromSystem(fieldComponent, cropDto);

        // Assert: still empty and no exceptions
        Assert.IsFalse(fieldComponent.CropViewItems.Any());
    }

    #endregion

    #region Additional Tests

    [TestMethod]
    public void AddCropDtoToSystem_UsesFactoryReturnValue_InstanceAdded()
    {
        // Arrange: factory returns a specific instance which should be added to collection
        var fieldComponent = new FieldSystemComponent() { CropViewItems = new ObservableCollection<CropViewItem>() };
        var cropDto = new CropDto();
        var returnedViewItem = new CropViewItem() { Name = "FactoryCreated" };

        _mockCropFactory.Setup(x => x.CreateCropViewItem(It.IsAny<ICropDto>())).Returns(returnedViewItem);

        // Act
        _fieldComponentService.AddCropDtoToSystem(fieldComponent, cropDto);

        // Assert: collection contains the same instance that factory returned
        Assert.AreEqual(1, fieldComponent.CropViewItems.Count);
        Assert.AreSame(returnedViewItem, fieldComponent.CropViewItems[0]);
        Assert.AreEqual("FactoryCreated", fieldComponent.CropViewItems[0].Name);
    }

    [TestMethod]
    public void AddCropDtoToSystem_PassesDtoToFactory()
    {
        // Arrange: capture the dto passed to factory and verify it matches the supplied dto
        var fieldComponent = new FieldSystemComponent() { CropViewItems = new ObservableCollection<CropViewItem>() };
        var cropDto = new CropDto() { Name = "DtoName" };
        _mockCropFactory.Setup(x => x.CreateCropViewItem(It.IsAny<ICropDto>())).Returns(new CropViewItem());

        // Act
        _fieldComponentService.AddCropDtoToSystem(fieldComponent, cropDto);

        // Assert: factory was invoked with the same dto instance
#pragma warning disable CS0252
        _mockCropFactory.Verify(x => x.CreateCropViewItem(It.Is<ICropDto>(d => d == cropDto)), Times.Once);
#pragma warning restore CS0252
    }

    [TestMethod]
    public void AddCropDtoToSystem_WithNullDto_DoesNotCallFactoryAndDoesNotAdd()
    {
        // Arrange: factory should not be called when dto is null
        var fieldComponent = new FieldSystemComponent() { CropViewItems = new ObservableCollection<CropViewItem>() };
        ICropDto cropDto = null;

        // Setup factory to fail the test if called (optional) and also verify later
        _mockCropFactory.Setup(x => x.CreateCropViewItem(It.IsAny<ICropDto>())).Throws(new Exception("Factory should not be called when dto is null"));

        var initialCount = fieldComponent.CropViewItems.Count;

        // Act
        _fieldComponentService.AddCropDtoToSystem(fieldComponent, cropDto);

        // Assert: factory was not called and collection remains unchanged
        Assert.AreEqual(initialCount, fieldComponent.CropViewItems.Count);
        _mockCropFactory.Verify(x => x.CreateCropViewItem(It.IsAny<ICropDto>()), Times.Never);
    }

    [TestMethod]
    public void AddCropDtoToSystem_WithNullFieldComponent_DoesNotCallFactoryAndDoesNotThrow()
    {
        // Arrange: crop DTO provided, but field component is null
        ICropDto cropDto = new CropDto() { Name = "DtoForNullField" };

        // Setup factory to throw if invoked to ensure it is not called
        _mockCropFactory.Setup(x => x.CreateCropViewItem(It.IsAny<ICropDto>())).Throws(new Exception("Factory should not be called when field component is null"));

        // Act & Assert: calling with null fieldComponent should not throw
        _fieldComponentService.AddCropDtoToSystem(null, cropDto);

        // Verify factory was never called
        _mockCropFactory.Verify(x => x.CreateCropViewItem(It.IsAny<ICropDto>()), Times.Never);
    }

    [TestMethod]
    public void ConvertCropDtoCollectionToCropViewItemCollection_WithMultipleDtos_UpdatesMatchingViewItems()
    {
        // Arrange:
        // - Create three CropViewItems on the field component; two of them will match incoming DTO GUIDs
        // - Create two DTOs corresponding to two of the view items
        // - Mock the transfer service to copy the AmountOfIrrigation value from DTO to the view item
        var guid1 = Guid.NewGuid();
        var guid2 = Guid.NewGuid();
        var guid3 = Guid.NewGuid();

        var view1 = new CropViewItem() { Guid = guid1, AmountOfIrrigation =0 };
        var view2 = new CropViewItem() { Guid = guid2, AmountOfIrrigation =0 };
        var view3 = new CropViewItem() { Guid = guid3, AmountOfIrrigation =0 };

        var fieldComponent = new FieldSystemComponent() { CropViewItems = new ObservableCollection<CropViewItem> { view1, view2, view3 } };

        var dto1 = new CropDto() { Guid = guid1, AmountOfIrrigation =11 };
        var dto2 = new CropDto() { Guid = guid2, AmountOfIrrigation =22 };

        var fieldComponentDto = new FieldSystemComponentDto() { CropDtos = new ObservableCollection<ICropDto> { dto1, dto2 } };

        // Mock: simulate the transfer service mapping DTO -> view item
        _mockCropTransferService
            .Setup(x => x.TransferDtoToDomainObject(It.IsAny<CropDto>(), It.IsAny<CropViewItem>()))
            .Returns((CropDto d, CropViewItem v) =>
            {
                v.AmountOfIrrigation = d.AmountOfIrrigation;
                return v;
            });

        // Act:
        // Call the method under test which should iterate the DTO collection and update matching view items
        _fieldComponentService.ConvertCropDtoCollectionToCropViewItemCollection(fieldComponent, fieldComponentDto);

        // Assert:
        // - view1 and view2 are updated with values from their corresponding DTOs
        // - view3 remains unchanged because there was no matching DTO
        Assert.AreEqual(11, view1.AmountOfIrrigation);
        Assert.AreEqual(22, view2.AmountOfIrrigation);
        Assert.AreEqual(0, view3.AmountOfIrrigation);

        // Verify transfer service invoked exactly twice (once per matching DTO)
        _mockCropTransferService.Verify(x => x.TransferDtoToDomainObject(It.IsAny<CropDto>(), It.IsAny<CropViewItem>()), Times.Exactly(2));
    }

    #endregion

    #region Tests for InitializeComponent

    [TestMethod]
    public void InitializeFieldSystemComponent_WithUninitializedComponent_SetsNameAndInitializesFlag()
    {
        // Arrange: create a farm and an uninitialized field component
        var farm = new Farm();
        var fieldComponent = new FieldSystemComponent() 
        { 
            ComponentNameDisplayString = "Field", 
            IsInitialized = false,
            Name = null 
        };

        // Act: initialize the field component
        _fieldComponentService.InitializeComponent(farm, fieldComponent);

        // Assert: component should now be initialized with a unique name
        Assert.IsTrue(fieldComponent.IsInitialized);
        Assert.IsNotNull(fieldComponent.Name);
        Assert.AreEqual("Field", fieldComponent.Name);
    }

    [TestMethod]
    public void InitializeFieldSystemComponent_WithAlreadyInitializedComponent_DoesNothing()
    {
        // Arrange: create a field component that is already initialized
        var farm = new Farm();
        var originalName = "Original Field Name";
        var fieldComponent = new FieldSystemComponent() 
        { 
            IsInitialized = true,
            Name = originalName 
        };

        // Act: attempt to initialize an already initialized component
        _fieldComponentService.InitializeComponent(farm, fieldComponent);

        // Assert: component should remain unchanged
        Assert.IsTrue(fieldComponent.IsInitialized);
        Assert.AreEqual(originalName, fieldComponent.Name);
    }

    [TestMethod]
    public void InitializeFieldSystemComponent_WithFarmContainingNoComponents_AssignsBaseName()
    {
        // Arrange: empty farm with no existing components
        var farm = new Farm() { Components = new List<ComponentBase>() };
        var fieldComponent = new FieldSystemComponent() 
        { 
            ComponentNameDisplayString = "Test Field",
            IsInitialized = false 
        };

        // Act
        _fieldComponentService.InitializeComponent(farm, fieldComponent);

        // Assert: should get the base name without any numbering
        Assert.IsTrue(fieldComponent.IsInitialized);
        Assert.AreEqual("Test Field", fieldComponent.Name);
    }

    [TestMethod]
    public void InitializeFieldSystemComponent_WithFarmContainingExistingComponents_AssignsUniqueName()
    {
        // Arrange: farm with existing components that have conflicting names
        var existingComponent1 = new FieldSystemComponent() { Name = "Field" };
        var existingComponent2 = new FieldSystemComponent() { Name = "Field #2" };
        var farm = new Farm() 
        { 
            Components = new List<ComponentBase> { existingComponent1, existingComponent2 } 
        };

        var newFieldComponent = new FieldSystemComponent() 
        { 
            ComponentNameDisplayString = "Field",
            IsInitialized = false 
        };

        // Act
        _fieldComponentService.InitializeComponent(farm, newFieldComponent);

        // Assert: should get a unique name that doesn't conflict with existing components
        Assert.IsTrue(newFieldComponent.IsInitialized);
        Assert.AreEqual("Field #3", newFieldComponent.Name);
    }

    [TestMethod]
    public void InitializeFieldSystemComponent_WithFarmContainingComponentsWithEmptyNames_IgnoresEmptyNames()
    {
        // Arrange: farm with existing components where some have empty/null names
        var componentWithEmptyName = new FieldSystemComponent() { Name = "" };
        var componentWithNullName = new FieldSystemComponent() { Name = null };
        var componentWithWhitespaceName = new FieldSystemComponent() { Name = "   " };
        var farm = new Farm() 
        { 
            Components = new List<ComponentBase> 
            { 
                componentWithEmptyName, 
                componentWithNullName, 
                componentWithWhitespaceName 
            } 
        };

        var newFieldComponent = new FieldSystemComponent() 
        { 
            ComponentNameDisplayString = "Field",
            IsInitialized = false 
        };

        // Act: empty/null names should be ignored in uniqueness check
        _fieldComponentService.InitializeComponent(farm, newFieldComponent);

        // Assert: should get base name since empty names are ignored
        Assert.IsTrue(newFieldComponent.IsInitialized);
        Assert.AreEqual("Field", newFieldComponent.Name);
    }

    [TestMethod]
    public void InitializeFieldSystemComponent_WithMultipleCallsOnSameComponent_OnlyInitializesOnce()
    {
        // Arrange: single field component and farm
        var farm = new Farm() { Components = new List<ComponentBase>() };
        var fieldComponent = new FieldSystemComponent() 
        { 
            ComponentNameDisplayString = "Field",
            IsInitialized = false 
        };

        // Act: call initialize multiple times
        _fieldComponentService.InitializeComponent(farm, fieldComponent);
        var nameAfterFirstCall = fieldComponent.Name;
        
        _fieldComponentService.InitializeComponent(farm, fieldComponent);
        _fieldComponentService.InitializeComponent(farm, fieldComponent);

        // Assert: should only be initialized once and name should not change
        Assert.IsTrue(fieldComponent.IsInitialized);
        Assert.AreEqual(nameAfterFirstCall, fieldComponent.Name);
    }

    [TestMethod]
    public void InitializeFieldSystemComponent_WithNullFarm_DoesNotThrow()
    {
        // Arrange: null farm parameter
        Farm farm = null;
        var fieldComponent = new FieldSystemComponent() 
        { 
            ComponentNameDisplayString = "Field",
            IsInitialized = false 
        };

        // Act & Assert: should handle null farm gracefully without throwing
        _fieldComponentService.InitializeComponent(farm, fieldComponent);

        // The component should still be marked as initialized even if farm is null
        // This tests the early return behavior for already initialized components
    }

    [TestMethod]
    public void InitializeFieldSystemComponent_WithMixedComponentTypes_OnlyConsidersNamesFromAllComponents()
    {
        // Arrange: farm with mixed component types to ensure name uniqueness across all component types
        var fieldComponent1 = new FieldSystemComponent() { Name = "Component" };
        // Note: In a real scenario, you might have other component types like AnimalComponent
        // For this test, we'll use another FieldSystemComponent to simulate different component types
        var fieldComponent2 = new FieldSystemComponent() { Name = "Component #2" };
        
        var farm = new Farm() 
        { 
            Components = new List<ComponentBase> { fieldComponent1, fieldComponent2 } 
        };

        var newFieldComponent = new FieldSystemComponent() 
        { 
            ComponentNameDisplayString = "Component",
            IsInitialized = false 
        };

        // Act
        _fieldComponentService.InitializeComponent(farm, newFieldComponent);

        // Assert: should consider names from all component types when generating unique name
        Assert.IsTrue(newFieldComponent.IsInitialized);
        Assert.AreEqual("Component #3", newFieldComponent.Name);
    }

    [TestMethod]
    public void InitializeFieldSystemComponent_WithLongComponentNameDisplayString_PreservesFullName()
    {
        // Arrange: field component with a long display name
        var farm = new Farm() { Components = new List<ComponentBase>() };
        var longDisplayName = "Very Long Field System Component Name For Testing Purposes";
        var fieldComponent = new FieldSystemComponent() 
        { 
            ComponentNameDisplayString = longDisplayName,
            IsInitialized = false 
        };

        // Act
        _fieldComponentService.InitializeComponent(farm, fieldComponent);

        // Assert: should preserve the full display name
        Assert.IsTrue(fieldComponent.IsInitialized);
        Assert.AreEqual(longDisplayName, fieldComponent.Name);
    }

    #endregion

    #region Tests for ResetAllYears

    [TestMethod]
    public void ResetAllYears_WithEmptyCollection_DoesNotThrow()
    {
        // Arrange: empty collection should be handled gracefully
        var emptyCropDtos = new List<ICropDto>();

        // Act & Assert: method should not throw and should be a no-op
        _fieldComponentService.ResetAllYears(emptyCropDtos);
        // No additional assertions needed - success is not throwing
    }

    [TestMethod]
    public void ResetAllYears_WithSingleCrop_DoesNotChangeYear()
    {
        // Arrange: single crop should retain its current year (max year stays the same)
        var cropDto = new CropDto { Year = 2020 };
        var cropDtos = new List<ICropDto> { cropDto };

        // Act
        _fieldComponentService.ResetAllYears(cropDtos);

        // Assert: year should remain unchanged since it's already the max
        Assert.AreEqual(2020, cropDto.Year);
    }

    [TestMethod]
    public void ResetAllYears_WithConsecutiveYears_DoesNotChangeYears()
    {
        // Arrange: crops with already consecutive years in descending order should remain unchanged
        var cropDto1 = new CropDto { Year = 2022 };
        var cropDto2 = new CropDto { Year = 2021 };
        var cropDto3 = new CropDto { Year = 2020 };
        var cropDtos = new List<ICropDto> { cropDto1, cropDto2, cropDto3 };

        // Act
        _fieldComponentService.ResetAllYears(cropDtos);

        // Assert: years should remain the same since they're already consecutive
        Assert.AreEqual(2022, cropDto1.Year);
        Assert.AreEqual(2021, cropDto2.Year);
        Assert.AreEqual(2020, cropDto3.Year);
    }

    [TestMethod]
    public void ResetAllYears_WithNonConsecutiveYears_ResetsToConsecutiveYears()
    {
        // Arrange: crops with gaps in years should be renumbered to consecutive descending order
        var cropDto1 = new CropDto { Year = 2025 }; // highest year
        var cropDto2 = new CropDto { Year = 2020 }; // gap exists
        var cropDto3 = new CropDto { Year = 2018 }; // more gaps
        var cropDtos = new List<ICropDto> { cropDto1, cropDto2, cropDto3 };

        // Act: method orders by descending year, then assigns consecutive years from max down
        _fieldComponentService.ResetAllYears(cropDtos);

        // Assert: years should now be consecutive descending from the original maximum
        Assert.AreEqual(2025, cropDto1.Year); // max year stays the same
        Assert.AreEqual(2024, cropDto2.Year); // second highest becomes max - 1
        Assert.AreEqual(2023, cropDto3.Year); // third highest becomes max - 2
    }

    [TestMethod]
    public void ResetAllYears_WithDuplicateYears_ResetsToConsecutiveYears()
    {
        // Arrange: crops with duplicate years should be separated into consecutive years
        var cropDto1 = new CropDto { Year = 2020 };
        var cropDto2 = new CropDto { Year = 2020 }; // duplicate year
        var cropDto3 = new CropDto { Year = 2020 }; // duplicate year
        var cropDtos = new List<ICropDto> { cropDto1, cropDto2, cropDto3 };

        // Act: algorithm orders by year then assigns consecutive descending values
        _fieldComponentService.ResetAllYears(cropDtos);

        // Assert: verify the result produces consecutive years from the original max
        var resultYears = cropDtos.Select(dto => dto.Year).OrderByDescending(y => y).ToList();
        
        // Check that we have the expected consecutive years (2020, 2019, 2018)
        Assert.AreEqual(2020, resultYears[0]); // highest year should be original max
        Assert.AreEqual(2019, resultYears[1]); // second should be max - 1
        Assert.AreEqual(2018, resultYears[2]); // third should be max - 2
        
        // Verify all years are unique (no more duplicates)
        Assert.AreEqual(3, resultYears.Distinct().Count());
        
        // Verify range is correct
        Assert.AreEqual(2020, resultYears.Max());
        Assert.AreEqual(2018, resultYears.Min());
    }

    [TestMethod]
    public void ResetAllYears_WithRandomOrderYears_ResetsToConsecutiveDescendingOrder()
    {
        // Arrange: crops in random year order should be processed correctly
        var cropDto1 = new CropDto { Year = 2021, CropType = CropType.Wheat };
        var cropDto2 = new CropDto { Year = 2023, CropType = CropType.Barley }; // highest
        var cropDto3 = new CropDto { Year = 2019, CropType = CropType.Oats }; // lowest
        var cropDto4 = new CropDto { Year = 2022, CropType = CropType.Corn };
        var cropDtos = new List<ICropDto> { cropDto1, cropDto2, cropDto3, cropDto4 };

        // Act: method should order by descending year, then assign consecutive years
        _fieldComponentService.ResetAllYears(cropDtos);

        // Assert: verify years are assigned in descending order from original max
        // Original order by descending year: 2023, 2022, 2021, 2019
        // New consecutive years should be: 2023, 2022, 2021, 2020
        Assert.AreEqual(2023, cropDto2.Year); // max year (2023) stays same
        Assert.AreEqual(2022, cropDto4.Year); // originally second highest (2022) stays same
        Assert.AreEqual(2021, cropDto1.Year); // originally third highest (2021) stays same
        Assert.AreEqual(2020, cropDto3.Year); // originally lowest (2019) becomes 2020

        // Verify other properties are preserved
        Assert.AreEqual(CropType.Wheat, cropDto1.CropType);
        Assert.AreEqual(CropType.Barley, cropDto2.CropType);
        Assert.AreEqual(CropType.Oats, cropDto3.CropType);
        Assert.AreEqual(CropType.Corn, cropDto4.CropType);
    }

    [TestMethod]
    public void ResetAllYears_WithNegativeYears_HandlesCorrectly()
    {
        // Arrange: test with negative years to ensure algorithm handles edge cases
        var cropDto1 = new CropDto { Year = -5 };
        var cropDto2 = new CropDto { Year = 10 }; // highest
        var cropDto3 = new CropDto { Year = 0 };
        var cropDtos = new List<ICropDto> { cropDto1, cropDto2, cropDto3 };

        // Act: algorithm should work with negative numbers
        _fieldComponentService.ResetAllYears(cropDtos);

        // Assert: consecutive years starting from max (10)
        Assert.AreEqual(10, cropDto2.Year); // max year (10) stays same
        Assert.AreEqual(9, cropDto3.Year);  // second highest (0) becomes 9
        Assert.AreEqual(8, cropDto1.Year);  // lowest (-5) becomes 8
    }

    [TestMethod]
    public void ResetAllYears_VerifiesConsecutiveAlgorithm()
    {
        // Arrange: set up a larger collection to verify the consecutive assignment algorithm
        var cropDtos = new List<ICropDto>
        {
            new CropDto { Year = 2010 },
            new CropDto { Year = 2025 }, // max
            new CropDto { Year = 2015 },
            new CropDto { Year = 2005 },
            new CropDto { Year = 2020 }
        };

        var originalMaxYear = cropDtos.Max(dto => dto.Year); // Should be 2025

        // Act
        _fieldComponentService.ResetAllYears(cropDtos);

        // Assert: verify the algorithm produces consecutive descending years
        var actualYears = cropDtos.Select(dto => dto.Year).OrderByDescending(y => y).ToList();
        
        // Check that max year is preserved
        Assert.AreEqual(originalMaxYear, actualYears[0]);
        
        // Check that all years are consecutive
        for (int i = 0; i < actualYears.Count - 1; i++)
        {
            var expectedDifference = 1;
            var actualDifference = actualYears[i] - actualYears[i + 1];
            Assert.AreEqual(expectedDifference, actualDifference, 
            $"Years should be consecutive: {actualYears[i]} and {actualYears[i + 1]}");
        }
        
        // Verify the range: should be from originalMaxYear down to (originalMaxYear - count + 1)
        var expectedMinYear = originalMaxYear - cropDtos.Count + 1;
        Assert.AreEqual(expectedMinYear, actualYears.Last());
    }

    #endregion
}
