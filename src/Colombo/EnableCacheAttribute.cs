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
    /// Allow a Colombo client to puts responses associated with requests of this type to puts them in a cache.
    /// To enable caching, you have to either register a <see cref="Colombo.Caching.IColomboCache"/> or use 
    /// <seealso cref="Colombo.Facilities.ColomboFacility.EnableInMemoryCaching"/> / <seealso cref="Colombo.Facilities.ColomboFacility.EnableMemcachedCaching"/>.
    /// To allow a request to be put in cache you have to implement <see cref="ICacheable.GetCacheKey"/>.
    /// Cache segments can be controlled via <see cref="CacheSegmentAttribute"/>, and expiration is time-based or controlled by <see cref="InvalidateCachedInstancesOfAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class EnableCacheAttribute : Attribute
    {
        /// <summary>
        /// Number of hours before the cached response will expired.
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// Number of minutes before the cached response will expired.
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Number of seconds before the cached response will expired.
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// Duration before the cached response will expired.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                if (Hours == 0 && Minutes == 0 && Seconds == 0)
                {
                    throw new ColomboException("You need to specify at least an hour value, a minute value or a second value.");
                }

                return new TimeSpan(0, Hours, Minutes, Seconds);
            }
        }
    }
}
