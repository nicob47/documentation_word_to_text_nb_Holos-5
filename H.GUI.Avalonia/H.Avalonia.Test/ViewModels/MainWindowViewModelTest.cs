using H.Avalonia.Services;
using Moq;
using H.Avalonia.ViewModels;

#nullable disable

namespace H.Avalonia.Test.ViewModels
{
    [TestClass]
    public class MainWindowViewModelTest
    {
        private MainWindowViewModel _viewModel = null!;
        private Mock<INotificationManagerService> _mockNotificationManager = null!;
        private INotificationManagerService _notificationManagerMock = null!;

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
            _mockNotificationManager = new Mock<INotificationManagerService>();
            _notificationManagerMock = _mockNotificationManager.Object;
        }

        [TestMethod]
        public void TestConstructorNullParameters()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new MainWindowViewModel(null));
        }

        [TestMethod]
        public void TestConstructorSetsNotificationService()
        {
            _viewModel = new MainWindowViewModel(_notificationManagerMock);
            Assert.AreSame(_viewModel.NotificationManagerService, _notificationManagerMock);
        }
    }
}
