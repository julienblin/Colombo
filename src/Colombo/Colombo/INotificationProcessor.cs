using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Represents a processor that can process notifications.
    /// </summary>
    [ContractClass(typeof(Contracts.NotificationProcessorContract))]
    public interface INotificationProcessor
    {
        /// <summary>
        /// Process the notifications.
        /// </summary>
        void Process(Notification[] notifications);
    }
}
