using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Colombo.TestSupport
{
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
