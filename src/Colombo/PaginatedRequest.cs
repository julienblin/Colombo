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
using System.ComponentModel.DataAnnotations;

namespace Colombo
{
    /// <summary>
    /// Static class to allow setting the PerPage default value.
    /// </summary>
    public static class PaginatedRequest
    {
        /// <summary>
        /// Default value for <see cref="DefaultPerPageValue"/>.
        /// </summary>
        public const int DefaultPerPageValueIfNotSet = 30;

        private static int defaultPerPageValue = DefaultPerPageValueIfNotSet;
        /// <summary>
        /// Default PaginatedRequest.PerPage value.
        /// Default to 30 if not set.
        /// </summary>
        public static int DefaultPerPageValue
        {
            get { return defaultPerPageValue; }
            set { defaultPerPageValue = value; }
        }
    }

    /// <summary>
    /// Base class for requests that are side-effect free and paginated.
    /// Associated reponses must be <see cref="PaginatedResponse"/>.
    /// </summary>
    public abstract class PaginatedRequest<TResponse> : SideEffectFreeRequest<TResponse>, IPaginationInfo
        where TResponse : PaginatedResponse, new()
    {
        private int currentPage = 1;
        /// <summary>
        /// Current page number, starts at 1 (and not 0).
        /// Defaults to 1.
        /// </summary>
        [Range(1, Int32.MaxValue)]
        public virtual int CurrentPage
        {
            get { return currentPage; }
            set { currentPage = value; }
        }

        private int perPage = PaginatedRequest.DefaultPerPageValue;
        /// <summary>
        /// Number of entries per page.
        /// Default to <see cref="PaginatedRequest.DefaultPerPageValue"/>.
        /// </summary>
        [Range(1, Int32.MaxValue)]
        public virtual int PerPage
        {
            get { return perPage; }
            set { perPage = value; }
        }
    }
}
