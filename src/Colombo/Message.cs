using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    [Serializable]
    public abstract class Message
    {
        protected Message()
        {
            CorrelationGuid = Guid.NewGuid();
            Timestamp = DateTime.UtcNow;
        }

        public Guid CorrelationGuid { get; set; }

        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2:yyyy-MM-dd-HH:mm:ss}", GetType().Name, CorrelationGuid, Timestamp);
        }
    }
}
