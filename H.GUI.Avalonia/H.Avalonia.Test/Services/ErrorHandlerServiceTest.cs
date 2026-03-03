using Moq;
using Avalonia.Controls.Notifications;
using H.Avalonia.Events;
using H.Avalonia.Services;
using Microsoft.Extensions.Logging;
using Prism.Events;

#nullable disable

namespace H.Avalonia.Test.Services
{
    [TestClass]
    public class ErrorHandlerServiceTest
    {
        private ErrorHandlerService _service = null!;
        private Mock<ILogger> _mockLogger = null!;
        private ILogger _loggerMock = null!;
        private Mock<IEventAggregator> _mockEventAggregator = null!;
        private IEventAggregator _eventAggregatorMock = null!;
        private Mock<INotificationManagerService> _mockNotificationManager = null!;
        private INotificationManagerService _notificationManagerMock = null!;
#pragma warning disable CS0414
        private Mock<PubSubEvent<ValidationErrorOccurredEvent>> _mockEvent = null!;
#pragma warning restore CS0414

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
            _mockLogger = new Mock<ILogger>();
            _loggerMock = _mockLogger.Object;
            _mockEventAggregator = new Mock<IEventAggregator>();
            _eventAggregatorMock = _mockEventAggregator.Object;
            _mockNotificationManager = new Mock<INotificationManagerService>();
            _notificationManagerMock = _mockNotificationManager.Object;

            var mockEvent = new Mock<PubSubEvent<ValidationErrorOccurredEvent>>();
            _mockEventAggregator.Setup(x => x.GetEvent<ValidationErrorOccurredEvent>()).Returns(new ValidationErrorOccurredEvent());
        }

        [TestMethod]
        public void TestConstructorNullLogger()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new ErrorHandlerService(null, _eventAggregatorMock, _notificationManagerMock));
        }
        [TestMethod]
        public void TestConstructorNullEventAggregator()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new ErrorHandlerService(_loggerMock, null, _notificationManagerMock));
        }
        [TestMethod]
        public void TestConstructorNullNotificationManager()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new ErrorHandlerService(_loggerMock, _eventAggregatorMock, null));
        }
        [TestMethod]
        public void TestConstructorInitialization()
        {
            _service = new ErrorHandlerService(_loggerMock, _eventAggregatorMock, _notificationManagerMock);
            Assert.IsNotNull(_service);
        }

        [TestMethod]
        public void TestHandleValidationWarning()
        {
            _service = new ErrorHandlerService(_loggerMock, _eventAggregatorMock, _notificationManagerMock);
            _service.HandleValidationWarning("Test Title", "Test Message");

            _mockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.AtMostOnce);

            _mockNotificationManager.Verify(x => x.ShowToast("Test Title", "Test Message", NotificationType.Information), Times.AtMostOnce);
        }

        [TestMethod]
        public void TestHandleNonInterruptingError()
        {
            _service = new ErrorHandlerService(_loggerMock, _eventAggregatorMock, _notificationManagerMock);
            _service.HandleNonInterruptingError("Error Title", "Error Message");
            _mockLogger.Verify(x => x.Log(It.IsAny<LogLevel>(), It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.AtMostOnce);
            _mockNotificationManager.Verify(x => x.ShowToast("Error Title", "Error Message", NotificationType.Error), Times.AtMostOnce);
        }
    }
}
