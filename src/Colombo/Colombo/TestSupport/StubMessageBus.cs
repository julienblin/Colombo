using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Colombo.Impl.Async;
using Colombo.Impl;

namespace Colombo.TestSupport
{
    public class StubMessageBus : BaseMessageBus, IStubMessageBus
    {
        private readonly Dictionary<Type, BaseExpectation> expectations = new Dictionary<Type, BaseExpectation>();

        #region IStubMessageBus implementation

        public IKernel Kernel { get; set; }

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

        public MessageBusNotifyExpectation<TNotification> ExpectNotify<TNotification>()
            where TNotification : Notification, new()
        {
            var expectation = new MessageBusNotifyExpectation<TNotification>(this);
            expectations[typeof (TNotification)] = expectation;
            return expectation;
        }

        public bool AllowUnexpectedMessages { get; set; }

        public BaseExpectation GetExpectationFor(Type messageType)
        {
            if (expectations.ContainsKey(messageType))
                return expectations[messageType];
            
            return null;
        }

        public void Verify()
        {
            foreach (var expectation in expectations.Values)
            {
                expectation.Verify();
            }
        }

        #endregion

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

        protected override IColomboSendInvocation BuildSendInvocationChain()
        {
            IColomboSendInvocation currentInvocation = new StubSendInvocation(this);
            return currentInvocation;
        }

        protected override IColomboNotifyInvocation BuildNotifyInvocationChain()
        {
            IColomboNotifyInvocation currentInvocation = new StubNotifyInvocation(this);
            return currentInvocation;
        }
    }
}
