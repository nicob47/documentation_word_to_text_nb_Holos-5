using H.Core.Enumerations;
using H.Core.Services;

namespace H.Core.Test.Services;

[TestClass]
public class CountrySettingsTest
{
    #region Fields

    private ICountrySettings _countrySettings = null!;

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
        _countrySettings = new CountrySettings();
    }

    [TestCleanup]
    public void TestCleanup()
    {
    }

    #endregion

    [TestMethod]
    public void GetProvincesForCanadaReturnsCorrectList()
    {
        _countrySettings.Version = CountryVersion.Canada;

        var result = _countrySettings.GetProvinces().Where(x => x != Province.SelectProvince).ToList();

        Assert.AreEqual(10, result.Count);

        Assert.IsTrue(result.Any(x => x == Province.Alberta));
        Assert.IsTrue(result.Any(x => x == Province.BritishColumbia));
        Assert.IsTrue(result.Any(x => x == Province.Manitoba));
        Assert.IsTrue(result.Any(x => x == Province.NewBrunswick));
        Assert.IsTrue(result.Any(x => x == Province.Newfoundland));
        Assert.IsTrue(result.Any(x => x == Province.NovaScotia));
        Assert.IsTrue(result.Any(x => x == Province.Ontario));
        Assert.IsTrue(result.Any(x => x == Province.PrinceEdwardIsland));
        Assert.IsTrue(result.Any(x => x == Province.Quebec));
        Assert.IsTrue(result.Any(x => x == Province.Saskatchewan));
    }
}
