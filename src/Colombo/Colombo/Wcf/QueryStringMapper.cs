using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;

namespace Colombo.Wcf
{
    public static class QueryStringMapper
    {
        public static object Map(string queryString, Type type)
        {
            return Map(HttpUtility.ParseQueryString(queryString), type);
        }

        public static object Map(NameValueCollection collection, Type type)
        {
            object result = null;
            try
            {
                result = Activator.CreateInstance(type);
            }
            catch (MissingMethodException ex)
            {
                throw new ColomboException(string.Format("Unable to create object of type {0}: No default constructor was found.", type), ex);
            }

            MapInstance(collection, result);
            return result;
        }

        public static void MapInstance(NameValueCollection collection, object instance)
        {
            var objectType = instance.GetType();
            foreach (var key in collection.AllKeys)
            {
                var indexOfDot = key.IndexOf('.');

                if (indexOfDot != -1)
                {
                    var propertyName = key.Substring(0, indexOfDot);
                    var leadingPropertyName = key.Substring(key.IndexOf('.') + 1);
                    var property = objectType.GetProperty(propertyName);
                    if (property == null)
                        throw new ColomboException(string.Format("Property with name {0} not found on {1}.", propertyName, instance));

                    var propertyInstance = property.GetValue(instance, null);
                    if (propertyInstance == null)
                    {
                        try
                        {
                            propertyInstance = Activator.CreateInstance(property.PropertyType);
                            property.SetValue(instance, propertyInstance, null);
                        }
                        catch (MissingMethodException ex)
                        {
                            throw new ColomboException(string.Format("Unable to create object of type {0}: No default constructor was found.", property.PropertyType), ex);
                        }
                    }
                    MapInstance(new NameValueCollection { { leadingPropertyName, collection.Get(key) } }, propertyInstance);
                }

                if(indexOfDot == -1)
                {
                    var property = objectType.GetProperty(key);
                    if (property == null)
                        throw new ColomboException(string.Format("Unable to map property {0} to {1}", key, instance));

                    property.SetValue(instance, Convert.ChangeType(collection.Get(key), property.PropertyType, CultureInfo.InvariantCulture), null);
                }
            }
        }
    }
}
