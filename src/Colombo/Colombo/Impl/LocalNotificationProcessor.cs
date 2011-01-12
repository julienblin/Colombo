using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;

namespace Colombo.Impl
{
    public class LocalNotificationProcessor : INotificationProcessor
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        public void Process(Notification[] notifications)
        {
            throw new NotImplementedException();
        }
    }
}
