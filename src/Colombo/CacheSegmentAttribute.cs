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
    /// Specify the cache segment to use. Must be applied on a request.
    /// The segment can be defined by either a name or a pointer to a value in the Context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CacheSegmentAttribute : Attribute
    {
        /// <summary>
        /// The name of the cache segment to use.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the ContextKey which value will be used for the cache segment.
        /// If no value is in the Context and a <see cref="Name"/> is provided, the Name will be used instead.
        /// </summary>
        public string FromContextKey { get; set; }

        /// <summary>
        /// The name of a Property which value will be used for the cache segment.
        /// If the value of the property is null and a <see cref="Name"/> is provided, the Name will be used instead.
        /// </summary>
        public string FromProperty { get; set; }

        /// <summary>
        /// Specify the cache segment to use. Must be applied on a request.
        /// </summary>
        public CacheSegmentAttribute()
        {
        }

        /// <summary>
        /// Specify the cache segment to use. Must be applied on a request.
        /// </summary>
        /// <param name="name">Name of the cache segment.</param>
        public CacheSegmentAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Returns the cache segment, specified either by the <see cref="FromContextKey"/> or <see cref="Name"/> properties.
        /// The FromContextKey has priority over the Name.
        /// </summary>
        public string GetCacheSegment(BaseRequest request)
        {
            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(FromContextKey) && string.IsNullOrEmpty(FromProperty))
                throw new ColomboException(string.Format("Unable to determine cache segment for {0}. You must specified either the Name, the FromContextKey or the FromProperty member.", request));

            if (!string.IsNullOrEmpty(FromContextKey) && !string.IsNullOrEmpty(FromProperty))
                throw new ColomboException(string.Format("Unable to determine cache segment for {0}. You cannot specify both FromContextKey and FromProperty.", request));

            if (!string.IsNullOrEmpty(FromContextKey))
            {
                if (request.Context.ContainsKey(FromContextKey))
                    return request.Context[FromContextKey];

                if (string.IsNullOrEmpty(Name))
                    throw new ColomboException(string.Format("The cache segment for {0} is supposed to come from the context key {1}, but it doesn't exists on Context and no Name has been given.",
                        request,
                        FromContextKey));
            }

            if (!string.IsNullOrEmpty(FromProperty))
            {
                var propertyInfo = request.GetType().GetProperty(FromProperty);
                if(propertyInfo == null)
                    throw new ColomboException(string.Format("Property {0} not found on {1} - unable to compute cache segment.", FromProperty, request));

                var propertyGetMethod = propertyInfo.GetGetMethod();
                if (propertyGetMethod == null)
                    throw new ColomboException(string.Format("Property {0} on {1} doesn't have a public get accessor - unable to compute cache segment.", FromProperty, request));

                var propertyValue = propertyGetMethod.Invoke(request, null);

                if (propertyValue != null)
                    return propertyValue.ToString();

                if (string.IsNullOrEmpty(Name))
                    throw new ColomboException(string.Format("The cache segment for {0} is supposed to come from the property {1}, but its value is null and no Name has been given.",
                        request,
                        FromProperty));
            }

            return Name;
        }
    }
}
