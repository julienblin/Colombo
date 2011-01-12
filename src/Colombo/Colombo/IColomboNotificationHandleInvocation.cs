using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public interface IColomboNotificationHandleInvocation
    {
        Notification Notification { get; set; }

        void Proceed();
    }
}
