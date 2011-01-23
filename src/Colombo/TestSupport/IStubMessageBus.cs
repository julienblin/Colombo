using System;
using Castle.MicroKernel;

namespace Colombo.TestSupport
{
    /// <summary>
    /// Allow the setup and verifications of expectations - to be used inside unit tests of handlers.
    /// </summary>
    public interface IStubMessageBus
    {
        /// <summary>
        /// The <see cref="IKernel"/> that will be injected.
        /// </summary>
        IKernel Kernel { get; set; }

        /// <summary>
        /// Indicates a handler type that is under test.
        /// </summary>
        RequestHandlerExpectation<THandler> TestHandler<THandler>()
            where THandler : IRequestHandler;

        /// <summary>
        /// Indicates an expectation that a type of Request will be sent.
        /// </summary>
        MessageBusSendExpectation<TRequest, TResponse> ExpectSend<TRequest, TResponse>()
            where TRequest : BaseRequest, new()
            where TResponse : Response, new();

        /// <summary>
        /// Indicates an expectation that a type of <see cref="Notification"/> will be notified.
        /// </summary>
        /// <typeparam name="TNotification"></typeparam>
        /// <returns></returns>
        MessageBusNotifyExpectation<TNotification> ExpectNotify<TNotification>()
            where TNotification : Notification, new();

        /// <summary>
        /// <c>true</c> to allow the <see cref="IStubMessageBus"/> to reply to requests using empty responses,
        /// <c>false</c> to disallow and throw a <see cref="ColomboExpectationException"/> when sending an unexpected request or notification.
        /// </summary>
        bool AllowUnexpectedMessages { get; set; }

        /// <summary>
        /// Returns the <see cref="BaseExpectation"/> associated with the <paramref name="messageType"/>
        /// </summary>
        BaseExpectation GetExpectationFor(Type messageType);

        /// <summary>
        /// Verify all the expectations
        /// </summary>
        /// <exception cref="ColomboExpectationException" />
        void Verify();
    }
}
