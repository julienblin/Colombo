#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

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
