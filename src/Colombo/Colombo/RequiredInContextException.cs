using System;
using System.Runtime.Serialization;

namespace Colombo
{
    [Serializable]
    public class RequiredInContextException : ColomboException
    {
        public RequiredInContextException() { }

        public RequiredInContextException(string message)
            : base(message)
        { }

        public RequiredInContextException(string message, Exception innerException) :
            base(message, innerException)
        { }

        protected RequiredInContextException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        { }
    }
}
