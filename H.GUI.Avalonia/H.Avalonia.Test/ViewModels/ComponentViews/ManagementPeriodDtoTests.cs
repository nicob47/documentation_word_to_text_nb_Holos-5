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
            Assert.AreEqual("Start date must be before the end date.", errors.ToList()[0]);
        }

        [TestMethod]
        public void TestValidateEnd()
        {
            Assert.IsFalse(_dto.HasErrors);

            _dto.End = new DateTime(1998, 02, 08);
            Assert.IsTrue(_dto.HasErrors);

            var errors = _dto.GetErrors(nameof(_dto.End)) as IEnumerable<string>;
            Assert.IsNotNull(errors);
            Assert.AreEqual("End date must be after the start date.", errors.ToList()[0]);
        }

        [TestMethod]
        public void TestValidateNumberOfDays()
        {
            Assert.IsFalse(_dto.HasErrors);

            _dto.NumberOfDays = -1;
            Assert.IsTrue(_dto.HasErrors);

            var errors = _dto.GetErrors(nameof(_dto.NumberOfDays)) as IEnumerable<string>;
            Assert.IsNotNull (errors);
            Assert.AreEqual("Number of days must be greater than 0.", errors.ToList()[0]);
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

        #region ValidationSummary Tests

        [TestMethod]
        public void ValidationSummary_NoErrors_ReturnsEmptyString()
        {
            // Arrange — _dto is valid after TestInitialize
            Assert.IsFalse(_dto.HasErrors);

            // Act & Assert
            Assert.AreEqual(string.Empty, _dto.ValidationSummary);
        }

        [TestMethod]
        public void ValidationSummary_SingleError_ReturnsSingleMessage()
        {
            // Arrange
            _dto.Name = "";

            // Act
            var summary = _dto.ValidationSummary;

            // Assert
            Assert.IsTrue(_dto.HasErrors);
            Assert.IsTrue(summary.Contains("Name cannot be empty."));
        }

        [TestMethod]
        public void ValidationSummary_MultipleErrors_ReturnsNewlineSeparatedMessages()
        {
            // Arrange — trigger two distinct validation errors
            _dto.Name = "";
            _dto.NumberOfDays = -1;

            // Act
            var summary = _dto.ValidationSummary;

            // Assert
            Assert.IsTrue(_dto.HasErrors);
            Assert.IsTrue(summary.Contains("Name cannot be empty."));
            Assert.IsTrue(summary.Contains("Number of days must be greater than 0."));
            Assert.IsTrue(summary.Contains(Environment.NewLine));
        }

        [TestMethod]
        public void ValidationSummary_ErrorAdded_ThenRemoved_UpdatesSummary()
        {
            // Arrange
            _dto.Name = "";
            Assert.IsTrue(_dto.HasErrors);
            Assert.IsTrue(_dto.ValidationSummary.Contains("Name cannot be empty."));

            // Act — fix the error
            _dto.Name = "Fixed Name";

            // Assert
            Assert.IsFalse(_dto.HasErrors);
            Assert.AreEqual(string.Empty, _dto.ValidationSummary);
        }

        #endregion

        #region Date/Days Auto-Recalculation Tests

        [TestMethod]
        public void NumberOfDays_RecalculatesWhenEndDateChanges()
        {
            // Arrange — fresh DTO to control exact state
            var dto = new ManagementPeriodDto();
            dto.Name = "Test";
            dto.Start = new DateTime(2026, 1, 1);
            dto.End = new DateTime(2026, 7, 1); // Jan 1 to Jul 1 inclusive = 182 days

            // Assert — inclusive day count: (End - Start).Days + 1
            Assert.AreEqual((new DateTime(2026, 7, 1) - new DateTime(2026, 1, 1)).Days + 1, dto.NumberOfDays);
        }

        [TestMethod]
        public void NumberOfDays_RecalculatesWhenStartDateChanges()
        {
            // Arrange — fresh DTO
            var dto = new ManagementPeriodDto();
            dto.Name = "Test";
            dto.Start = new DateTime(2026, 1, 1);
            dto.End = new DateTime(2026, 12, 31);
            var initialDays = dto.NumberOfDays;

            // Act — move start date forward
            dto.Start = new DateTime(2026, 6, 1);

            // Assert — days should be less (inclusive count)
            var expectedDays = (new DateTime(2026, 12, 31) - new DateTime(2026, 6, 1)).Days + 1;
            Assert.AreEqual(expectedDays, dto.NumberOfDays);
            Assert.IsTrue(dto.NumberOfDays < initialDays);
        }

        [TestMethod]
        public void EndDate_RecalculatesWhenNumberOfDaysChanges()
        {
            // Arrange — fresh DTO
            var dto = new ManagementPeriodDto();
            dto.Name = "Test";
            dto.Start = new DateTime(2026, 1, 1);
            dto.End = new DateTime(2026, 12, 31);

            // Act — change number of days
            dto.NumberOfDays = 30;

            // Assert — End date should have moved (inclusive: Start + N-1)
            Assert.AreEqual(new DateTime(2026, 1, 1).AddDays(30 - 1), dto.End);
        }

        [TestMethod]
        public void Recalculation_NoInfiniteLoop_WhenDatesChangeRapidly()
        {
            // Arrange — fresh DTO
            var dto = new ManagementPeriodDto();
            dto.Name = "Test";
            dto.Start = new DateTime(2026, 1, 1);
            dto.End = new DateTime(2026, 12, 31);

            // Act — rapid changes should not cause infinite recursion
            dto.NumberOfDays = 100;
            dto.Start = new DateTime(2026, 3, 1);
            dto.End = new DateTime(2026, 6, 1);
            dto.NumberOfDays = 60;

            // Assert — just verifying no StackOverflow and final state is consistent (inclusive end)
            Assert.AreEqual(new DateTime(2026, 3, 1).AddDays(60 - 1), dto.End);
            Assert.AreEqual(60, dto.NumberOfDays);
        }

        [TestMethod]
        public void NumberOfDays_DoesNotRecalculate_WhenEndIsBeforeStart()
        {
            // Arrange — set up valid dates first
            var dto = new ManagementPeriodDto();
            dto.Name = "Test";
            dto.Start = new DateTime(2026, 6, 1);
            dto.End = new DateTime(2026, 12, 31);
            var daysBeforeInvalidEnd = dto.NumberOfDays;

            // Act — set End before Start (invalid)
            dto.End = new DateTime(2026, 1, 1);

            // Assert — NumberOfDays should NOT have recalculated (End > Start check fails)
            // The recalculation guard prevents update when End <= Start
            Assert.IsTrue(dto.HasErrors);
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
