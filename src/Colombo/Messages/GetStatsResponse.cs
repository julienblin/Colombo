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

namespace Colombo.Messages
{
    /// <summary>
    /// Gives statistics information.
    /// </summary>
    public class GetStatsResponse : Response
    {
        /// <summary>
        /// <c>true</c> is statistics are availabe, <c>false</c> if not.
        /// </summary>
        public virtual bool StatsAvailable { get; set; }

        /// <summary>
        /// Uptime.
        /// </summary>
        public virtual TimeSpan Uptime { get; set; }

        /// <summary>
        /// Number of requests handled by the local processor.
        /// </summary>
        public virtual int NumRequestsHandled { get; set; }

        /// <summary>
        /// Number of errors (Exception) that occured when handling requests.
        /// </summary>
        public virtual int NumErrors { get; set; }

        /// <summary>
        /// Average time spent per request handled by the local processor.
        /// </summary>
        public virtual TimeSpan AverageTimePerRequestHandled { get; set; }

        /// <summary>
        /// The percentage of errors / requests handled.
        /// </summary>
        public virtual decimal ErrorRate { get; set; }

        /// <summary>
        /// Version number for the Colombo component.
        /// </summary>
        public virtual string ColomboVersion { get; set; }
    }
}
