#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

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

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kernel">Kernel used to resolve the notification handlers.</param>
        public KernelNotificationHandlerFactory(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            Contract.EndContractBlock();

            this.kernel = kernel;
        }

        /// <summary>
        /// Return true if it can create a <see cref="INotificationHandler"/> that handles the <paramref name="notification"/>, false otherwise.
        /// </summary>
        public bool CanCreateNotificationHandlerFor(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");
            Contract.EndContractBlock();

            var requestNotificationType = CreateNotificationHandlerTypeFrom(notification);
            return kernel.HasComponent(requestNotificationType);
        }

        /// <summary>
        /// Create the <see cref="INotificationHandler">notification handlers</see> that can handle the <paramref name="notification"/>.
        /// </summary>
        public INotificationHandler[] CreateNotificationHandlersFor(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");
            Contract.EndContractBlock();

            var requestNotificationType = CreateNotificationHandlerTypeFrom(notification);
            return (INotificationHandler[])kernel.ResolveAll(requestNotificationType);
        }

        /// <summary>
        /// Dispose the <paramref name="notificationHandler"/>.
        /// </summary>
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
