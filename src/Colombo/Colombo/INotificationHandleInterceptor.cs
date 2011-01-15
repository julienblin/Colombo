namespace Colombo
{
    public interface INotificationHandleInterceptor : IColomboInterceptor
    {
        void Intercept(IColomboNotificationHandleInvocation invocation);
    }
}
