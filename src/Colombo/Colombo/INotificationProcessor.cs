using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.NotificationProcessorContract))]
    public interface INotificationProcessor
    {
        void Process(Notification[] notifications);
    }
}
