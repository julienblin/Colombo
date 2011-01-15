using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.MessageBusNotifyInterceptorContract))]
    public interface IMessageBusNotifyInterceptor : IColomboInterceptor
    {
        void Intercept(IColomboNotifyInvocation invocation);
    }
}
