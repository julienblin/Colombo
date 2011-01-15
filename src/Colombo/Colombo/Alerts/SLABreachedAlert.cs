using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Colombo.Alerts
{
    /// <summary>
    /// Alert that means that a SLA for a request has not been respected.
    /// </summary>
    public class SLABreachedAlert : IColomboAlert
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requests">The requests that where sent.</param>
        /// <param name="allowed">The allowed time defined by the SLA.</param>
        /// <param name="measured">The measured time for the operation.</param>
        public SLABreachedAlert(BaseRequest[] requests, TimeSpan allowed, TimeSpan measured)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            Requests = requests;
            Allowed = allowed;
            Measured = measured;
        }

        /// <summary>
        /// The requests that where sent.
        /// </summary>
        public BaseRequest[] Requests { get; private set; }

        /// <summary>
        /// The allowed time defined by the SLA.
        /// </summary>
        public TimeSpan Allowed { get; private set; }

        /// <summary>
        /// The measured time for the operation.
        /// </summary>
        public TimeSpan Measured { get; private set; }

        /// <summary>
        /// A description of the alert.
        /// </summary>
        public override string ToString()
        {
            Contract.Assume(Requests != null);

            return string.Format("SLA breached for {0}: allowed {1} ms, measured {2} ms.",
                Requests == null ? "" : string.Join(", ", Requests.Select(x => x.ToString())),
                Allowed.TotalMilliseconds,
                Measured.TotalMilliseconds
            );
        }
    }
}
