﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;

namespace Colombo.TestSupport
{
    public class StubMessageBus : IMessageBus, IStubMessageBus
    {
        private readonly Dictionary<Type, BaseExpectation> expectations = new Dictionary<Type, BaseExpectation>();

        #region IMessageBus implementation

        public TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var responses = InternalSend(new List<BaseRequest> { request });

            var typedResponse = responses[request] as TResponse;
            if (typedResponse == null)
                throw new ColomboTestSupportException(string.Format("Internal error: InternalSend returned null or incorrect response type: expected {0}, actual {1}.", typeof(TResponse), responses[request].GetType()));

            return typedResponse;
        }

        public IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }

        public TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }

        public TResponse Send<TRequest, TResponse>(Action<TRequest> action)
            where TRequest : SideEffectFreeRequest<TResponse>, new()
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }

        public IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }

        public ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
        {
            throw new NotImplementedException();
        }

        public void Notify(Notification notification, params Notification[] notifications)
        {
            throw new NotImplementedException();
        }

        #endregion

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

        public MessageBusSendExpectation<TRequest, TResponse> Expect<TRequest, TResponse>()
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

        public MessageBusNotifyExpectation<TNotification> Expect<TNotification>()
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

        protected virtual ResponsesGroup InternalSend(IList<BaseRequest> requests)
        {
            if (!AllowUnexpectedMessages && requests.Any(request => !expectations.ContainsKey(request.GetType())))
            {
                var unexpectedRequests = requests.Where(request => !expectations.ContainsKey(request.GetType()));
                if(unexpectedRequests.Count() == 1)
                    throw new ColomboExpectationException(string.Format("Request {0} was not expected. If you want to allow automatic responding, try the AllowUnexpectedMessages property.", unexpectedRequests.First()));
                else
                    throw new ColomboExpectationException(string.Format("Requests {0} were not expected. If you want to allow automatic responding, try the AllowUnexpectedMessages property.", string.Join(", ", unexpectedRequests.Select(r => r.ToString()))));
            }

            var topInvocation = BuildSendInvocationChain();
            topInvocation.Requests = requests;
            topInvocation.Proceed();

            if (topInvocation.Responses == null)
                throw new ColomboException("Internal error: responses should not be null");

            return topInvocation.Responses;
        }

        private IColomboSendInvocation BuildSendInvocationChain()
        {
            IColomboSendInvocation currentInvocation = new StubSendInvocation(this);
            return currentInvocation;
        }
    }
}