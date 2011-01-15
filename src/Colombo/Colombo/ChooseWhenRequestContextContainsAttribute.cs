using System;

namespace Colombo
{
    /// <summary>
    /// Apply to a request handler to allow specialization of the request handler based on request.Context values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class ChooseWhenRequestContextContainsAttribute : Attribute
    {
        private readonly string key;
        private readonly string value;

        /// <summary>
        /// Apply to a request handler to allow specialization of the request handler based on Context values.
        /// </summary>
        /// <param name="key">Name of the key which, when not null, will make this request handler chosen.</param>
        public ChooseWhenRequestContextContainsAttribute(string key)
            : this(key, null)
        {
        }

        /// <summary>
        /// Apply to a request handler to allow specialization of the request handler based on Context values.
        /// </summary>
        /// <param name="key">Name of the key which, when the value is equal to <paramref name="value"/>, will make this request handler chosen.</param>
        /// <param name="value">Value associated with the key in request context.</param>
        public ChooseWhenRequestContextContainsAttribute(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException("key");

            this.key = key;
            this.value = value;
        }

        /// <summary>
        /// Determine if the request handler should be chosen.
        /// </summary>
        public bool IsChoosen(BaseRequest request)
        {
            if (value == null)
                return request.Context.ContainsKey(key);
            
            return request.Context.ContainsKey(key) && request.Context[key].Equals(value, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
