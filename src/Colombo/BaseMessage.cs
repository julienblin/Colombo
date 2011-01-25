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

namespace Colombo
{
    /// <summary>
    /// Base class for all the Colombo messages.
    /// </summary>
    [DataContract]
    public abstract class BaseMessage
    {
        private Guid correlationGuid = Guid.NewGuid();
        /// <summary>
        /// Represents an identifier that could relate several messages together.
        /// </summary>
        [DataMember]
        public virtual Guid CorrelationGuid
        {
            get { return correlationGuid; }
            set { correlationGuid = value; }
        }

        private DateTime utcTimestamp = DateTime.UtcNow;
        /// <summary>
        /// Timestamp for the creation of the message, expressed as UTC.
        /// </summary>
        [DataMember]
        public virtual DateTime UtcTimestamp
        {
            get { return utcTimestamp; }
            set { utcTimestamp = value; }
        }

        /// <summary>
        /// A standard message representation : {GetType} | {CorrelationGuid} | {UtcTimestamp}
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} | {1} | {2:yyyy-MM-dd-HH:mm:ss}", GetType().Name, CorrelationGuid, UtcTimestamp);
        }
    }
}
