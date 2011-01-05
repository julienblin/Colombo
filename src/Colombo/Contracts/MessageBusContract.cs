using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IMessageBus))]
    public abstract class MessageBusContract : IMessageBus
    {
        public TResponse Send<TResponse>(Request<TResponse> request) where TResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            throw new NotImplementedException();
        }

        public ResponseGroup<TFirstResponse, TSecondResponse>
            Send<TFirstResponse, TSecondResponse>
            (SideEffectFreeRequest<TFirstResponse> firstRequest, SideEffectFreeRequest<TSecondResponse> secondRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(firstRequest != null, "firstRequest");
            Contract.Requires<ArgumentNullException>(secondRequest != null, "secondRequest");
            Contract.Ensures(Contract.Result<ResponseGroup<TFirstResponse, TSecondResponse>>() != null);
            throw new NotImplementedException();
        }

        public ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse>
            Send<TFirstResponse, TSecondResponse, TThirdResponse>
            (SideEffectFreeRequest<TFirstResponse> firstRequest, SideEffectFreeRequest<TSecondResponse> secondRequest,
             SideEffectFreeRequest<TThirdResponse> thirdRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
            where TThirdResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(firstRequest != null, "firstRequest");
            Contract.Requires<ArgumentNullException>(secondRequest != null, "secondRequest");
            Contract.Requires<ArgumentNullException>(thirdRequest != null, "thirdRequest");
            Contract.Ensures(Contract.Result<ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse>>() != null);
            throw new NotImplementedException();
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
            Contract.Requires<ArgumentNullException>(firstRequest != null, "firstRequest");
            Contract.Requires<ArgumentNullException>(secondRequest != null, "secondRequest");
            Contract.Requires<ArgumentNullException>(thirdRequest != null, "thirdRequest");
            Contract.Requires<ArgumentNullException>(fourthRequest != null, "fourthRequest");
            Contract.Ensures(Contract.Result<ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse>>() != null);
            throw new NotImplementedException();
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
            Contract.Requires<ArgumentNullException>(firstRequest != null, "firstRequest");
            Contract.Requires<ArgumentNullException>(secondRequest != null, "secondRequest");
            Contract.Requires<ArgumentNullException>(thirdRequest != null, "thirdRequest");
            Contract.Requires<ArgumentNullException>(fourthRequest != null, "fourthRequest");
            Contract.Requires<ArgumentNullException>(fifthRequest != null, "fifthRequest");
            Contract.Ensures(Contract.Result<ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse, TFifthResponse>>() != null);
            throw new NotImplementedException();
        }
    }
}
