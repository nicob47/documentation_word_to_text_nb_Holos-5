using H.Core.Factories;
using H.Core.Factories.Fields;
using Moq;
using Prism.Ioc;

namespace H.Core.Test.Factories;

[TestClass]
public class FieldComponentDtoFactoryTest
{
    #region Fields

    private IFieldFactory _factory = null!;

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
        var mockContainerProvider = new Mock<IContainerProvider>();

        _factory = new FieldFactory(mockContainerProvider.Object);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    #region Tests



    #endregion
}
