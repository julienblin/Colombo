using System;
using System.Collections.Generic;

namespace Colombo
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class InvalidateCachedInstancesOfAttribute : Attribute
    {
        public InvalidateCachedInstancesOfAttribute(Type responseType)
            : this(responseType, null)
        {
        }

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

        public IEnumerable<Type> ResponsesTypes { get; set; }
    }
}
