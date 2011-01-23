using System;
using System.Runtime.Serialization;

namespace Colombo.TestSupport
{
    /// <summary>
    /// An exception that occured inside a test support utilization.
    /// <see cref="ColomboTest"/>
    /// </summary>
    [Serializable]
    public class ColomboTestSupportException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboTestSupportException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboTestSupportException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboTestSupportException(string message, Exception innerException) :
            base(message, innerException)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ColomboTestSupportException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        { }
    }
}
