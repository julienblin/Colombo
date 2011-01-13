using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using Castle.MicroKernel;
using System.Diagnostics.Contracts;

namespace Colombo.Impl.NotificationHandle
{
    /// <summary>
    /// Implementation of <see cref="INotificationHandlerFactory"/> that uses <see cref="Castle.MicroKernel.IKernel"/>.
    /// </summary>
    public class KernelNotificationHandlerFactory : INotificationHandlerFactory
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly IKernel kernel;

        public KernelNotificationHandlerFactory(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            Contract.EndContractBlock();

            this.kernel = kernel;
        }

        public bool CanCreateNotificationHandlerFor(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException("reqnotificationuest");
            Contract.EndContractBlock();

            var requestNotificationType = CreateNotificationHandlerTypeFrom(notification);
            return kernel.HasComponent(requestNotificationType);
        }

        public INotificationHandler[] CreateNotificationHandlersFor(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException("reqnotificationuest");
            Contract.EndContractBlock();

            var requestNotificationType = CreateNotificationHandlerTypeFrom(notification);
            return (INotificationHandler[])kernel.ResolveAll(requestNotificationType);
        }

        public void DisposeNotificationHandler(INotificationHandler notificationHandler)
        {
            if (notificationHandler == null) throw new ArgumentNullException("notificationHandler");
            Contract.EndContractBlock();

            kernel.ReleaseComponent(notificationHandler);
        }

        private static Type CreateNotificationHandlerTypeFrom(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");
            Contract.EndContractBlock();

            Type notificationType = notification.GetType();

            Contract.Assume(typeof(INotificationHandler<>).IsGenericTypeDefinition);
            Contract.Assume(typeof(INotificationHandler<>).GetGenericArguments().Length == 1);
            return typeof(INotificationHandler<>).MakeGenericType(notificationType);
        }
    }
}
