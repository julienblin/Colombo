using System;
using System.Runtime.Serialization;

namespace Colombo.TestSupport
{
    /// <summary>
    /// An exception that occured inside a unit test support utilization.
    /// <see cref="ColomboTest"/>
    /// </summary>
    [Serializable]
    public class ColomboTestException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboTestException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboTestException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboTestException(string message, Exception innerException) :
            base(message, innerException)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ColomboTestException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        { }
    }
}
