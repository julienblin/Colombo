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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Colombo.Impl;
using Colombo.Impl.Notify;
using Colombo.Impl.Send;

namespace Colombo.TestSupport
{
    /// <summary>
    /// Implementation of the <see cref="IStubMessageBus"/>.
    /// </summary>
    public class StubMessageBus : BaseMessageBus, IStubMessageBus
    {
        private readonly Dictionary<Type, BaseExpectation> expectations = new Dictionary<Type, BaseExpectation>();

        private IRequestHandlerHandleInterceptor[] requestHandlerInterceptors = new IRequestHandlerHandleInterceptor[0];
        /// <summary>
        /// The list of <see cref="IRequestHandlerHandleInterceptor"/> to use.
        /// </summary>
        public IRequestHandlerHandleInterceptor[] RequestHandlerInterceptors
        {
            get { return requestHandlerInterceptors; }
            set
            {
                if (value == null) throw new ArgumentNullException("RequestHandlerInterceptors");
                Contract.EndContractBlock();

                requestHandlerInterceptors = value.OrderBy(x => x.InterceptionPriority).ToArray();
                Logger.InfoFormat("Using the following interceptors: {0}", string.Join(", ", requestHandlerInterceptors.Select(x => x.GetType().Name)));
            }
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

        #region IStubMessageBus implementation

        /// <summary>
        /// The <see cref="IKernel"/> that will be injected.
        /// </summary>
        public IKernel Kernel { get; set; }

        /// <summary>
        /// Indicates a handler type that is under test.
        /// </summary>
        public RequestHandlerExpectation<THandler> TestHandler<THandler>()
            where THandler : IRequestHandler
        {
            Kernel.Register(Component.For<THandler>().LifeStyle.Transient);

            var handler = Kernel.Resolve<THandler>();

            if (expectations.ContainsKey(handler.GetRequestType()))
                throw new ColomboTestSupportException(string.Format("Unable to test handler {0}: an expectation for {1} already exists.", typeof(THandler), handler.GetRequestType()));

            var expectation = new RequestHandlerExpectation<THandler>(this);
            expectations[handler.GetRequestType()] = expectation;
            return expectation;
        }

        /// <summary>
        /// Indicates an expectation that a type of Request will be sent.
        /// </summary>
        public MessageBusSendExpectation<TRequest, TResponse> ExpectSend<TRequest, TResponse>()
            where TRequest : BaseRequest, new()
            where TResponse : Response, new()
        {
            var request = new TRequest();

            if (!request.GetResponseType().Equals(typeof(TResponse)))
                throw new ColomboTestSupportException(string.Format("Wrong response type for request {0}. Expected: {1}, actual {2}.", typeof(TRequest), typeof(TResponse), request.GetResponseType()));

            if(expectations.ContainsKey(typeof (TRequest)))
                throw new ColomboTestSupportException(string.Format("Unable to place expectation for {0}: an expectation already exists.", typeof(TRequest)));

            var expectation = new MessageBusSendExpectation<TRequest, TResponse>(this);
            expectations[typeof (TRequest)] = expectation;
            return expectation;
        }

        /// <summary>
        /// Indicates an expectation that a type of <see cref="Notification"/> will be notified.
        /// </summary>
        /// <typeparam name="TNotification"></typeparam>
        /// <returns></returns>
        public MessageBusNotifyExpectation<TNotification> ExpectNotify<TNotification>()
            where TNotification : Notification, new()
        {
            var expectation = new MessageBusNotifyExpectation<TNotification>(this);
            expectations[typeof (TNotification)] = expectation;
            return expectation;
        }

        /// <summary>
        /// <c>true</c> to allow the <see cref="IStubMessageBus"/> to reply to requests using empty responses,
        /// <c>false</c> to disallow and throw a <see cref="ColomboExpectationException"/> when sending an unexpected request or notification.
        /// </summary>
        public bool AllowUnexpectedMessages { get; set; }

        /// <summary>
        /// Returns the <see cref="BaseExpectation"/> associated with the <paramref name="messageType"/>
        /// </summary>
        public BaseExpectation GetExpectationFor(Type messageType)
        {
            if (expectations.ContainsKey(messageType))
                return expectations[messageType];
            
            return null;
        }

        /// <summary>
        /// Verify all the expectations
        /// </summary>
        /// <exception cref="ColomboExpectationException" />
        public void Verify()
        {
            foreach (var expectation in expectations.Values)
            {
                expectation.Verify();
            }
        }

        #endregion

        /// <summary>
        /// Dispatch notifications
        /// </summary>
        public override void Notify(Notification notification, params Notification[] notifications)
        {
            var finalNotifications = new List<Notification> { notification };
            if (notifications != null)
                finalNotifications.AddRange(notifications);

            if (!AllowUnexpectedMessages && finalNotifications.Any(notif => !expectations.ContainsKey(notif.GetType())))
            {
                var unexpectedNotifications = finalNotifications.Where(notif => !expectations.ContainsKey(notif.GetType()));
                if (unexpectedNotifications.Count() == 1)
                    throw new ColomboExpectationException(string.Format("Notification {0} was not expected. If you want to disable this check, try the AllowUnexpectedMessages property.", unexpectedNotifications.First()));
                else
                    throw new ColomboExpectationException(string.Format("Notifications {0} were not expected. If you want to disable this check, try the AllowUnexpectedMessages property.", string.Join(", ", unexpectedNotifications.Select(r => r.ToString()))));
            }

            base.Notify(notification, notifications);
        }

        /// <summary>
        /// Real sending of the requests. All the other send methods delegates to this one.
        /// Uses <see cref="BaseMessageBus.BuildSendInvocationChain"/>.
        /// </summary>
        protected override ResponsesGroup InternalSend(IList<BaseRequest> requests)
        {
            if (!AllowUnexpectedMessages && requests.Any(request => !expectations.ContainsKey(request.GetType())))
            {
                var unexpectedRequests = requests.Where(request => !expectations.ContainsKey(request.GetType()));
                if (unexpectedRequests.Count() == 1)
                    throw new ColomboExpectationException(string.Format("Request {0} was not expected. If you want to allow automatic responding, try the AllowUnexpectedMessages property.", unexpectedRequests.First()));
                else
                    throw new ColomboExpectationException(string.Format("Requests {0} were not expected. If you want to allow automatic responding, try the AllowUnexpectedMessages property.", string.Join(", ", unexpectedRequests.Select(r => r.ToString()))));
            }

            return base.InternalSend(requests);
        }

        /// <summary>
        /// Return a invocation chain for the Send operation.
        /// </summary>
        protected override IColomboSendInvocation BuildSendInvocationChain()
        {
            IColomboSendInvocation currentInvocation = new StubSendInvocation(this, RequestHandlerInterceptors);

            foreach (var interceptor in MessageBusSendInterceptors.Reverse())
            {
                if (interceptor != null)
                    currentInvocation = new MessageBusSendInterceptorInvocation(interceptor, currentInvocation);
            }

            return currentInvocation;
        }

        /// <summary>
        /// Return a invocation chain for the Notify operation.
        /// </summary>
        protected override IColomboNotifyInvocation BuildNotifyInvocationChain()
        {
            IColomboNotifyInvocation currentInvocation = new StubNotifyInvocation(this, NotificationHandleInterceptors);

            foreach (var interceptor in MessageBusNotifyInterceptors.Reverse())
            {
                if (interceptor != null)
                    currentInvocation = new MessageBusNotifyInterceptorInvocation(interceptor, currentInvocation);
            }

            return currentInvocation;
        }
    }
}
