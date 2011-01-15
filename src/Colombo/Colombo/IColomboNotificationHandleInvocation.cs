namespace Colombo
{
    public interface IColomboNotificationHandleInvocation
    {
        Notification Notification { get; set; }

        void Proceed();
    }
}
