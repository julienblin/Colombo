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
using System.Runtime.Serialization;
using System.Text;

namespace Colombo.Host
{
    /// <summary>
    /// An exception that occured inside Colombo.Host.
    /// </summary>
    [Serializable]
    public class ColomboHostException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboHostException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboHostException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboHostException(string message, Exception innerException) :
            base(message, innerException)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ColomboHostException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// String representation of the exception.
        /// </summary>
        public override string ToString()
        {
            if ((InnerException == null) || !(InnerException is AggregateException))
                return base.ToString();

            var resultWithAggregate = new StringBuilder(base.ToString());
            var aggregateEx = (AggregateException)InnerException;
            if (aggregateEx.InnerExceptions.Count > 0)
            {
                resultWithAggregate.AppendLine();
                resultWithAggregate.AppendLine("Inner exceptions:");
                foreach (var innerEx in aggregateEx.InnerExceptions)
                {
                    resultWithAggregate.AppendFormat("{0}", innerEx);
                    resultWithAggregate.AppendLine();
                }
            }
            return resultWithAggregate.ToString();
        }
    }
}
