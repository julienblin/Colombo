#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

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
