using System;
using System.Runtime.Serialization;
using System.Text;

namespace Colombo
{
    /// <summary>
    /// An exception that occured inside Colombo.
    /// </summary>
    [Serializable]
    public class ColomboException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboException(string message, Exception innerException) :
            base(message, innerException)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ColomboException(SerializationInfo info,
           StreamingContext context)
            : base(info, context)
        { }

        /// <summary>
        /// String representation of the exception.
        /// </summary>
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
