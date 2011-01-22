using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

namespace Colombo.Wcf
{
    [ServiceContract(Namespace = "")]
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WcfJsBridgeService
    {
        private readonly static Dictionary<string, Type> GetTypeMapping = new Dictionary<string, Type>();
        private readonly static Dictionary<string, Type> PostTypeMapping = new Dictionary<string, Type>();
        private readonly static List<Type> KnownTypes = new List<Type>();

        public static void RegisterRequestType(Type requestType)
        {
            if(!typeof(BaseRequest).IsAssignableFrom(requestType))
                throw new ColomboException(string.Format("{0} is no assignable to BaseRequest. You can only register BaseRequest types.", requestType));

            var request = (BaseRequest)Activator.CreateInstance(requestType);
            var responseType = request.GetResponseType();

            var messageName = requestType.Name.Replace("Request", string.Empty);

            if(request.IsSideEffectFree)
                GetTypeMapping[messageName] = requestType;
            else
                PostTypeMapping[messageName] = requestType;

            if (!KnownTypes.Contains(requestType))
                KnownTypes.Add(requestType);

            if (!KnownTypes.Contains(responseType))
                KnownTypes.Add(responseType);
        }

        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
        {
            return KnownTypes;
        }

        [OperationContract]
        [WebGet(UriTemplate = "/{messageName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [ServiceKnownType("GetKnownTypes", typeof(WcfJsBridgeService))]
        public Response InvokeGet(string messageName)
        {
            if(!GetTypeMapping.ContainsKey(messageName))
                throw new ColomboException(string.Format("No request with the name {0} or {0}Request has been registered. You must register your request type using WcfJsBridgeService.RegisterRequestType before using the bridge.", messageName));

            var properties = OperationContext.Current.IncomingMessageProperties;
            var property = (HttpRequestMessageProperty)properties[HttpRequestMessageProperty.Name];
            var parameters = HttpUtility.ParseQueryString(property.QueryString);

            var requestType = GetTypeMapping[messageName];
            var request = (BaseRequest)RecurseType(parameters, requestType, string.Empty);

            return WcfServices.Process(request);
        }

        private static object RecurseType(NameValueCollection collection, Type type, string prefix)
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
                                property.SetValue(returnObject, RecurseType(collection, property.PropertyType, String.Concat(prefix, property.PropertyType.Name)), null);
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
