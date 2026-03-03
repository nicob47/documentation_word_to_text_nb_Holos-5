using H.Core.Converters;

namespace H.Core.Test.Converters
{
    [TestClass]
    public class ConverterBaseTest
    {
        private CropTypeStringConverter _converter = null!;

        [TestInitialize]
        public void Initialize()
        {
            _converter = new CropTypeStringConverter();
        }

        [TestMethod]
        public void GetLettersAsLowerCaseTest()
        {
            var result = _converter.Convert("Peas, dry");

            Assert.AreEqual(result, Enumerations.CropType.DryPeas) ;
        }
    }
}
