using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Colombo.Impl
{
    public class LocalNotificationProcessor : INotificationProcessor
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly INotificationHandlerFactory notificationHandlerFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestHandlerFactory">The <see cref="IRequestHandlerFactory"/> used to create <see cref="IRequestHandler"/>.</param>
        public LocalNotificationProcessor(INotificationHandlerFactory notificationHandlerFactory)
        {
            if (notificationHandlerFactory == null) throw new ArgumentNullException("notificationHandlerFactory");
            Contract.EndContractBlock();

            this.notificationHandlerFactory = notificationHandlerFactory;
        }

        public void Process(Notification[] notifications)
        {
            if (notifications == null) throw new ArgumentNullException("notifications");
            Contract.EndContractBlock();

            Logger.Debug("Parallel processing of {0} notifications with local handlers...", notifications.Length);
            foreach (var notification in notifications)
            {
                if (notificationHandlerFactory.CanCreateNotificationHandlerFor(notification))
                {
                    foreach (var notifHandler in notificationHandlerFactory.CreateNotificationHandlersFor(notification))
                    {
                        try
                        {
                            Logger.DebugFormat("Handling {0} with {1}...", notification, notifHandler);
                            notifHandler.Handle(notification);
                        }
                        catch (Exception ex)
                        {
                            Logger.ErrorFormat(ex, "Error while handling {0} with {1}.", notification, notifHandler);
                            throw;
                        }
                        finally
                        {
                            notificationHandlerFactory.DisposeNotificationHandler(notifHandler);
                        }
                    }
                }
            }
        }
    }
}
