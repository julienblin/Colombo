using System;
using System.Runtime.Serialization;

namespace Colombo.TestSupport
{
    /// <summary>
    /// Indicates that an expectation failed.
    /// </summary>
    public class ColomboExpectationException : ColomboTestSupportException
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboExpectationException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboExpectationException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboExpectationException(string message, Exception innerException) :
            base(message, innerException)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ColomboExpectationException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        { }
    }
}
