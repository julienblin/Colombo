using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace Colombo.Impl.Notify
{
    internal class NotificationProcessorNotifyInvocation : BaseNotifyInvocation
    {
        private readonly INotificationProcessor[] notificationProcessors;

        public NotificationProcessorNotifyInvocation(INotificationProcessor[] notificationProcessors)
        {
            if (notificationProcessors == null) throw new ArgumentNullException("notificationProcessors");
            Contract.EndContractBlock();

            this.notificationProcessors = notificationProcessors;
        }

        public override void Proceed()
        {
            var finalNotificationsArray = Notifications.ToArray();
            foreach (var processor in notificationProcessors.Where(processor => processor != null))
            {
                Task.Factory.StartNew(proc => ((INotificationProcessor)proc).Process(finalNotificationsArray), processor);
            }
        }
    }
}
