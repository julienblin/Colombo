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
using System.Collections.Specialized;
using System.Web;
using Castle.Components.Binder;

namespace Colombo.Wcf
{
    /// <summary>
    /// Helper that maps Query string to objects.
    /// </summary>
    public static class QueryStringMapper
    {
        /// <summary>
        /// Map a querystring to a object of type <paramref name="type"/>.
        /// </summary>
        public static object Map(string queryString, Type type)
        {
            return Map(HttpUtility.ParseQueryString(queryString), type);
        }

        /// <summary>
        /// Map a <see cref="NameValueCollection"/> extracted from a query string to an object of type <paramref name="type"/>.
        /// </summary>
        public static object Map(NameValueCollection collection, Type type)
        {
            var treeBuilder = new TreeBuilder();
            var sourceNode = treeBuilder.BuildSourceNode(collection);
            var baseNode = new CompositeNode("");
            baseNode.AddChildNode(sourceNode);

            var binder = new DataBinder();
            try
            {
                return binder.BindObject(type, "root", baseNode);
            }
            catch (Exception ex)
            {
                throw new ColomboException(string.Format("An error occured when binding Query string to {0}", type), ex);
            }
        }
    }
}
