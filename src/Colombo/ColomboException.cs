using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Colombo
{
    /// <summary>
    /// An exception that occured inside Colombo.
    /// </summary>
    [Serializable]
    public class ColomboException : Exception
    {
        public ColomboException() { }

        public ColomboException(string message)
            : base(message)
        { }

        public ColomboException(string message, Exception innerException) :
            base(message, innerException)
        { }

        protected ColomboException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        { }
    }
}
