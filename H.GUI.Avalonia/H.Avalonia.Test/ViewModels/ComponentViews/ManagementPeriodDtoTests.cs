using H.Core.Factories;
using H.Core.Factories.Animals;

namespace H.Avalonia.Test.ViewModels.ComponentViews
{
    [TestClass]
    public class ManagementPeriodDtoTests
    {
        private ManagementPeriodDto _dto = null!;

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
            _dto = new ManagementPeriodDto();
            _dto.Start = new DateTime(2000, 01, 01);
            _dto.End = new DateTime(2010, 01, 01);
            _dto.Name = "Test Period";
            _dto.NumberOfDays = 365;
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }


        [TestMethod]
        public void TestConstructor()
        {
            Assert.IsNotNull(_dto);
        }

        [TestMethod]
        public void TestValidatePeriodName()
        {
            Assert.IsFalse(_dto.HasErrors);

            _dto.Name = "";
            
            Assert.IsTrue(_dto.HasErrors);
            var errors = _dto.GetErrors(nameof(_dto.Name)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Name cannot be empty.", errors.ToList()[0]);
        }

        [TestMethod]
        public void TestValidateStart()
        {
            Assert.IsFalse(_dto.HasErrors);

            _dto.Start = new DateTime(2020, 01, 01);
            Assert.IsTrue(_dto.HasErrors);

            var errors = _dto.GetErrors(nameof(_dto.Start)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Must be a valid date before the End Date.", errors.ToList()[0]);
        }

        [TestMethod]
        public void TestValidateEnd()
        {
            Assert.IsFalse(_dto.HasErrors);

            _dto.End = new DateTime(1998, 02, 08);
            Assert.IsTrue(_dto.HasErrors);

            var errors = _dto.GetErrors(nameof(_dto.End)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Must be a valid date later than the Start Date.", errors.ToList()[0]);
        }

        [TestMethod]
        public void TestValidateNumberOfDays()
        {
            Assert.IsFalse(_dto.HasErrors);

            _dto.NumberOfDays = -1;
            Assert.IsTrue(_dto.HasErrors);

            var errors = _dto.GetErrors(nameof(_dto.NumberOfDays)) as IEnumerable<string>;
            Assert.IsNotNull (errors);
            Assert.AreEqual("Must be greater than 0.", errors.ToList()[0]);
        }

        #region MilkFatContent Tests

        [TestMethod]
        public void MilkFatContent_DefaultValue_IsCorrect()
        {
            // Arrange & Act
            var dto = new ManagementPeriodDto();

            // Assert
            Assert.AreEqual(3.9, dto.MilkFatContent);
        }

        [TestMethod]
        public void MilkFatContent_ValidValue_NoErrors()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkFatContent = 4.2;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
            Assert.AreEqual(4.2, _dto.MilkFatContent);
        }

        [TestMethod]
        public void MilkFatContent_Negative_HasError()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkFatContent = -1.0;

            // Assert
            Assert.IsTrue(_dto.HasErrors);
            var errors = _dto.GetErrors(nameof(_dto.MilkFatContent)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Milk fat content cannot be negative", errors.First());
        }

        [TestMethod]
        public void MilkFatContent_ExceedsMaximum_HasError()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkFatContent = 15.0;

            // Assert
            Assert.IsTrue(_dto.HasErrors);
            var errors = _dto.GetErrors(nameof(_dto.MilkFatContent)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Milk fat content cannot exceed 10%", errors.First());
        }

        [TestMethod]
        public void MilkFatContent_BoundaryValue_Zero_NoErrors()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkFatContent = 0;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
        }

        [TestMethod]
        public void MilkFatContent_BoundaryValue_Maximum_NoErrors()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkFatContent = 10.0;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
        }

        [TestMethod]
        public void MilkFatContent_TypicalHolstein_NoErrors()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkFatContent = 3.7;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
        }

        [TestMethod]
        public void MilkFatContent_TypicalJersey_NoErrors()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkFatContent = 4.9;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
        }

        #endregion

        #region MilkProteinContentAsPercentage Tests

        [TestMethod]
        public void MilkProteinContent_DefaultValue_IsCorrect()
        {
            // Arrange & Act
            var dto = new ManagementPeriodDto();

            // Assert
            Assert.AreEqual(3.2, dto.MilkProteinContentAsPercentage);
        }

        [TestMethod]
        public void MilkProteinContent_ValidValue_NoErrors()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkProteinContentAsPercentage = 3.5;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
            Assert.AreEqual(3.5, _dto.MilkProteinContentAsPercentage);
        }

        [TestMethod]
        public void MilkProteinContent_Negative_HasError()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkProteinContentAsPercentage = -1.0;

            // Assert
            Assert.IsTrue(_dto.HasErrors);
            var errors = _dto.GetErrors(nameof(_dto.MilkProteinContentAsPercentage)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Milk protein content cannot be negative", errors.First());
        }

        [TestMethod]
        public void MilkProteinContent_ExceedsMaximum_HasError()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkProteinContentAsPercentage = 12.0;

            // Assert
            Assert.IsTrue(_dto.HasErrors);
            var errors = _dto.GetErrors(nameof(_dto.MilkProteinContentAsPercentage)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("Milk protein content cannot exceed 10%", errors.First());
        }

        [TestMethod]
        public void MilkProteinContent_BoundaryValue_Zero_NoErrors()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkProteinContentAsPercentage = 0;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
        }

        [TestMethod]
        public void MilkProteinContent_BoundaryValue_Maximum_NoErrors()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkProteinContentAsPercentage = 10.0;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
        }

        [TestMethod]
        public void MilkProteinContent_TypicalHolstein_NoErrors()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkProteinContentAsPercentage = 3.1;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
        }

        [TestMethod]
        public void MilkProteinContent_TypicalJersey_NoErrors()
        {
            // Arrange
            Assert.IsFalse(_dto.HasErrors);

            // Act
            _dto.MilkProteinContentAsPercentage = 3.8;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
        }

        #endregion

        #region Combined Milk Composition Tests

        [TestMethod]
        public void MilkComposition_HighFatHighProtein_NoErrors()
        {
            // Arrange & Act - Jersey-type milk
            _dto.MilkFatContent = 4.9;
            _dto.MilkProteinContentAsPercentage = 3.8;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
        }

        [TestMethod]
        public void MilkComposition_LowFatLowProtein_NoErrors()
        {
            // Arrange & Act - Holstein-type milk
            _dto.MilkFatContent = 3.5;
            _dto.MilkProteinContentAsPercentage = 3.0;

            // Assert
            Assert.IsFalse(_dto.HasErrors);
        }

        [TestMethod]
        public void MilkComposition_BothInvalid_HasMultipleErrors()
        {
            // Arrange & Act
            _dto.MilkFatContent = -1.0;
            _dto.MilkProteinContentAsPercentage = 15.0;

            // Assert
            Assert.IsTrue(_dto.HasErrors);
            
            var fatErrors = _dto.GetErrors(nameof(_dto.MilkFatContent)) as IEnumerable<string>;
            Assert.IsNotNull(fatErrors);
            
            var proteinErrors = _dto.GetErrors(nameof(_dto.MilkProteinContentAsPercentage)) as IEnumerable<string>;
            Assert.IsNotNull(proteinErrors);
        }

        #endregion
    }
}
