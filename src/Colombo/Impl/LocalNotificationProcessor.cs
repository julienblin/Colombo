﻿#region License
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Colombo.Impl.NotificationHandle;

namespace Colombo.Impl
{
    /// <summary>
    /// <see cref="INotificationProcessor"/> that processes notifications locally.
    /// </summary>
    public class LocalNotificationProcessor : INotificationProcessor
    {
        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger.
        /// </summary>
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
                if (!Logger.IsInfoEnabled) return;

                if (notificationHandleInterceptors.Length == 0)
                    Logger.Info("No interceptor has been registered for handling notifications.");
                else
                    Logger.InfoFormat("Handling notifications with the following interceptors: {0}", string.Join(", ", notificationHandleInterceptors.Select(x => x.GetType().Name)));
            }
        }

        private readonly INotificationHandlerFactory notificationHandlerFactory;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="notificationHandlerFactory">The <see cref="IRequestHandlerFactory"/> used to create <see cref="IRequestHandler"/>.</param>
        public LocalNotificationProcessor(INotificationHandlerFactory notificationHandlerFactory)
        {
            if (notificationHandlerFactory == null) throw new ArgumentNullException("notificationHandlerFactory");
            Contract.EndContractBlock();

            this.notificationHandlerFactory = notificationHandlerFactory;
        }

        /// <summary>
        /// Process the notifications.
        /// </summary>
        public void Process(Notification[] notifications)
        {
            if (notifications == null) throw new ArgumentNullException("notifications");
            Contract.EndContractBlock();

            Logger.Debug("Parallel processing of {0} notifications with local handlers...", notifications.Length);

            foreach (var invoc in BuildHandleInvocationChains(notifications))
                Task.Factory.StartNew(i => ((IColomboNotificationHandleInvocation)i).Proceed(), invoc);
        }

        private IEnumerable<IColomboNotificationHandleInvocation> BuildHandleInvocationChains(IEnumerable<Notification> notifications)
        {
            foreach (var notification in notifications)
            {
                if (!notificationHandlerFactory.CanCreateNotificationHandlerFor(notification)) continue;

                foreach (var notificationHandlerInvocation in
                    notificationHandlerFactory.CreateNotificationHandlersFor(notification).Select(notifHandler => new NotificationHandlerHandleInvocation(notificationHandlerFactory, notifHandler)))
                {
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
