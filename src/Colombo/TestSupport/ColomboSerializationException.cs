using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Colombo.TestSupport
{
    /// <summary>
    /// An exception thrown when requests or responses could not be serialized.
    /// <see cref="ColomboTest"/>
    /// </summary>
    [Serializable]
    public class ColomboSerializationException : ColomboTestSupportException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboSerializationException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboSerializationException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboSerializationException(string message, Exception innerException) :
            base(message, innerException)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ColomboSerializationException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        { }
    }
}
