using System;
using System.Diagnostics.Contracts;
using Castle.MicroKernel;

namespace Colombo.Impl.NotificationHandle
{
    /// <summary>
    /// Implementation of <see cref="INotificationHandlerFactory"/> that uses <see cref="Castle.MicroKernel.IKernel"/>.
    /// </summary>
    public class KernelNotificationHandlerFactory : INotificationHandlerFactory
    {
        private readonly IKernel kernel;

        public KernelNotificationHandlerFactory(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            Contract.EndContractBlock();

            this.kernel = kernel;
        }

        public bool CanCreateNotificationHandlerFor(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");
            Contract.EndContractBlock();

            var requestNotificationType = CreateNotificationHandlerTypeFrom(notification);
            return kernel.HasComponent(requestNotificationType);
        }

        public INotificationHandler[] CreateNotificationHandlersFor(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");
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

            var notificationType = notification.GetType();

            Contract.Assume(typeof(INotificationHandler<>).IsGenericTypeDefinition);
            Contract.Assume(typeof(INotificationHandler<>).GetGenericArguments().Length == 1);
            return typeof(INotificationHandler<>).MakeGenericType(notificationType);
        }
    }
}
