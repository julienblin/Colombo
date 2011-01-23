using System;
using Castle.Core.Logging;

namespace Colombo.Impl.NotificationHandle
{
    internal class NotificationHandlerHandleInvocation : BaseNotificationHandleInvocation
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly INotificationHandlerFactory notificationHandlerFactory;
        private readonly INotificationHandler notificationHandler;

        public NotificationHandlerHandleInvocation(INotificationHandlerFactory notificationHandlerFactory, INotificationHandler notificationHandler)
        {
            this.notificationHandlerFactory = notificationHandlerFactory;
            this.notificationHandler = notificationHandler;
        }

        public override void Proceed()
        {
            try
            {
                Logger.DebugFormat("Handling {0} with {1}...", Notification, notificationHandler);
                notificationHandler.Handle(Notification);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat(ex, "Error while handling {0} with {1}.", Notification, notificationHandler);
            }
            finally
            {
                notificationHandlerFactory.DisposeNotificationHandler(notificationHandler);
            }
        }
    }
}
