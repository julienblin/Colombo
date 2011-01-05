using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public abstract class BaseMessage
    {
        /// <summary>
        /// Base class for all the Colombo messages.
        /// </summary>
        protected BaseMessage()
        {
            CorrelationGuid = Guid.NewGuid();
            UtcTimestamp = DateTime.UtcNow;
        }

        /// <summary>
        /// Represents an identifier that could relate several messages together.
        /// </summary>
        public virtual Guid CorrelationGuid { get; set; }

        /// <summary>
        /// Timestamp for the creation of the message, expressed as UTC.
        /// </summary>
        public virtual DateTime UtcTimestamp { get; set; }

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2:yyyy-MM-dd-HH:mm:ss}", GetType().Name, CorrelationGuid, UtcTimestamp);
        }
    }
}
