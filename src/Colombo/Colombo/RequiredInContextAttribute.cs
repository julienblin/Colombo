using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RequiredInContextAttribute : Attribute
    {
        private readonly List<string> keys = new List<string>();

        public RequiredInContextAttribute(string key) : this(key, null) { }

        public RequiredInContextAttribute(string key, params string[] keys)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("key must not be null, empty, or consisting of whitespaces.");

            this.keys.Add(key);

            if (keys != null)
                this.keys.AddRange(keys);
        }

        public IEnumerable<string> GetKeys()
        {
            return keys;
        }
    }
}
