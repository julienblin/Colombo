using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;

namespace Colombo.Wcf
{
    public static class QueryStringMapper
    {
        public static object Map(string queryString, Type type)
        {
            return Map(HttpUtility.ParseQueryString(queryString), type, string.Empty);
        }

        public static object Map(NameValueCollection collection, Type type, string prefix)
        {
            try
            {
                var returnObject = Activator.CreateInstance(type);

                foreach (var property in type.GetProperties())
                {
                    foreach (var key in collection.AllKeys)
                    {
                        if (String.IsNullOrEmpty(prefix) || key.Length > prefix.Length)
                        {
                            var propertyNameToMatch = String.IsNullOrEmpty(prefix) ? key : key.Substring(property.Name.IndexOf(prefix) + prefix.Length + 1);

                            if (property.Name == propertyNameToMatch)
                            {
                                property.SetValue(returnObject, Convert.ChangeType(collection.Get(key), property.PropertyType), null);
                            }
                            else if (property.GetValue(returnObject, null) == null)
                            {
                                property.SetValue(returnObject, Map(collection, property.PropertyType, String.Concat(prefix, property.PropertyType.Name)), null);
                            }
                        }
                    }
                }

                return returnObject;
            }
            catch (MissingMethodException)
            {
                //Quite a blunt way of dealing with Types without default constructor
                return null;
            }
        }
    }
}
