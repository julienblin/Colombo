using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

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

        public override string ToString()
        {
            if ((InnerException == null) || !(InnerException is AggregateException))
                return base.ToString();

            var resultWithAggregate = new StringBuilder(base.ToString());
            var aggregateEx = (AggregateException)InnerException;
            if (aggregateEx.InnerExceptions.Count > 0)
            {
                resultWithAggregate.AppendLine();
                resultWithAggregate.AppendLine("Inner exceptions:");
                foreach (var innerEx in aggregateEx.InnerExceptions)
                {
                    resultWithAggregate.AppendFormat("{0}", innerEx);
                    resultWithAggregate.AppendLine();
                }
            }
            return resultWithAggregate.ToString();
        }
    }
}
