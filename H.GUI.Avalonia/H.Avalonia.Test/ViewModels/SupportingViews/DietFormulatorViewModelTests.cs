using H.Avalonia.ViewModels.SupportingViews;
using H.Core.Providers.Feed;
using H.Core.Services.DietService;
using H.Core.Services.StorageService;
using H.Core.Enumerations;
using Moq;
using Prism.Regions;

namespace H.Avalonia.Test.ViewModels.SupportingViews
{
    [TestClass]
    public class DietFormulatorViewModelTests
    {
        private DietFormulatorViewModel _viewModel = null!;
        private Mock<IDietService> _mockDietService = null!;
        private Mock<IRegionManager> _mockRegionManager = null!;
        private Mock<IStorageService> _mockStorageService = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            // Arrange
            _mockDietService = new Mock<IDietService>();
            _mockRegionManager = new Mock<IRegionManager>();
            _mockStorageService = new Mock<IStorageService>();

            // Create mock diet DTOs
            var mockDiets = new List<IDietDto>
            {
                new DietDto 
                { 
                    Name = "Test Diet 1", 
                    AnimalType = AnimalType.Beef, 
                    DietType = DietType.HighEnergy,
                    CrudeProtein = 12.5,
                    TotalDigestibleNutrient = 85.0
                },
                new DietDto 
                { 
                    Name = "Test Diet 2", 
                    AnimalType = AnimalType.Dairy, 
                    DietType = DietType.MediumEnergy,
                    CrudeProtein = 15.2,
                    TotalDigestibleNutrient = 75.0
                },
                new DietDto 
                { 
                    Name = "Swine Diet A", 
                    AnimalType = AnimalType.Swine, 
                    DietType = DietType.Gestation,
                    CrudeProtein = 14.3,
                    TotalDigestibleNutrient = 80.0
                }
            };

            _mockDietService.Setup(x => x.GetDiets()).Returns(mockDiets);

            _viewModel = new DietFormulatorViewModel(_mockRegionManager.Object, _mockStorageService.Object, _mockDietService.Object);
        }

        [TestMethod]
        public void Constructor_ShouldLoadDiets()
        {
            // Assert
            Assert.IsNotNull(_viewModel.Diets);
            Assert.AreEqual(3, _viewModel.Diets.Count);
            Assert.AreEqual("Test Diet 1", _viewModel.Diets.First().Name);
        }

        [TestMethod]
        public void SearchText_FiltersByName_ShouldReturnMatchingDiets()
        {
            // Act
            _viewModel.SearchText = "Test";

            // Assert
            Assert.AreEqual(2, _viewModel.Diets.Count);
            Assert.IsTrue(_viewModel.Diets.All(d => d.Name.Contains("Test")));
        }

        [TestMethod]
        public void SearchText_FiltersByAnimalType_ShouldReturnMatchingDiets()
        {
            // Act
            _viewModel.SearchText = "Swine";

            // Assert
            Assert.AreEqual(1, _viewModel.Diets.Count);
            Assert.AreEqual(AnimalType.Swine, _viewModel.Diets.First().AnimalType);
        }

        [TestMethod]
        public void SearchText_ClearFilter_ShouldReturnAllDiets()
        {
            // Arrange
            _viewModel.SearchText = "Test";
            Assert.AreEqual(2, _viewModel.Diets.Count);

            // Act
            _viewModel.SearchText = "";

            // Assert
            Assert.AreEqual(3, _viewModel.Diets.Count);
        }

        [TestMethod]
        public void SearchText_CaseInsensitive_ShouldReturnMatchingDiets()
        {
            // Act
            _viewModel.SearchText = "test";

            // Assert
            Assert.AreEqual(2, _viewModel.Diets.Count);
            Assert.IsTrue(_viewModel.Diets.All(d => d.Name.ToLower().Contains("test")));
        }

        [TestMethod]
        public void SelectedDiet_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var expectedDiet = _viewModel.Diets.First();

            // Act
            _viewModel.SelectedDiet = expectedDiet;

            // Assert
            Assert.AreEqual(expectedDiet, _viewModel.SelectedDiet);
        }

        [TestMethod]
        public void InitializeViewModel_ShouldReloadDiets()
        {
            // Arrange
            _viewModel.Diets.Clear();
            Assert.AreEqual(0, _viewModel.Diets.Count);

            // Act
            _viewModel.InitializeViewModel();

            // Assert
            Assert.AreEqual(3, _viewModel.Diets.Count);
        }
    }
}
