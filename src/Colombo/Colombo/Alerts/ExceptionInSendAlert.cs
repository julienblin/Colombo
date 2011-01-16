using System;
using System.Linq;

namespace Colombo.Alerts
{
    /// <summary>
    /// This alert means that an exception occured when sending a request.
    /// </summary>
    public class ExceptionInSendAlert : IColomboAlert
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="requests">Requests that where sent.</param>
        /// <param name="ex">Exception that occured during the send.</param>
        public ExceptionInSendAlert(BaseRequest[] requests, Exception ex)
        {
            Requests = requests;
            Exception = ex;
        }

        /// <summary>
        /// Requests that where sent.
        /// </summary>
        public BaseRequest[] Requests { get; private set; }

        /// <summary>
        /// Exception that occured during the send.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// A description of the alert.
        /// </summary>
        public override string ToString()
        {
            return string.Format("An exception occured when sending {0}. Possible reason : {1}",
                string.Join(", ", Requests.Select(x => x.ToString())),
                Exception
            );
        }
    }
}
