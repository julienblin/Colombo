using System;
using System.Collections.Generic;

namespace Colombo
{
    /// <summary>
    /// Indicates that whenever the request annotated with this attribute is sent, Colombo needs to invalidate all the responses
    /// of the types indicated that belongs to the same cache segment.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class InvalidateCachedInstancesOfAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="responseType">The type of the response to invalidate.</param>
        public InvalidateCachedInstancesOfAttribute(Type responseType)
            : this(responseType, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="responseType">The type of the response to invalidate.</param>
        /// <param name="responsesType">Other response types to invalidate.</param>
        public InvalidateCachedInstancesOfAttribute(Type responseType, params Type[] responsesType)
        {
            if (responseType == null) throw new ArgumentNullException("responseType");

            var responsesTypes = new List<Type> { responseType };

            if (responsesType != null)
                responsesTypes.AddRange(responsesType);

            foreach (var t in responsesTypes)
            {
                if (!typeof(Response).IsAssignableFrom(t))
                    throw new ColomboException(string.Format("Type arguments passed to InvalidateCachedInstancesOf must derived from Response. Invalid parameter: {0}", t));
            }

            ResponsesTypes = responsesTypes;
        }

        /// <summary>
        /// Response types to invalidate.
        /// </summary>
        public IEnumerable<Type> ResponsesTypes { get; set; }
    }
}
