using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.MessageBusContract))]
    public interface IMessageBus
    {
        TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new();

        ResponseGroup<TFirstResponse, TSecondResponse> ParallelSend<TFirstResponse, TSecondResponse>
            (Request<TFirstResponse> firstRequest, Request<TSecondResponse> secondRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new();

        ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse> ParallelSend<TFirstResponse, TSecondResponse, TThirdResponse>
            (Request<TFirstResponse> firstRequest, Request<TSecondResponse> secondRequest, Request<TThirdResponse> thirdRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
            where TThirdResponse : Response, new();

        ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse> ParallelSend<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse>
            (Request<TFirstResponse> firstRequest, Request<TSecondResponse> secondRequest, Request<TThirdResponse> thirdRequest, Request<TFourthResponse> fourthRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
            where TThirdResponse : Response, new()
            where TFourthResponse : Response, new();
    }
}
