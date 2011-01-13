using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Castle.Core.Logging;

namespace Colombo.Impl.Notify
{
    public class NotificationProcessorNotifyInvocation : BaseNotifyInvocation
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

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
            foreach (var processor in notificationProcessors)
            {
                Task.Factory.StartNew((proc) =>
                {
                    ((INotificationProcessor)proc).Process(finalNotificationsArray);
                },
                processor);
            }
        }
    }
}
