using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Colombo
{
    /// <summary>
    /// Base class for all the Colombo messages.
    /// </summary>
    [DataContract]
    public abstract class BaseMessage
    {
        protected BaseMessage()
        {
        }

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

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2:yyyy-MM-dd-HH:mm:ss}", GetType().Name, CorrelationGuid, UtcTimestamp);
        }
    }
}
