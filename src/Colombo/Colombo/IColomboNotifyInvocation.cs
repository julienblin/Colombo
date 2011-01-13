using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public interface IColomboNotifyInvocation
    {
        IList<Notification> Notifications { get; set; }

        void Proceed();
    }
}
