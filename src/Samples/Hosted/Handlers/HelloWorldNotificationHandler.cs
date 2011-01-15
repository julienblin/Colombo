using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Samples.Messages;
using Castle.Core.Logging;

namespace Colombo.Samples.Hosted.Handlers
{
    public class HelloWorldNotificationHandler : NotificationHandler<HelloWorldNotification>
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        protected override void Handle()
        {
            Logger.InfoFormat("Received Hello world for {0}", Notification.Name);
        }
    }
}
