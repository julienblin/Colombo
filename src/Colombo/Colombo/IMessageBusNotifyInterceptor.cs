using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Interceptor for the Notify operation (send a notification).
    /// </summary>
    [ContractClass(typeof(Contracts.MessageBusNotifyInterceptorContract))]
    public interface IMessageBusNotifyInterceptor : IColomboInterceptor
    {
        /// <summary>
        /// Invoked when the notification is intercepted.
        /// </summary>
        void Intercept(IColomboNotifyInvocation invocation);
    }
}
