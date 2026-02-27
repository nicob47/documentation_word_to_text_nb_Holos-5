using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace H.Avalonia.Services
{
    public class NotificationManagerService : INotificationManagerService
    {
        #region Fields

        private WindowNotificationManager _notificationManager = null!;
        private readonly ILogger _logger = null!;
        private bool _isInitialized = false;
        // ConcurrentBag used over list as it avoids potential issues introduced by async timer for notification expirations
        // Bag used over queue as some notifications have greater lifespan
        private readonly ConcurrentBag<Notification> _activeNotifications = new();
        private TimeSpan _successTimeSpan = TimeSpan.FromSeconds(5);
        private TimeSpan _informationTimeSpan = TimeSpan.FromSeconds(5);
        private TimeSpan _warningTimeSpan = TimeSpan.FromSeconds(10);
        private TimeSpan _errorTimeSpan = TimeSpan.FromSeconds(10);

        #endregion

        #region Properties

        public bool IsInitialized
        {
            get => _isInitialized;
        }

        public int maxDisplayedItems
        {
            get => _notificationManager?.MaxItems ?? 0;
            set
            {
                if (value >= 0)
                {
                    _notificationManager.MaxItems = value;
                }
            }
        }

        public IReadOnlyCollection<Notification> ActiveNotifications => _activeNotifications;

        #endregion

        #region Constructors

        public NotificationManagerService(ILogger logger)
        {
            if (logger != null)
            {
                _logger = logger;
            }
            else
            {
                throw new ArgumentNullException(nameof(logger));
            }
        }

        #endregion

        #region Public Methods

        public void Initialize(TopLevel targetWindow)
        {
            if (targetWindow == null)
            {
                throw new ArgumentNullException(nameof(targetWindow));
            }

            if (!_isInitialized)
            {
                _logger.LogInformation("Initializing " + this + " to " + targetWindow);

                _notificationManager = new WindowNotificationManager(targetWindow)
                {
                    Position = NotificationPosition.BottomRight,
                    MaxItems = 4,
                    Margin = new(0, 0, 15, 5),
                };
                _isInitialized = true;
            }
            else
            {
                _logger.LogWarning("{Service} attempted reinitialization.", nameof(NotificationManagerService));
            }
        }

        public void ShowToast(string title, string message, NotificationType type = NotificationType.Information)
        {
            if (!_isInitialized)
            {
                _logger.LogWarning("Toast message sent to {Service} before initialization completed.", nameof(NotificationManagerService));
                return;
            }

            // Determine duration based on notification type if not explicitly provided
            TimeSpan duration = _informationTimeSpan;
            switch (type)
            {
                case NotificationType.Success:
                    duration = _successTimeSpan;
                    break;
                case NotificationType.Warning:
                    duration = _warningTimeSpan;
                    break;
                case NotificationType.Error: 
                    duration = _errorTimeSpan;
                    break;
            }

            var notification = new Notification(title, message, type, duration);
            _notificationManager?.Show(notification);
            _activeNotifications.Add(notification);

            // Remove notification from collection once timer expires
            Task.Delay(notification.Expiration).ContinueWith(x =>
            {
                _activeNotifications.TryTake(out Notification? discard);
            });
        }

        #endregion
    }
}