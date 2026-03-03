using H.Core.Enumerations;
using H.Core.Models.Animals;
using H.Core.Providers.Animals;

namespace H.Core.Test.Providers.Animals
{
    [TestClass]
    public class Table_27_Enteric_CH4_Swine_Poultry_OtherLivestock_Provider_Test
    {
        private Table_27_Enteric_CH4_Swine_Poultry_OtherLivestock_Provider _provider = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _provider = new Table_27_Enteric_CH4_Swine_Poultry_OtherLivestock_Provider();
        }

        [TestMethod]
        public void GetAnnualEntericMethaneEmissionRateTest()
        {
            var result = _provider.GetAnnualEntericMethaneEmissionRate(AnimalType.SwineSows, new ManagementPeriod());
            Assert.AreEqual(2.42, result);
        }
    }
}
