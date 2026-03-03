using H.Avalonia.ViewModels.OptionsViews.DataTransferObjects;
using H.Core.Calculators.UnitsOfMeasurement;
using H.Core.Enumerations;
using H.Core.Providers.Animals;
using Moq;

namespace H.Avalonia.Test.ViewModels.OptionsViews.DataTransferObjects
{
    [TestClass]
    public class DefaultBeddingCompositionDTOTests
    {
        private DefaultBeddingCompositionDTO _viewModel = null!;
        private Mock<IUnitsOfMeasurementCalculator> _mockUnitsCalculator = null!;
        private IUnitsOfMeasurementCalculator _unitsCalculatorMock = null!;

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
            _mockUnitsCalculator = new Mock<IUnitsOfMeasurementCalculator>();
            _unitsCalculatorMock = _mockUnitsCalculator.Object;
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void TestGetterAndSetterWhenMetric()
        {
            _mockUnitsCalculator.Setup(x => x.IsMetric).Returns(true);
            var testDataClassInstance = new Table_30_Default_Bedding_Material_Composition_Data();
            _viewModel = new DefaultBeddingCompositionDTO(testDataClassInstance, _unitsCalculatorMock);
            _viewModel.SetInitializationFlag(false);

            _viewModel.TotalNitrogenKilogramsDryMatter = 0.005;

            // Getter should return backing field as is (i.e. in metric units)
            Assert.AreEqual(0.005, _viewModel.TotalNitrogenKilogramsDryMatter);
            // Setter should store in metric units / update data class instance properly
            Assert.AreEqual(0.005, testDataClassInstance.TotalNitrogenKilogramsDryMatter);
        }

        [TestMethod]
        public void TestGetterAndSetterWhenImperial()
        {
            _mockUnitsCalculator.Setup(x => x.IsMetric).Returns(false);
            _mockUnitsCalculator.Setup(x => x.GetUnitsOfMeasurementValue(MeasurementSystemType.Metric, ImperialUnitsOfMeasurement.Pounds, 10)).Returns(4.535147);
            _mockUnitsCalculator.Setup(x => x.GetUnitsOfMeasurementValue(MeasurementSystemType.Imperial, MetricUnitsOfMeasurement.Kilograms, 4.535147)).Returns(10);
            var testDataClassInstance = new Table_30_Default_Bedding_Material_Composition_Data();
            _viewModel = new DefaultBeddingCompositionDTO(testDataClassInstance, _unitsCalculatorMock);
            _viewModel.SetInitializationFlag(false);

            _viewModel.TotalNitrogenKilogramsDryMatter = 10;

            // Getter should return backing field in imperial units (matching input)
            Assert.AreEqual(10, _viewModel.TotalNitrogenKilogramsDryMatter);
            // Setter should store input (and update data class instance) in metric units 
            Assert.AreEqual(4.535147, testDataClassInstance.TotalNitrogenKilogramsDryMatter);
        }

        [TestMethod]
        public void TestSetterDuringInitializationMetric()
        {
            _mockUnitsCalculator.Setup(x => x.IsMetric).Returns(true);
            var testDataClassInstance = new Table_30_Default_Bedding_Material_Composition_Data();
            _viewModel = new DefaultBeddingCompositionDTO(testDataClassInstance, _unitsCalculatorMock);
            _viewModel.SetInitializationFlag(true);

            _viewModel.TotalNitrogenKilogramsDryMatter = 7.5;

            Assert.AreEqual(7.5, _viewModel.TotalNitrogenKilogramsDryMatter);
            // Data class instance should not be updated when initialization occurs
            Assert.AreEqual(0, testDataClassInstance.TotalNitrogenKilogramsDryMatter);
        }

        [TestMethod]
        public void TestSetterDuringInitializationImperial()
        {
            _mockUnitsCalculator.Setup(x => x.IsMetric).Returns(false);
            _mockUnitsCalculator.Setup(x => x.GetUnitsOfMeasurementValue(MeasurementSystemType.Imperial, MetricUnitsOfMeasurement.Kilograms, 0.375)).Returns(0.8269);
            var testDataClassInstance = new Table_30_Default_Bedding_Material_Composition_Data();
            _viewModel = new DefaultBeddingCompositionDTO(testDataClassInstance, _unitsCalculatorMock);
            _viewModel.SetInitializationFlag(true);

            _viewModel.TotalNitrogenKilogramsDryMatter = 0.375;
            
            // There should be no method call to convert to metric units 
            _mockUnitsCalculator.Verify(x => x.GetUnitsOfMeasurementValue(MeasurementSystemType.Metric, ImperialUnitsOfMeasurement.Pounds, It.IsAny<Double>()), Times.Never());
            // Data class instance should not be updated when initialization occurs
            Assert.AreEqual(0, testDataClassInstance.TotalNitrogenKilogramsDryMatter);
            // Getter should return the input (metric) converted to imperial units
            Assert.AreEqual(0.8269, _viewModel.TotalNitrogenKilogramsDryMatter);
        }

        [TestMethod]
        public void TestValidateNumericPropertyWithNegative()
        {
            _mockUnitsCalculator.Setup(x => x.IsMetric).Returns(true);
            var testDataClassInstance = new Table_30_Default_Bedding_Material_Composition_Data();
            _viewModel = new DefaultBeddingCompositionDTO(testDataClassInstance, _unitsCalculatorMock);
            // Initially set a property to a valid value
            _viewModel.TotalPhosphorusKilogramsDryMatter = 2;
            Assert.IsTrue(!_viewModel.HasErrors);

            _viewModel.TotalPhosphorusKilogramsDryMatter = -7;
            
            // Data class instance should not be updated with invalid values
            Assert.AreEqual(2, testDataClassInstance.TotalPhosphorusKilogramsDryMatter);
            Assert.IsTrue(_viewModel.HasErrors);
            var errors = _viewModel.GetErrors(nameof(_viewModel.TotalPhosphorusKilogramsDryMatter)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Must be greater than or equal to 0.", errors.ToList()[0]);
        }

        [TestMethod]
        public void TestValidateNumericPropertyWithZero()
        {
            _mockUnitsCalculator.Setup(x => x.IsMetric).Returns(true);
            var testDataClassInstance = new Table_30_Default_Bedding_Material_Composition_Data();
            _viewModel = new DefaultBeddingCompositionDTO(testDataClassInstance, _unitsCalculatorMock);

            _viewModel.TotalPhosphorusKilogramsDryMatter = 0;
            Assert.IsTrue(!_viewModel.HasErrors);

            Assert.AreEqual(0, _viewModel.TotalPhosphorusKilogramsDryMatter);
            Assert.AreEqual(0, testDataClassInstance.TotalPhosphorusKilogramsDryMatter);
        }

        [TestMethod]
        public void TestValidateNumericPropertyWithPositive()
        {
            _mockUnitsCalculator.Setup(x => x.IsMetric).Returns(true);
            var testDataClassInstance = new Table_30_Default_Bedding_Material_Composition_Data();
            _viewModel = new DefaultBeddingCompositionDTO(testDataClassInstance, _unitsCalculatorMock);

            _viewModel.TotalPhosphorusKilogramsDryMatter = 12;
            Assert.IsTrue(!_viewModel.HasErrors);

            Assert.AreEqual(12, _viewModel.TotalPhosphorusKilogramsDryMatter);
            Assert.AreEqual(12, testDataClassInstance.TotalPhosphorusKilogramsDryMatter);
        }
    }
}
