using H.Avalonia.ViewModels.ComponentViews.OtherAnimals;
using H.Core;
using H.Core.Enumerations;
using H.Core.Factories.Animals;
using H.Core.Models;
using H.Core.Models.Animals;
using H.Core.Services.Animals;
using H.Core.Services.StorageService;
using Microsoft.Extensions.Logging;
using Moq;

#nullable disable

namespace H.Avalonia.Test.ViewModels.ComponentViews.OtherAnimals
{
    /// <summary>
    /// Tests for <see cref="OtherAnimalsViewModelBase"/> functionality, exercised through
    /// <see cref="HorsesComponentViewModel"/> (a concrete subclass).
    /// Covers: group selection, management period add/remove, HasManagementPeriods,
    /// sequential default dates, and overlapping period validation.
    /// </summary>
    [TestClass]
    public class OtherAnimalsViewModelBaseTests
    {
        #region Fields

        private HorsesComponentViewModel _viewModel = null!;
        private Mock<IStorageService> _mockStorageService = null!;
        private Mock<IStorage> _mockStorage = null!;

        #endregion

        #region Initialization

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
            _mockStorageService = new Mock<IStorageService>();
            _mockStorage = new Mock<IStorage>();
            var applicationData = new ApplicationData();
            _mockStorage.Setup(x => x.ApplicationData).Returns(applicationData);
            _mockStorageService.Setup(x => x.Storage).Returns(_mockStorage.Object);
            var mockLogger = new Mock<ILogger>();
            var mockAnimalComponentService = new Mock<IAnimalComponentService>();
            var mockManagementPeriodService = new Mock<IManagementPeriodService>();

            _viewModel = new HorsesComponentViewModel(
                mockLogger.Object,
                mockAnimalComponentService.Object,
                _mockStorageService.Object,
                mockManagementPeriodService.Object);
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Creates an AnimalGroupDto, adds it to the ViewModel, and selects it.
        /// </summary>
        private AnimalGroupDto AddAndSelectGroup(string name = "Test Group")
        {
            var group = new AnimalGroupDto
            {
                Name = name,
                GroupType = AnimalType.Horses
            };

            _viewModel.AnimalGroupDtos.Add(group);
            _viewModel.SelectAnimalGroupCommand.Execute(group);
            return group;
        }

        #endregion

        #region HasManagementPeriods Tests

        [TestMethod]
        public void HasManagementPeriods_NoGroupSelected_ReturnsFalse()
        {
            // Assert
            Assert.IsFalse(_viewModel.HasManagementPeriods);
        }

        [TestMethod]
        public void HasManagementPeriods_GroupSelectedNoPeriodsAdded_ReturnsFalse()
        {
            // Arrange
            AddAndSelectGroup();

            // Assert
            Assert.IsTrue(_viewModel.IsGroupSelected);
            Assert.IsFalse(_viewModel.HasManagementPeriods);
        }

        [TestMethod]
        public void HasManagementPeriods_AfterAddingPeriod_ReturnsTrue()
        {
            // Arrange
            AddAndSelectGroup();

            // Act
            _viewModel.AddManagementPracticeCommand.Execute(null);

            // Assert
            Assert.IsTrue(_viewModel.HasManagementPeriods);
        }

        [TestMethod]
        public void HasManagementPeriods_AfterRemovingLastPeriod_ReturnsFalse()
        {
            // Arrange
            AddAndSelectGroup();
            _viewModel.AddManagementPracticeCommand.Execute(null);
            Assert.IsTrue(_viewModel.HasManagementPeriods);

            var practice = _viewModel.SelectedManagementPractice;

            // Act
            _viewModel.RemoveManagementPracticeCommand.Execute(practice);

            // Assert
            Assert.IsFalse(_viewModel.HasManagementPeriods);
        }

        #endregion

        #region Group Selection Tests

        [TestMethod]
        public void SelectAnimalGroup_SetsIsGroupSelected()
        {
            // Arrange
            var group = new AnimalGroupDto { Name = "Group 1", GroupType = AnimalType.Horses };
            _viewModel.AnimalGroupDtos.Add(group);

            // Act
            _viewModel.SelectAnimalGroupCommand.Execute(group);

            // Assert
            Assert.IsTrue(_viewModel.IsGroupSelected);
            Assert.AreEqual(group, _viewModel.SelectedAnimalGroup);
        }

        [TestMethod]
        public void SelectAnimalGroup_UpdatesIsSelectedOnGroups()
        {
            // Arrange
            var group1 = new AnimalGroupDto { Name = "Group 1" };
            var group2 = new AnimalGroupDto { Name = "Group 2" };
            _viewModel.AnimalGroupDtos.Add(group1);
            _viewModel.AnimalGroupDtos.Add(group2);

            // Act — select group 1
            _viewModel.SelectAnimalGroupCommand.Execute(group1);

            // Assert
            Assert.IsTrue(group1.IsSelected);
            Assert.IsFalse(group2.IsSelected);

            // Act — switch to group 2
            _viewModel.SelectAnimalGroupCommand.Execute(group2);

            // Assert
            Assert.IsFalse(group1.IsSelected);
            Assert.IsTrue(group2.IsSelected);
        }

        [TestMethod]
        public void SelectAnimalGroup_UpdatesSelectedGroupManagementPeriods()
        {
            // Arrange — two groups with different management periods
            var group1 = new AnimalGroupDto { Name = "Group 1" };
            group1.ManagementPeriodDtos.Add(new ManagementPeriodDto { Name = "G1 Practice 1" });
            var group2 = new AnimalGroupDto { Name = "Group 2" };
            group2.ManagementPeriodDtos.Add(new ManagementPeriodDto { Name = "G2 Practice 1" });
            group2.ManagementPeriodDtos.Add(new ManagementPeriodDto { Name = "G2 Practice 2" });
            _viewModel.AnimalGroupDtos.Add(group1);
            _viewModel.AnimalGroupDtos.Add(group2);

            // Act — select group 1
            _viewModel.SelectAnimalGroupCommand.Execute(group1);
            Assert.AreEqual(1, _viewModel.SelectedGroupManagementPeriods.Count);

            // Act — switch to group 2
            _viewModel.SelectAnimalGroupCommand.Execute(group2);

            // Assert
            Assert.AreEqual(2, _viewModel.SelectedGroupManagementPeriods.Count);
        }

        [TestMethod]
        public void SelectAnimalGroup_AutoSelectsFirstPractice()
        {
            // Arrange
            var group = new AnimalGroupDto { Name = "Group 1" };
            var practice = new ManagementPeriodDto { Name = "Practice 1" };
            group.ManagementPeriodDtos.Add(practice);
            _viewModel.AnimalGroupDtos.Add(group);

            // Act
            _viewModel.SelectAnimalGroupCommand.Execute(group);

            // Assert
            Assert.IsNotNull(_viewModel.SelectedManagementPractice);
            Assert.AreEqual("Practice 1", _viewModel.SelectedManagementPractice.Name);
            Assert.IsTrue(_viewModel.IsPracticeSelected);
        }

        #endregion

        #region Remove Group Tests

        [TestMethod]
        public void RemoveAnimalGroup_RemovesFromCollection()
        {
            // Arrange
            var group = new AnimalGroupDto { Name = "Group 1" };
            _viewModel.AnimalGroupDtos.Add(group);
            Assert.AreEqual(1, _viewModel.AnimalGroupDtos.Count);

            // Act
            _viewModel.RemoveAnimalGroupCommand.Execute(group);

            // Assert
            Assert.AreEqual(0, _viewModel.AnimalGroupDtos.Count);
        }

        [TestMethod]
        public void RemoveAnimalGroup_WasSelected_AutoSelectsNext()
        {
            // Arrange
            var group1 = new AnimalGroupDto { Name = "Group 1" };
            var group2 = new AnimalGroupDto { Name = "Group 2" };
            _viewModel.AnimalGroupDtos.Add(group1);
            _viewModel.AnimalGroupDtos.Add(group2);
            _viewModel.SelectAnimalGroupCommand.Execute(group1);

            // Act — remove the selected group
            _viewModel.RemoveAnimalGroupCommand.Execute(group1);

            // Assert — group2 should be auto-selected
            Assert.AreEqual(group2, _viewModel.SelectedAnimalGroup);
            Assert.IsTrue(_viewModel.IsGroupSelected);
        }

        [TestMethod]
        public void RemoveAnimalGroup_LastGroup_ClearsSelection()
        {
            // Arrange
            var group = AddAndSelectGroup();

            // Act
            _viewModel.RemoveAnimalGroupCommand.Execute(group);

            // Assert
            Assert.IsNull(_viewModel.SelectedAnimalGroup);
            Assert.IsFalse(_viewModel.IsGroupSelected);
        }

        #endregion

        #region Add Management Practice Tests

        [TestMethod]
        public void AddManagementPractice_FirstPeriod_DefaultsToCurrentYearDates()
        {
            // Arrange
            AddAndSelectGroup();

            // Act
            _viewModel.AddManagementPracticeCommand.Execute(null);

            // Assert
            var practice = _viewModel.SelectedGroupManagementPeriods[0];
            Assert.AreEqual(new DateTime(DateTime.Now.Year, 1, 1), practice.Start);
            Assert.AreEqual(new DateTime(DateTime.Now.Year, 12, 31), practice.End);
        }

        [TestMethod]
        public void AddManagementPractice_FirstPeriod_NamedPractice1()
        {
            // Arrange
            AddAndSelectGroup();

            // Act
            _viewModel.AddManagementPracticeCommand.Execute(null);

            // Assert
            Assert.AreEqual("Practice 1", _viewModel.SelectedGroupManagementPeriods[0].Name);
        }

        [TestMethod]
        public void AddManagementPractice_SubsequentPeriod_StartsAfterPreviousEnd()
        {
            // Arrange
            AddAndSelectGroup();
            _viewModel.AddManagementPracticeCommand.Execute(null);

            var firstPractice = _viewModel.SelectedGroupManagementPeriods[0];
            var firstEnd = firstPractice.End;

            // Act
            _viewModel.AddManagementPracticeCommand.Execute(null);

            // Assert — second practice starts where the first one ends
            var secondPractice = _viewModel.SelectedGroupManagementPeriods[1];
            Assert.AreEqual(firstEnd, secondPractice.Start);
        }

        [TestMethod]
        public void AddManagementPractice_SubsequentPeriod_DefaultDuration365Days()
        {
            // Arrange
            AddAndSelectGroup();
            _viewModel.AddManagementPracticeCommand.Execute(null);

            var firstPractice = _viewModel.SelectedGroupManagementPeriods[0];
            var firstEnd = firstPractice.End;

            // Act
            _viewModel.AddManagementPracticeCommand.Execute(null);

            // Assert — second practice has 365-day duration
            var secondPractice = _viewModel.SelectedGroupManagementPeriods[1];
            Assert.AreEqual(firstEnd.AddDays(365), secondPractice.End);
        }

        [TestMethod]
        public void AddManagementPractice_SequentialNames()
        {
            // Arrange
            AddAndSelectGroup();

            // Act
            _viewModel.AddManagementPracticeCommand.Execute(null);
            _viewModel.AddManagementPracticeCommand.Execute(null);
            _viewModel.AddManagementPracticeCommand.Execute(null);

            // Assert
            Assert.AreEqual("Practice 1", _viewModel.SelectedGroupManagementPeriods[0].Name);
            Assert.AreEqual("Practice 2", _viewModel.SelectedGroupManagementPeriods[1].Name);
            Assert.AreEqual("Practice 3", _viewModel.SelectedGroupManagementPeriods[2].Name);
        }

        [TestMethod]
        public void AddManagementPractice_AutoSelectsNewPractice()
        {
            // Arrange
            AddAndSelectGroup();

            // Act
            _viewModel.AddManagementPracticeCommand.Execute(null);

            // Assert
            Assert.IsNotNull(_viewModel.SelectedManagementPractice);
            Assert.AreEqual("Practice 1", _viewModel.SelectedManagementPractice.Name);
        }

        [TestMethod]
        public void AddManagementPractice_ThirdPeriod_StartsAfterSecondEnd()
        {
            // Arrange
            AddAndSelectGroup();
            _viewModel.AddManagementPracticeCommand.Execute(null);
            _viewModel.AddManagementPracticeCommand.Execute(null);

            var secondEnd = _viewModel.SelectedGroupManagementPeriods[1].End;

            // Act
            _viewModel.AddManagementPracticeCommand.Execute(null);

            // Assert
            var thirdPractice = _viewModel.SelectedGroupManagementPeriods[2];
            Assert.AreEqual(secondEnd, thirdPractice.Start);
        }

        [TestMethod]
        public void AddManagementPractice_NoOverlapByDefault()
        {
            // Arrange
            AddAndSelectGroup();

            // Act — add multiple periods
            _viewModel.AddManagementPracticeCommand.Execute(null);
            _viewModel.AddManagementPracticeCommand.Execute(null);
            _viewModel.AddManagementPracticeCommand.Execute(null);

            // Assert — none should have overlap errors
            foreach (var practice in _viewModel.SelectedGroupManagementPeriods)
            {
                Assert.IsFalse(practice.HasErrors,
                    $"Practice '{practice.Name}' should not have errors but has: {practice.ValidationSummary}");
            }
        }

        #endregion

        #region Remove Management Practice Tests

        [TestMethod]
        public void RemoveManagementPractice_RemovesFromCollection()
        {
            // Arrange
            AddAndSelectGroup();
            _viewModel.AddManagementPracticeCommand.Execute(null);
            _viewModel.AddManagementPracticeCommand.Execute(null);
            Assert.AreEqual(2, _viewModel.SelectedGroupManagementPeriods.Count);

            var practiceToRemove = _viewModel.SelectedGroupManagementPeriods[0];

            // Act
            _viewModel.RemoveManagementPracticeCommand.Execute(practiceToRemove);

            // Assert
            Assert.AreEqual(1, _viewModel.SelectedGroupManagementPeriods.Count);
            Assert.IsFalse(_viewModel.SelectedGroupManagementPeriods.Contains(practiceToRemove));
        }

        [TestMethod]
        public void RemoveManagementPractice_WasSelected_AutoSelectsNext()
        {
            // Arrange
            AddAndSelectGroup();
            _viewModel.AddManagementPracticeCommand.Execute(null);
            _viewModel.AddManagementPracticeCommand.Execute(null);

            // Select the first practice
            var first = _viewModel.SelectedGroupManagementPeriods[0];
            var second = _viewModel.SelectedGroupManagementPeriods[1];
            _viewModel.SelectedManagementPractice = first;

            // Act — remove the selected one
            _viewModel.RemoveManagementPracticeCommand.Execute(first);

            // Assert — the remaining practice should be selected
            Assert.AreEqual(second, _viewModel.SelectedManagementPractice);
        }

        [TestMethod]
        public void RemoveManagementPractice_NullParameter_DoesNothing()
        {
            // Arrange
            AddAndSelectGroup();
            _viewModel.AddManagementPracticeCommand.Execute(null);
            var countBefore = _viewModel.SelectedGroupManagementPeriods.Count;

            // Act
            _viewModel.RemoveManagementPracticeCommand.Execute(null);

            // Assert
            Assert.AreEqual(countBefore, _viewModel.SelectedGroupManagementPeriods.Count);
        }

        #endregion

        #region Overlap Validation Tests

        [TestMethod]
        public void OverlapValidation_SinglePeriod_NoErrors()
        {
            // Arrange
            AddAndSelectGroup();

            // Act
            _viewModel.AddManagementPracticeCommand.Execute(null);

            // Assert — a single period cannot overlap with anything
            var practice = _viewModel.SelectedGroupManagementPeriods[0];
            Assert.IsFalse(practice.HasErrors);
        }

        [TestMethod]
        public void OverlapValidation_NonOverlappingPeriods_NoErrors()
        {
            // Arrange
            var group = AddAndSelectGroup();

            // Add two non-overlapping periods manually
            var practice1 = new ManagementPeriodDto
            {
                Name = "Practice 1",
                Start = new DateTime(2026, 1, 1),
                End = new DateTime(2026, 6, 30),
                NumberOfDays = 180,
                NumberOfAnimals = 20
            };
            var practice2 = new ManagementPeriodDto
            {
                Name = "Practice 2",
                Start = new DateTime(2026, 7, 1),
                End = new DateTime(2026, 12, 31),
                NumberOfDays = 183,
                NumberOfAnimals = 20
            };

            group.ManagementPeriodDtos.Add(practice1);
            group.ManagementPeriodDtos.Add(practice2);

            // Re-select the group to trigger validation
            _viewModel.SelectAnimalGroupCommand.Execute(group);

            // Assert — no overlap errors on either period
            var practice1Errors = practice1.GetErrors("DateOverlap") as IEnumerable<string>;
            var practice2Errors = practice2.GetErrors("DateOverlap") as IEnumerable<string>;
            Assert.IsNull(practice1Errors);
            Assert.IsNull(practice2Errors);
        }

        [TestMethod]
        public void OverlapValidation_OverlappingPeriods_HasErrors()
        {
            // Arrange — build group with overlapping periods BEFORE selecting
            var group = new AnimalGroupDto { Name = "Test Group", GroupType = AnimalType.Horses };

            var practice1 = new ManagementPeriodDto
            {
                Name = "Practice 1",
                Start = new DateTime(2026, 1, 1),
                End = new DateTime(2026, 8, 1),
                NumberOfDays = 212,
                NumberOfAnimals = 20
            };
            var practice2 = new ManagementPeriodDto
            {
                Name = "Practice 2",
                Start = new DateTime(2026, 6, 1),
                End = new DateTime(2026, 12, 31),
                NumberOfDays = 213,
                NumberOfAnimals = 20
            };

            group.ManagementPeriodDtos.Add(practice1);
            group.ManagementPeriodDtos.Add(practice2);
            _viewModel.AnimalGroupDtos.Add(group);

            // Act — first-time selection triggers UpdateSelectedGroupManagementPeriods + validation
            _viewModel.SelectAnimalGroupCommand.Execute(group);

            // Assert — both should have overlap errors
            Assert.IsTrue(practice1.HasErrors);
            Assert.IsTrue(practice2.HasErrors);
            Assert.IsTrue(practice1.ValidationSummary.Contains("Practice 2"));
            Assert.IsTrue(practice2.ValidationSummary.Contains("Practice 1"));
        }

        [TestMethod]
        public void OverlapValidation_FixedOverlap_ClearsErrors()
        {
            // Arrange — create overlapping periods
            var group = AddAndSelectGroup();

            // Use AddManagementPracticeCommand for first two (non-overlapping by default)
            _viewModel.AddManagementPracticeCommand.Execute(null);
            _viewModel.AddManagementPracticeCommand.Execute(null);

            var practice1 = _viewModel.SelectedGroupManagementPeriods[0];
            var practice2 = _viewModel.SelectedGroupManagementPeriods[1];

            // Create overlap by setting practice2 start before practice1 end
            practice2.Start = practice1.Start;

            // Verify overlap is detected
            Assert.IsTrue(practice1.HasErrors || practice2.HasErrors,
                "At least one practice should have overlap errors");

            // Act — fix the overlap by setting practice2 start after practice1 end
            practice2.Start = practice1.End;

            // Assert — errors should be cleared
            var p1Errors = practice1.GetErrors("DateOverlap") as IEnumerable<string>;
            var p2Errors = practice2.GetErrors("DateOverlap") as IEnumerable<string>;
            Assert.IsNull(p1Errors);
            Assert.IsNull(p2Errors);
        }

        [TestMethod]
        public void OverlapValidation_DateChangeTriggersRevalidation()
        {
            // Arrange — start with non-overlapping periods
            AddAndSelectGroup();
            _viewModel.AddManagementPracticeCommand.Execute(null);
            _viewModel.AddManagementPracticeCommand.Execute(null);

            var practice1 = _viewModel.SelectedGroupManagementPeriods[0];
            var practice2 = _viewModel.SelectedGroupManagementPeriods[1];

            // Verify initially no overlap
            var p1Errors = practice1.GetErrors("DateOverlap") as IEnumerable<string>;
            Assert.IsNull(p1Errors);

            // Act — extend practice1's end date to overlap with practice2
            practice1.End = practice2.End;

            // Assert — now there should be overlap errors
            Assert.IsTrue(practice1.HasErrors);
            Assert.IsTrue(practice2.HasErrors);
        }

        [TestMethod]
        public void OverlapValidation_RemovingOverlappingPeriod_ClearsErrors()
        {
            // Arrange — create overlapping periods, then select group to trigger validation
            var group = new AnimalGroupDto { Name = "Test Group", GroupType = AnimalType.Horses };

            var practice1 = new ManagementPeriodDto
            {
                Name = "Practice 1",
                Start = new DateTime(2026, 1, 1),
                End = new DateTime(2026, 4, 1),
                NumberOfDays = 90,
                NumberOfAnimals = 20
            };
            var practice2 = new ManagementPeriodDto
            {
                Name = "Practice 2",
                Start = new DateTime(2026, 3, 1),
                End = new DateTime(2026, 6, 1),
                NumberOfDays = 92,
                NumberOfAnimals = 20
            };

            group.ManagementPeriodDtos.Add(practice1);
            group.ManagementPeriodDtos.Add(practice2);
            _viewModel.AnimalGroupDtos.Add(group);

            // Select group — triggers overlap validation
            _viewModel.SelectAnimalGroupCommand.Execute(group);

            // Verify overlap exists
            Assert.IsTrue(practice1.HasErrors);

            // Act — remove the overlapping practice
            _viewModel.RemoveManagementPracticeCommand.Execute(practice2);

            // Assert — practice1 should now be error-free
            var p1Errors = practice1.GetErrors("DateOverlap") as IEnumerable<string>;
            Assert.IsNull(p1Errors);
        }

        [TestMethod]
        public void OverlapValidation_MultipleOverlaps_AllDetected()
        {
            // Arrange — three periods, all overlapping with each other
            var group = new AnimalGroupDto { Name = "Test Group", GroupType = AnimalType.Horses };

            var practice1 = new ManagementPeriodDto
            {
                Name = "Practice 1",
                Start = new DateTime(2026, 1, 1),
                End = new DateTime(2026, 12, 31),
                NumberOfDays = 364,
                NumberOfAnimals = 20
            };
            var practice2 = new ManagementPeriodDto
            {
                Name = "Practice 2",
                Start = new DateTime(2026, 3, 1),
                End = new DateTime(2026, 9, 1),
                NumberOfDays = 184,
                NumberOfAnimals = 20
            };
            var practice3 = new ManagementPeriodDto
            {
                Name = "Practice 3",
                Start = new DateTime(2026, 6, 1),
                End = new DateTime(2027, 1, 1),
                NumberOfDays = 214,
                NumberOfAnimals = 20
            };

            group.ManagementPeriodDtos.Add(practice1);
            group.ManagementPeriodDtos.Add(practice2);
            group.ManagementPeriodDtos.Add(practice3);
            _viewModel.AnimalGroupDtos.Add(group);

            // Select group — triggers overlap validation
            _viewModel.SelectAnimalGroupCommand.Execute(group);

            // Assert — all three should have overlap errors
            Assert.IsTrue(practice1.HasErrors);
            Assert.IsTrue(practice2.HasErrors);
            Assert.IsTrue(practice3.HasErrors);
        }

        #endregion

        #region Group Switching with Management Periods Tests

        [TestMethod]
        public void SwitchGroup_ManagementPeriodsUpdateToNewGroup()
        {
            // Arrange — two groups with different periods
            var group1 = new AnimalGroupDto { Name = "Group 1" };
            group1.ManagementPeriodDtos.Add(new ManagementPeriodDto { Name = "G1P1" });

            var group2 = new AnimalGroupDto { Name = "Group 2" };
            group2.ManagementPeriodDtos.Add(new ManagementPeriodDto { Name = "G2P1" });
            group2.ManagementPeriodDtos.Add(new ManagementPeriodDto { Name = "G2P2" });
            group2.ManagementPeriodDtos.Add(new ManagementPeriodDto { Name = "G2P3" });

            _viewModel.AnimalGroupDtos.Add(group1);
            _viewModel.AnimalGroupDtos.Add(group2);

            // Act — select group 1 then switch to group 2
            _viewModel.SelectAnimalGroupCommand.Execute(group1);
            Assert.AreEqual(1, _viewModel.SelectedGroupManagementPeriods.Count);

            _viewModel.SelectAnimalGroupCommand.Execute(group2);

            // Assert
            Assert.AreEqual(3, _viewModel.SelectedGroupManagementPeriods.Count);
            Assert.AreEqual("G2P1", _viewModel.SelectedManagementPractice.Name);
        }

        [TestMethod]
        public void SwitchGroup_ClearsSelectedPracticeWhenNewGroupHasNoPeriods()
        {
            // Arrange
            var group1 = new AnimalGroupDto { Name = "Group 1" };
            group1.ManagementPeriodDtos.Add(new ManagementPeriodDto { Name = "G1P1" });
            var group2 = new AnimalGroupDto { Name = "Group 2" };

            _viewModel.AnimalGroupDtos.Add(group1);
            _viewModel.AnimalGroupDtos.Add(group2);

            _viewModel.SelectAnimalGroupCommand.Execute(group1);
            Assert.IsNotNull(_viewModel.SelectedManagementPractice);

            // Act — switch to empty group
            _viewModel.SelectAnimalGroupCommand.Execute(group2);

            // Assert
            Assert.IsNull(_viewModel.SelectedManagementPractice);
            Assert.IsFalse(_viewModel.IsPracticeSelected);
            Assert.IsFalse(_viewModel.HasManagementPeriods);
        }

        #endregion

        #region Enum Collection Initialization Tests

        [TestMethod]
        public void Constructor_InitializesManureStateTypes()
        {
            Assert.IsNotNull(_viewModel.ManureStateTypes);
            Assert.IsTrue(_viewModel.ManureStateTypes.Any());
        }

        [TestMethod]
        public void Constructor_InitializesHousingTypes()
        {
            Assert.IsNotNull(_viewModel.HousingTypes);
            Assert.IsTrue(_viewModel.HousingTypes.Any());
        }

        [TestMethod]
        public void Constructor_InitializesBeddingMaterialTypes()
        {
            Assert.IsNotNull(_viewModel.BeddingMaterialTypes);
            Assert.IsTrue(_viewModel.BeddingMaterialTypes.Any());
        }

        [TestMethod]
        public void Constructor_InitializesDietAdditiveTypes()
        {
            Assert.IsNotNull(_viewModel.DietAdditiveTypes);
            Assert.IsTrue(_viewModel.DietAdditiveTypes.Any());
        }

        #endregion
    }
}
