using System;
using System.Linq;

namespace Colombo.Alerts
{
    /// <summary>
    /// This alert means that an exception occured.
    /// </summary>
    public class ExceptionAlert : IColomboAlert
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="requests">Requests that where operated.</param>
        /// <param name="ex">Exception that occured.</param>
        public ExceptionAlert(BaseRequest[] requests, Exception ex)
        {
            Requests = requests;
            Exception = ex;
        }

        /// <summary>
        /// Requests that where operated.
        /// </summary>
        public BaseRequest[] Requests { get; private set; }

        /// <summary>
        /// Exception that occured.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// A description of the alert.
        /// </summary>
        public override string ToString()
        {
            return string.Format("An exception occured when operating {0}. Possible reason : {1}",
                string.Join(", ", Requests.Select(x => x.ToString())),
                Exception
            );
        }
    }
}
