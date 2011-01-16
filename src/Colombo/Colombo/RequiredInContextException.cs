using System;
using System.Runtime.Serialization;

namespace Colombo
{
    /// <summary>
    /// An exception raised when a request indicates required keys, but some are missing.
    /// </summary>
    [Serializable]
    public class RequiredInContextException : ColomboException
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RequiredInContextException() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public RequiredInContextException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        public RequiredInContextException(string message, Exception innerException) :
            base(message, innerException)
        { }

        /// <summary>
        /// Constructor
        /// </summary>
        protected RequiredInContextException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        { }
    }
}
