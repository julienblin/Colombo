using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Colombo
{
    /// <summary>
    /// An exception that include several inner exceptions.
    /// Used when multiple exceptions are raised inside parallel execution.
    /// </summary>
    [Serializable]
    public class ComposedColomboException : ColomboException
    {
        public ComposedColomboException() { }

        public ComposedColomboException(string message, Exception[] innerExceptions) :
            base(message)
        {
            InnerExceptions = innerExceptions;
        }

        protected ComposedColomboException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// All the inner exceptions.
        /// </summary>
        public Exception[] InnerExceptions { get; private set; }
    }
}
