namespace Colombo
{
    /// <summary>
    /// Invocation used in the interception process for handling notifications.
    /// </summary>
    public interface IColomboNotificationHandleInvocation
    {
        /// <summary>
        /// The notification that will be handled.
        /// </summary>
        Notification Notification { get; set; }

        /// <summary>
        /// Proceed with the invocation.
        /// </summary>
        void Proceed();
    }
}
