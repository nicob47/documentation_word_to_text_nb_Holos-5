using H.Avalonia.ViewModels.OptionsViews.DataTransferObjects;
using H.Core.Providers.Animals;

namespace H.Avalonia.Test.ViewModels.OptionsViews.DataTransferObjects
{
    [TestClass]
    public class DefaultManureCompositionDTOTests
    {
        private DefaultManureCompositionData _dataClassInstance = null!;
        private DefaultManureCompositionDTO _manureCompositionDTO = null!;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestInitialize]
        public void TestInitialize()
        {
            _dataClassInstance = _dataClassInstance = new DefaultManureCompositionData();
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestValidateNumericPropertyCorrectInput()
        {
            _manureCompositionDTO = new DefaultManureCompositionDTO(_dataClassInstance);

            _manureCompositionDTO.CarbonFraction = 5.62;

            Assert.IsTrue(!_manureCompositionDTO.HasErrors);
        }

        [TestMethod]
        public void TestValidateNumericPropertyBadInput()
        {
            _manureCompositionDTO = new DefaultManureCompositionDTO(_dataClassInstance);
            _manureCompositionDTO.CarbonFraction = 2.05;
            Assert.AreEqual(_manureCompositionDTO.CarbonFraction, _dataClassInstance.CarbonFraction);

            _manureCompositionDTO.CarbonFraction = -9.72;

            Assert.IsTrue(_manureCompositionDTO.HasErrors);
            Assert.AreEqual(2.05, _dataClassInstance.CarbonFraction);
        }
    }
}
