using System;
using System.Runtime.Serialization;
using System.Text;

namespace Colombo.Host
{
    [Serializable]
    public class ColomboHostException : Exception
    {
        public ColomboHostException() { }

        public ColomboHostException(string message)
            : base(message)
        { }

        public ColomboHostException(string message, Exception innerException) :
            base(message, innerException)
        { }

        protected ColomboHostException(SerializationInfo info,
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
