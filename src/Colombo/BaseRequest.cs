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
using System.Diagnostics.Contracts;
using System.Linq;

namespace Colombo
{
    /// <summary>
    /// Base class for requests - non generic version.
    /// </summary>
    public abstract class BaseRequest : BaseMessage, ICacheable
    {
        private IDictionary<string, string> context;
        /// <summary>
        /// Context of the request. Garanteed to be non-null.
        /// </summary>
        public virtual IDictionary<string, string> Context
        {
            get { return context ?? (context = new Dictionary<string, string>()); }
            set
            {
                if (value == null) throw new ArgumentNullException("Context");
                Contract.EndContractBlock();
                context = value;
            }
        }

        /// <summary>
        /// Get the name of the group that this request belongs to.
        /// </summary>
        /// <remarks>
        /// Defaults to the name of the assembly.
        /// </remarks>
        /// <returns></returns>
        public virtual string GetGroupName()
        {
            return GetType().Assembly.GetName().Name;
        }

        /// <summary>
        /// Get the System.Type of the response associated with this request.
        /// </summary>
        /// <returns></returns>
        public abstract Type GetResponseType();

        /// <summary>
        /// <c>true</c> is this request is side-effect free, <c>false</c> otherwise.
        /// </summary>
        public abstract bool IsSideEffectFree { get; }

        /// <summary>
        /// Create a Response object that is related to the current request.
        /// </summary>
        public virtual Response CreateResponse()
        {
            var responseType = GetResponseType();
            if((responseType == null) || (!typeof(Response).IsAssignableFrom(responseType)))
                throw new ColomboException(string.Format("Unable to create a response from response type {0}: either null or not derived from Response.", responseType));
            
            var response = (Response)Activator.CreateInstance(responseType);
            response.CorrelationGuid = CorrelationGuid;
            return response;
        }

        /// <summary>
        /// <see cref="BaseMessage.ToString"/>. Adds Context information.
        /// </summary>
        public override string ToString()
        {
            if ((Context != null) && (Context.Count > 0))
                return string.Format("{0} | {1}", base.ToString(), string.Join(", ", Context.Select(x => string.Format("{0}={1}", x.Key, x.Value))));
            
            return base.ToString();
        }

        /// <summary>
        /// Returns the key that is meant to be used when put in a cache.
        /// The default implementation throws an exception, this means you must explicitly override it in Request classes.
        /// </summary>
        public virtual string GetCacheKey()
        {
            throw new ColomboException("You must provide an implementation of GetCacheKey() to use the Cache functionnality.");
        }
    }
}
