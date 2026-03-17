using H.Core.Factories.Animals;
using H.Core.Mappers;
using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Models.Animals.Beef;
using H.Core.Models.Animals.Swine;

namespace H.Core.Test.Mappers;

[TestClass]
public class BeefSwineMapperTests
{
    [TestMethod]
    public void PropertyMapper_CopiesCowCalfComponentToDto()
    {
        var source = new CowCalfComponent { Name = "Ranch A" };
        var dto = new AnimalComponentDto();

        PropertyMapper.CopyTo(source, dto);

        Assert.AreEqual("Ranch A", dto.Name);
    }

    [TestMethod]
    public void PropertyMapper_CopiesBackgroundingComponentToDto()
    {
        var source = new BackgroundingComponent { Name = "Stocker Pasture" };
        var dto = new AnimalComponentDto();

        PropertyMapper.CopyTo(source, dto);

        Assert.AreEqual("Stocker Pasture", dto.Name);
    }

    [TestMethod]
    public void PropertyMapper_CopiesFinishingComponentToDto()
    {
        var source = new FinishingComponent { Name = "Feedlot #1" };
        var dto = new AnimalComponentDto();

        PropertyMapper.CopyTo(source, dto);

        Assert.AreEqual("Feedlot #1", dto.Name);
    }

    [TestMethod]
    public void PropertyMapper_CopiesFarrowToFinishComponentToDto()
    {
        var source = new FarrowToFinishComponent { Name = "Hog Barn" };
        var dto = new AnimalComponentDto();

        PropertyMapper.CopyTo(source, dto);

        Assert.AreEqual("Hog Barn", dto.Name);
    }

    [TestMethod]
    public void PropertyMapper_CopiesFarrowToWeanComponentToDto()
    {
        var source = new FarrowToWeanComponent { Name = "Sow Unit" };
        var dto = new AnimalComponentDto();

        PropertyMapper.CopyTo(source, dto);

        Assert.AreEqual("Sow Unit", dto.Name);
    }

    [TestMethod]
    public void PropertyMapper_CopiesGrowerToFinishComponentToDto()
    {
        var source = new GrowerToFinishComponent { Name = "Grower Barn" };
        var dto = new AnimalComponentDto();

        PropertyMapper.CopyTo(source, dto);

        Assert.AreEqual("Grower Barn", dto.Name);
    }

    [TestMethod]
    public void PropertyMapper_CopiesIsoWeanComponentToDto()
    {
        var source = new IsoWeanComponent { Name = "Nursery" };
        var dto = new AnimalComponentDto();

        PropertyMapper.CopyTo(source, dto);

        Assert.AreEqual("Nursery", dto.Name);
    }

    [TestMethod]
    public void PropertyMapper_DtoToBeefModel_CopiesName()
    {
        var dto = new AnimalComponentDto { Name = "From DTO" };
        var model = new CowCalfComponent();

        PropertyMapper.CopyTo(dto, model);

        Assert.AreEqual("From DTO", model.Name);
    }

    [TestMethod]
    public void PropertyMapper_DtoToSwineModel_CopiesName()
    {
        var dto = new AnimalComponentDto { Name = "From DTO" };
        var model = new FarrowToFinishComponent();

        PropertyMapper.CopyTo(dto, model);

        Assert.AreEqual("From DTO", model.Name);
    }

    [TestMethod]
    public void PropertyMapper_PreservesGuidOnBeefComponent()
    {
        var model = new CowCalfComponent();
        var originalGuid = model.Guid;
        var dto = new AnimalComponentDto { Name = "Test" };

        PropertyMapper.CopyTo(dto, model);

        Assert.AreEqual(originalGuid, model.Guid, "Guid should not be overwritten by PropertyMapper");
    }

    [TestMethod]
    public void PropertyMapper_PreservesGuidOnSwineComponent()
    {
        var model = new FarrowToFinishComponent();
        var originalGuid = model.Guid;
        var dto = new AnimalComponentDto { Name = "Test" };

        PropertyMapper.CopyTo(dto, model);

        Assert.AreEqual(originalGuid, model.Guid, "Guid should not be overwritten by PropertyMapper");
    }

    [TestMethod]
    public void AnimalComponentMapper_MapsBeefComponentToDto()
    {
        var mapper = new AnimalComponentBaseToAnimalComponentDtoMapper();
        var source = new CowCalfComponent { Name = "Ranch B" };

        var result = mapper.Map(source);

        Assert.AreEqual("Ranch B", result.Name);
    }

    [TestMethod]
    public void AnimalComponentMapper_MapsSwineComponentToDto()
    {
        var mapper = new AnimalComponentBaseToAnimalComponentDtoMapper();
        var source = new FarrowToFinishComponent { Name = "Hog Farm" };

        var result = mapper.Map(source);

        Assert.AreEqual("Hog Farm", result.Name);
    }
}
