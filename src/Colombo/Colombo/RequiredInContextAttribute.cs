using System;
using System.Collections.Generic;

namespace Colombo
{
    /// <summary>
    /// Indicate that a request must contains certain keys in their Context to be considered valid.
    /// </summary>
    /// <see cref="Colombo.Interceptors.RequiredInContextHandleInterceptor"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RequiredInContextAttribute : Attribute
    {
        private readonly List<string> keys = new List<string>();

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="key">Name of the required key in Context.</param>
        public RequiredInContextAttribute(string key) : this(key, null) { }

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="key">Name of the required key in Context.</param>
        /// <param name="keys">Names of other required keys in Context.</param>
        public RequiredInContextAttribute(string key, params string[] keys)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key must not be null, empty, or consisting of whitespaces.");

            this.keys.Add(key);

            if (keys != null)
                this.keys.AddRange(keys);
        }

        /// <summary>
        /// Get the list of keys.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetKeys()
        {
            return keys;
        }
    }
}
