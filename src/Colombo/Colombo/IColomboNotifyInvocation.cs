using System.Collections.Generic;

namespace Colombo
{
    public interface IColomboNotifyInvocation
    {
        IList<Notification> Notifications { get; set; }

        void Proceed();
    }
}
