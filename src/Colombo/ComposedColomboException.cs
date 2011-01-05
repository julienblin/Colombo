using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Colombo
{
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

        public Exception[] InnerExceptions { get; private set; }
    }
}
