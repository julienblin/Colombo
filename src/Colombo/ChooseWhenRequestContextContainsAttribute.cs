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
