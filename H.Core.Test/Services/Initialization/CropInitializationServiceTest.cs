using H.Core.Models;
using H.Core.Providers.Energy;
using H.Core.Services.Initialization;
using H.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace H.Core.Test.Services.Initialization
{
    [TestClass]
    public class CropInitializationServiceTest
    {
        #region Fields

        private Mock<ICacheService> _cacheServiceMock = null!;
        private Mock<ITable50FuelEnergyEstimatesProvider> _table50ProviderMock = null!;
        private CropInitializationService _service = null!;
        private Mock<ILogger> _mockLogger = null!;

        #endregion

        #region Initialialization

        [TestInitialize]
        public void Setup()
        {
            _cacheServiceMock = new Mock<ICacheService>();
            _table50ProviderMock = new Mock<ITable50FuelEnergyEstimatesProvider>();
            _mockLogger = new Mock<Microsoft.Extensions.Logging.ILogger>();
            _service = new CropInitializationService(_mockLogger.Object, _cacheServiceMock.Object, _table50ProviderMock.Object);
        }

        #endregion

        #region Public Methods

        [TestMethod]
        public void Initialize_CallsInitializeFuelEnergy()
        {
            // Arrange
            var farm = new Farm();
            var serviceMock = new Mock<CropInitializationService>(_mockLogger.Object, _cacheServiceMock.Object, _table50ProviderMock.Object) { CallBase = true };
            serviceMock.Setup(s => s.InitializeFuelEnergy(farm)).Verifiable();

            // Act
            serviceMock.Object.Initialize(farm);

            // Assert
            serviceMock.Verify(s => s.InitializeFuelEnergy(farm), Times.Once);
        } 

        #endregion
    }
}
