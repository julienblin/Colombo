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
