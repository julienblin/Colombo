using System;

namespace Colombo.Alerts
{
    /// <summary>
    /// This alert means that a healthcheck failed.
    /// </summary>
    public class HealthCheckFailedAlert : IColomboAlert
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="machineNameOrigin">Name of machine from which the health check executes (source).</param>
        /// <param name="address">Address of the endpoint for which the health check failed. (target)</param>
        /// <param name="exception">Exception that occured during the health check.</param>
        public HealthCheckFailedAlert(string machineNameOrigin, string address, Exception exception)
        {
            MachineNameOrigin = machineNameOrigin;
            Address = address;
            Exception = exception;
        }

        /// <summary>
        /// Name of machine from which the health check executes (source).
        /// </summary>
        public string MachineNameOrigin { get; private set; }

        /// <summary>
        /// Address of the endpoint for which the health check failed. (target)
        /// </summary>
        public string Address { get; private set; }

        /// <summary>
        /// Exception that occured during the health check.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// A description of the alert.
        /// </summary>
        public override string ToString()
        {
            return string.Format("A HealthCheck has failed from machine {0} to address {1}. Possible reason : {2}",
                MachineNameOrigin,
                Address,
                Exception
            );
        }
    }
}
