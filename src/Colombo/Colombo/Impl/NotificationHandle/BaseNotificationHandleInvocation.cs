namespace Colombo.Impl.NotificationHandle
{
    internal abstract class BaseNotificationHandleInvocation : IColomboNotificationHandleInvocation
    {
        public Notification Notification { get; set; }

        public abstract void Proceed();
    }
}
