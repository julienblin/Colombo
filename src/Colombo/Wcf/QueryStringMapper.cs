using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
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
