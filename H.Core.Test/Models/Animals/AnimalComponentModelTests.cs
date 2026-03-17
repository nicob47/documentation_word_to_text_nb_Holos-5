using H.Core.Models;
using H.Core.Models.Animals.Beef;
using H.Core.Models.Animals.Swine;
using H.Core.Models.Animals.Poultry.Chicken;
using H.Core.Models.Animals.Poultry.Turkey;

namespace H.Core.Test.Models.Animals;

[TestClass]
public class AnimalComponentModelTests
{
    [TestMethod]
    public void CowCalfComponent_HasCorrectCategoryAndType()
    {
        var component = new CowCalfComponent();

        Assert.AreEqual(ComponentCategory.BeefProduction, component.ComponentCategory);
        Assert.AreEqual(ComponentType.CowCalf, component.ComponentType);
    }

    [TestMethod]
    public void BackgroundingComponent_HasCorrectCategoryAndType()
    {
        var component = new BackgroundingComponent();

        Assert.AreEqual(ComponentCategory.BeefProduction, component.ComponentCategory);
        Assert.AreEqual(ComponentType.Backgrounding, component.ComponentType);
    }

    [TestMethod]
    public void FinishingComponent_HasCorrectCategoryAndType()
    {
        var component = new FinishingComponent();

        Assert.AreEqual(ComponentCategory.BeefProduction, component.ComponentCategory);
        Assert.AreEqual(ComponentType.Finishing, component.ComponentType);
    }

    [TestMethod]
    public void FarrowToFinishComponent_HasCorrectCategoryAndType()
    {
        var component = new FarrowToFinishComponent();

        Assert.AreEqual(ComponentCategory.Swine, component.ComponentCategory);
        Assert.AreEqual(ComponentType.FarrowToFinish, component.ComponentType);
    }

    [TestMethod]
    public void FarrowToWeanComponent_HasCorrectCategoryAndType()
    {
        var component = new FarrowToWeanComponent();

        Assert.AreEqual(ComponentCategory.Swine, component.ComponentCategory);
        Assert.AreEqual(ComponentType.FarrowToWean, component.ComponentType);
    }

    [TestMethod]
    public void GrowerToFinishComponent_HasCorrectCategoryAndType()
    {
        var component = new GrowerToFinishComponent();

        Assert.AreEqual(ComponentCategory.Swine, component.ComponentCategory);
        Assert.AreEqual(ComponentType.SwineGrowers, component.ComponentType);
    }

    [TestMethod]
    public void IsoWeanComponent_HasCorrectCategoryAndType()
    {
        var component = new IsoWeanComponent();

        Assert.AreEqual(ComponentCategory.Swine, component.ComponentCategory);
        Assert.AreEqual(ComponentType.IsoWean, component.ComponentType);
    }

    [TestMethod]
    public void ChickenMeatProductionComponent_HasCorrectCategoryAndType()
    {
        var component = new ChickenMeatProductionComponent();

        Assert.AreEqual(ComponentCategory.Poultry, component.ComponentCategory);
        Assert.AreEqual(ComponentType.ChickenMeatProduction, component.ComponentType);
    }

    [TestMethod]
    public void TurkeyMeatProductionComponent_HasCorrectCategoryAndType()
    {
        var component = new TurkeyMeatProductionComponent();

        Assert.AreEqual(ComponentCategory.Poultry, component.ComponentCategory);
        Assert.AreEqual(ComponentType.TurkeyMeatProduction, component.ComponentType);
    }
}
