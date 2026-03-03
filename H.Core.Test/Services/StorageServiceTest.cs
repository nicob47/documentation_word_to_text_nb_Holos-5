using H.Core.Models;
using H.Core.Services.StorageService;

namespace H.Core.Test.Services;

[TestClass]
public class StorageServiceTest
{
    #region Fields

    private IStorageService _storageService = null!;
    private GlobalSettings _globalSettings = null!;
    private ApplicationData _applicationData = null!;
    private Storage _storage = null!;

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
        _globalSettings = new GlobalSettings();
        _applicationData = new ApplicationData() {GlobalSettings = _globalSettings};
        _storage = new Storage() {ApplicationData = _applicationData};

        _storageService = new DefaultStorageService(_storage);
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    #region Tests

    [TestMethod]
    public void GetActiveFarmReturnsNotNull()
    {
        _globalSettings.ActiveFarm = new Farm();

        var result = _storageService.GetActiveFarm();

        Assert.IsNotNull(result);
    }

    [TestMethod]
    public void GetAllFarmsReturnsEmptyCollection()
    {
        var result = _storageService.GetAllFarms();

        Assert.IsFalse(result.Any());
    }

    [TestMethod]
    public void GetAllFarmsReturnsNonEmptyCollection()
    {
        _applicationData.Farms.Add(new Farm());
        _applicationData.Farms.Add(new Farm());

        var result = _storageService.GetAllFarms();

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void GetAllFarmsReturnsUniqueCollection()
    {
        _applicationData.Farms.Add(new Farm());
        _applicationData.Farms.Add(new Farm());

        var result = _storageService.GetAllFarms();

        Assert.AreEqual(2, result.Select(x => x.Guid).Distinct().Count());
    }

    [TestMethod]
    public void AddFarm()
    {
        _storageService.AddFarm(new Farm());
        _storageService.AddFarm(new Farm());

        Assert.AreEqual(2, _applicationData.Farms.Count);
    }

    #endregion
}
