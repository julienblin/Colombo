using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Alerts
{
    public class SLABreachedAlert : IColomboAlert
    {
        public SLABreachedAlert(BaseRequest[] requests, TimeSpan allowed, TimeSpan measured)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            Requests = requests;
            Allowed = allowed;
            Measured = measured;
        }

        public BaseRequest[] Requests { get; private set; }

        public TimeSpan Allowed { get; private set; }

        public TimeSpan Measured { get; private set; }

        public override string ToString()
        {
            Contract.Assume(Requests != null);

            return string.Format("SLA breached for {0}: allowed {1} ms, measured {2} ms.",
                string.Join(", ", Requests.Select(x => x.ToString())),
                Allowed.TotalMilliseconds,
                Measured.TotalMilliseconds
            );
        }
    }
}
