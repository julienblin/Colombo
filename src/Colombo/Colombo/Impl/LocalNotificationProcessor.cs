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

        private INotificationHandleInterceptor[] notificationHandleInterceptors = new INotificationHandleInterceptor[0];
        /// <summary>
        /// The list of <see cref="INotificationHandleInterceptor"/> to use.
        /// </summary>
        public INotificationHandleInterceptor[] NotificationHandleInterceptors
        {
            get { return notificationHandleInterceptors; }
            set
            {
                if (value == null) throw new ArgumentNullException("NotificationHandleInterceptors");
                Contract.EndContractBlock();

                notificationHandleInterceptors = value.OrderBy(x => x.InterceptionPriority).ToArray();
                if (Logger.IsInfoEnabled)
                {
                    if (notificationHandleInterceptors.Length == 0)
                        Logger.Info("No interceptor has been registered for handling notifications.");
                    else
                        Logger.InfoFormat("Handling notifications with the following interceptors: {0}", string.Join(", ", notificationHandleInterceptors.Select(x => x.GetType().Name)));
                }
            }
        }

        public bool ProcessSynchronously { get; set; }

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

            foreach (var invoc in BuildHandleInvocationChains(notifications))
                Task.Factory.StartNew((i) => ((IColomboNotificationHandleInvocation)i).Proceed(), invoc);
        }

        private IEnumerable<IColomboNotificationHandleInvocation> BuildHandleInvocationChains(Notification[] notifications)
        {
            foreach (var notification in notifications)
            {
                if (notificationHandlerFactory.CanCreateNotificationHandlerFor(notification))
                {
                    foreach (var notifHandler in notificationHandlerFactory.CreateNotificationHandlersFor(notification))
                    {
                        var notificationHandlerInvocation = new NotificationHandlerHandleInvocation(notificationHandlerFactory, notifHandler);
                        notificationHandlerInvocation.Logger = Logger;
                        IColomboNotificationHandleInvocation currentInvocation = notificationHandlerInvocation;

                        foreach (var interceptor in NotificationHandleInterceptors.Reverse())
                        {
                            if (interceptor != null)
                                currentInvocation = new NotificationHandleInterceptorInvocation(interceptor, currentInvocation);
                        }
                        currentInvocation.Notification = notification;
                        yield return currentInvocation;
                    }
                }
            }
        }
    }
}
