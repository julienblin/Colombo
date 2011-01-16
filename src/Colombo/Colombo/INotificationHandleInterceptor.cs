namespace Colombo
{
    /// <summary>
    /// Interceptor for the handle request operation.
    /// </summary>
    public interface INotificationHandleInterceptor : IColomboInterceptor
    {
        /// <summary>
        /// Invoked when the request handling is intercepted.
        /// </summary>
        void Intercept(IColomboNotificationHandleInvocation invocation);
    }
}
