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

        ResponseGroup<TFirstResponse, TSecondResponse>
            Send<TFirstResponse, TSecondResponse>
            (SideEffectFreeRequest<TFirstResponse> firstRequest, SideEffectFreeRequest<TSecondResponse> secondRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new();

        ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse>
            Send<TFirstResponse, TSecondResponse, TThirdResponse>
            (SideEffectFreeRequest<TFirstResponse> firstRequest, SideEffectFreeRequest<TSecondResponse> secondRequest,
             SideEffectFreeRequest<TThirdResponse> thirdRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
            where TThirdResponse : Response, new();

        ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse>
            Send<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse>
            (SideEffectFreeRequest<TFirstResponse> firstRequest, SideEffectFreeRequest<TSecondResponse> secondRequest,
             SideEffectFreeRequest<TThirdResponse> thirdRequest, SideEffectFreeRequest<TFourthResponse> fourthRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
            where TThirdResponse : Response, new()
            where TFourthResponse : Response, new();

        ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse, TFifthResponse>
            Send<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse, TFifthResponse>
            (SideEffectFreeRequest<TFirstResponse> firstRequest, SideEffectFreeRequest<TSecondResponse> secondRequest,
             SideEffectFreeRequest<TThirdResponse> thirdRequest, SideEffectFreeRequest<TFourthResponse> fourthRequest,
             SideEffectFreeRequest<TFifthResponse> fifthRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
            where TThirdResponse : Response, new()
            where TFourthResponse : Response, new()
            where TFifthResponse : Response, new();
    }
}
