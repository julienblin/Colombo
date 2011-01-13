using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Impl.Notify
{
    public abstract class BaseNotifyInvocation : IColomboNotifyInvocation
    {
        public IList<Notification> Notifications {get;set;}

        public abstract void Proceed();
    }
}
