using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public class MessageBus : IMessageBus
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        public TResponse Send<TResponse>(Request<TResponse> request) where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var responses = InternalSend(request);
            Contract.Assume(responses != null);
            Contract.Assume(responses.Length == 1);

            var typedResponse = responses[0] as TResponse;
            if(typedResponse == null)
                LogAndThrowError("Internal error: InternalSend returned null or incorrect response type: expected {0}, actual {1}.", typeof(TResponse), responses[0].GetType());

            return typedResponse;
        }

        public ResponseGroup<TFirstResponse, TSecondResponse>
            Send<TFirstResponse, TSecondResponse>
            (SideEffectFreeRequest<TFirstResponse> firstRequest, SideEffectFreeRequest<TSecondResponse> secondRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
        {
            if (firstRequest == null) throw new ArgumentNullException("firstRequest");
            if (secondRequest == null) throw new ArgumentNullException("secondRequest");
            Contract.EndContractBlock();

            var responses = InternalSend(firstRequest, secondRequest);
            Contract.Assume(responses != null);
            Contract.Assume(responses.Length == 2);

            return new ResponseGroup<TFirstResponse, TSecondResponse>(responses);
        }

        public ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse>
            Send<TFirstResponse, TSecondResponse, TThirdResponse>
            (SideEffectFreeRequest<TFirstResponse> firstRequest, SideEffectFreeRequest<TSecondResponse> secondRequest,
             SideEffectFreeRequest<TThirdResponse> thirdRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
            where TThirdResponse : Response, new()
        {
            if (firstRequest == null) throw new ArgumentNullException("firstRequest");
            if (secondRequest == null) throw new ArgumentNullException("secondRequest");
            if (thirdRequest == null) throw new ArgumentNullException("thirdRequest");
            Contract.EndContractBlock();

            var responses = InternalSend(firstRequest, secondRequest, thirdRequest);
            Contract.Assume(responses != null);
            Contract.Assume(responses.Length == 3);

            return new ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse>(responses);
        }

        public ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse>
            Send<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse>
            (SideEffectFreeRequest<TFirstResponse> firstRequest, SideEffectFreeRequest<TSecondResponse> secondRequest,
             SideEffectFreeRequest<TThirdResponse> thirdRequest, SideEffectFreeRequest<TFourthResponse> fourthRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
            where TThirdResponse : Response, new()
            where TFourthResponse : Response, new()
        {
            if (firstRequest == null) throw new ArgumentNullException("firstRequest");
            if (secondRequest == null) throw new ArgumentNullException("secondRequest");
            if (thirdRequest == null) throw new ArgumentNullException("thirdRequest");
            if (fourthRequest == null) throw new ArgumentNullException("fourthRequest");
            Contract.EndContractBlock();

            var responses = InternalSend(firstRequest, secondRequest, thirdRequest, fourthRequest);
            Contract.Assume(responses != null);
            Contract.Assume(responses.Length == 4);

            return new ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse>(responses);
        }

        public ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse, TFifthResponse>
            Send<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse, TFifthResponse>
            (SideEffectFreeRequest<TFirstResponse> firstRequest, SideEffectFreeRequest<TSecondResponse> secondRequest,
             SideEffectFreeRequest<TThirdResponse> thirdRequest, SideEffectFreeRequest<TFourthResponse> fourthRequest,
             SideEffectFreeRequest<TFifthResponse> fifthRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
            where TThirdResponse : Response, new()
            where TFourthResponse : Response, new()
            where TFifthResponse : Response, new()
        {
            if (firstRequest == null) throw new ArgumentNullException("firstRequest");
            if (secondRequest == null) throw new ArgumentNullException("secondRequest");
            if (thirdRequest == null) throw new ArgumentNullException("thirdRequest");
            if (fourthRequest == null) throw new ArgumentNullException("fourthRequest");
            if (fifthRequest == null) throw new ArgumentNullException("fifthRequest");
            Contract.EndContractBlock();

            var responses = InternalSend(firstRequest, secondRequest, thirdRequest, fourthRequest, fifthRequest);
            Contract.Assume(responses != null);
            Contract.Assume(responses.Length == 5);

            return new ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse, TFifthResponse>(responses);
        }

        protected virtual Response[] InternalSend(params BaseRequest[] requests)
        {
            throw new NotImplementedException();
        }

        protected virtual void LogAndThrowError(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");
            Contract.EndContractBlock();

            var errorMessage = string.Format(format, args);
            Logger.Error(errorMessage);
            throw new ColomboException(errorMessage);
        }
    }
}
