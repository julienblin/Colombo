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

namespace Colombo
{
    /// <summary>
    /// Use this base class to create notification handlers.
    /// </summary>
    public abstract class NotificationHandler<TNotification> : INotificationHandler<TNotification>
        where TNotification : Notification
    {
        /// <summary>
        /// Incoming notification.
        /// </summary>
        protected TNotification Notification { get; private set; }

        /// <summary>
        /// Handles the notification.
        /// </summary>
        public void Handle(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");
            Contract.EndContractBlock();

            Handle((TNotification)notification);
        }

        /// <summary>
        /// Handles the notification.
        /// </summary>
        public void Handle(TNotification notification)
        {
            Notification = notification;
            Handle();
        }

        /// <summary>
        /// Handles the notification.
        /// </summary>
        protected abstract void Handle();

        /// <summary>
        /// Create a new request to be used inside this notification handler.
        /// The CorrelationGuid and the Context are copied.
        /// </summary>
        protected TRequest CreateRequest<TRequest>()
            where TRequest : BaseRequest, new()
        {
            var result = new TRequest
                             {
                                 CorrelationGuid = Notification.CorrelationGuid,
                                 Context = Notification.Context
                             };
            return result;
        }

        /// <summary>
        /// Create a new notification to be used inside this notification handler.
        /// The CorrelationGuid and the Context are copied.
        /// </summary>
        protected TNewNotification CreateNotification<TNewNotification>()
            where TNewNotification : Notification, new()
        {
            var result = new TNewNotification
                             {
                                 CorrelationGuid = Notification.CorrelationGuid,
                                 Context = Notification.Context
                             };
            return result;
        }
    }
}
