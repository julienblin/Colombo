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
                        currentInvocation.Notification = notification;
                        yield return currentInvocation;
                    }
                }
            }

            //var requestProcessorInvocation = new RequestProcessorSendInvocation(requestProcessors);
            //requestProcessorInvocation.Logger = Logger;
            //IColomboSendInvocation currentInvocation = requestProcessorInvocation;

            //foreach (var interceptor in MessageBusSendInterceptors.Reverse())
            //{
            //    if (interceptor != null)
            //        currentInvocation = new MessageBusSendInterceptorInvocation(interceptor, currentInvocation);
            //}

            //return currentInvocation;
        }
    }
}
