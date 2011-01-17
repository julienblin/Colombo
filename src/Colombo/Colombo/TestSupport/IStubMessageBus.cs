using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel;

namespace Colombo.TestSupport
{
    public interface IStubMessageBus
    {
        IKernel Kernel { get; set; }

        RequestHandlerExpectation<THandler> TestHandler<THandler>()
            where THandler : IRequestHandler;

        MessageBusSendExpectation<TRequest, TResponse> Expect<TRequest, TResponse>()
            where TRequest : BaseRequest, new()
            where TResponse : Response, new();

        MessageBusNotifyExpectation<TNotification> Expect<TNotification>()
            where TNotification : Notification, new();

        bool AllowUnexpectedMessages { get; set; }

        BaseExpectation GetExpectationFor(Type messageType);

        void Verify();
    }
}
