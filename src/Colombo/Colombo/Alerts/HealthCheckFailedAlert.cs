using System;

namespace Colombo.Alerts
{
    public class HealthCheckFailedAlert : IColomboAlert
    {
        public HealthCheckFailedAlert(string machineNameOrigin, string address, Exception exception)
        {
            MachineNameOrigin = machineNameOrigin;
            Address = address;
            Exception = exception;
        }

        public string MachineNameOrigin { get; private set; }

        public string Address { get; private set; }

        public Exception Exception { get; private set; }

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
