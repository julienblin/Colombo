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
