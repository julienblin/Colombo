using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Alerts
{
    /// <summary>
    /// This alert means that an AppFabric cache operation was on error.
    /// </summary>
    public class AppFabricCacheErrorAlert : IColomboAlert
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="machineNameOrigin">Name of machine from which the Colombo client executes (source).</param>
        /// <param name="exception">Error raised</param>
        public AppFabricCacheErrorAlert(string machineNameOrigin, Exception exception)
        {
            MachineNameOrigin = machineNameOrigin;
            Exception = exception;
        }

        /// <summary>
        /// Name of machine from which the Colombo client executes (source).
        /// </summary>
        public string MachineNameOrigin { get; private set; }

        /// <summary>
        /// Exception
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// A description of the alert.
        /// </summary>
        public override string ToString()
        {
            return string.Format("AppFabric cache servers error from machine {0}. Potential reason: {1}.",
                MachineNameOrigin,
                Exception);
        }
    }
}
