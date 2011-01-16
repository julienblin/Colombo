using System;
using System.Runtime.Serialization;
using System.Text;

namespace Colombo.Host
{
    /// <summary>
    /// An exception that occured inside Colombo.Host.
    /// </summary>
    [Serializable]
    public class ColomboHostException : Exception
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboHostException() { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboHostException(string message)
            : base(message)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ColomboHostException(string message, Exception innerException) :
            base(message, innerException)
        { }

        /// <summary>
        /// Constructor.
        /// </summary>
        protected ColomboHostException(SerializationInfo info,
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
