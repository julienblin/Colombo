using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Alerts
{
    public class SLABreachedAlert : IColomboAlert
    {
        public SLABreachedAlert(BaseRequest request, TimeSpan allowed, TimeSpan measured)
        {
            Request = request;
            Allowed = allowed;
            Measured = measured;
        }

        public BaseRequest Request { get; private set; }

        public TimeSpan Allowed { get; private set; }

        public TimeSpan Measured { get; private set; }

        public override string ToString()
        {
            return string.Format("SLA breached for {0}: allowed {1} ms, measured {2} ms.",
                Request,
                Allowed.TotalMilliseconds,
                Measured.TotalMilliseconds
            );
        }
    }
}
