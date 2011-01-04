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
        TResponse IMessageBus.Send<TResponse>(Request<TResponse> request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            return default(TResponse);
        }


        ResponseGroup<TFirstResponse, TSecondResponse> IMessageBus.ParallelSend<TFirstResponse, TSecondResponse>
            (Request<TFirstResponse> firstRequest, Request<TSecondResponse> secondRequest)
        {
            Contract.Requires<ArgumentNullException>(firstRequest != null, "firstRequest");
            Contract.Requires<ArgumentNullException>(secondRequest != null, "secondRequest");
            return default(ResponseGroup<TFirstResponse, TSecondResponse>);
        }


        ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse> IMessageBus.ParallelSend<TFirstResponse, TSecondResponse, TThirdResponse>
            (Request<TFirstResponse> firstRequest, Request<TSecondResponse> secondRequest, Request<TThirdResponse> thirdRequest)
        {
            Contract.Requires<ArgumentNullException>(firstRequest != null, "firstRequest");
            Contract.Requires<ArgumentNullException>(secondRequest != null, "secondRequest");
            Contract.Requires<ArgumentNullException>(thirdRequest != null, "thirdRequest");
            return default(ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse>);
        }

        ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse> IMessageBus.ParallelSend<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse>
            (Request<TFirstResponse> firstRequest, Request<TSecondResponse> secondRequest, Request<TThirdResponse> thirdRequest, Request<TFourthResponse> fourthRequest)
        {
            Contract.Requires<ArgumentNullException>(firstRequest != null, "firstRequest");
            Contract.Requires<ArgumentNullException>(secondRequest != null, "secondRequest");
            Contract.Requires<ArgumentNullException>(thirdRequest != null, "thirdRequest");
            Contract.Requires<ArgumentNullException>(fourthRequest != null, "fourthRequest");
            return default(ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse>);
        }
    }
}
