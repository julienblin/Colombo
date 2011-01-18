using System.Collections.Generic;

namespace Colombo.Impl.Notify
{
    public abstract class BaseNotifyInvocation : IColomboNotifyInvocation
    {
        public IList<Notification> Notifications {get;set;}

        public abstract void Proceed();
    }
}
