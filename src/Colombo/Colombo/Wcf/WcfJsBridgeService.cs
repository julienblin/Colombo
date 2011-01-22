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
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class WcfJsBridgeService
    {
        private readonly static Dictionary<string, Type> GetTypeMapping = new Dictionary<string, Type>();
        private readonly static Dictionary<string, Type> PostTypeMapping = new Dictionary<string, Type>();
        private readonly static List<Type> KnownTypes = new List<Type>();

        public static void RegisterRequestType(Type requestType)
        {
            if (!typeof(BaseRequest).IsAssignableFrom(requestType))
                throw new ColomboException(string.Format("{0} is no assignable to BaseRequest. You can only register BaseRequest types.", requestType));

            var request = (BaseRequest)Activator.CreateInstance(requestType);
            var responseType = request.GetResponseType();

            var messageName = requestType.Name.Replace("Request", string.Empty);

            if (request.IsSideEffectFree)
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
            if (!GetTypeMapping.ContainsKey(messageName))
            {
                if (PostTypeMapping.ContainsKey(messageName))
                    throw new ColomboException(string.Format("{0} is not a side-effect free request. You must invoke using POST, not GET.", PostTypeMapping[messageName]));

                throw new ColomboException(string.Format("No request with the name {0} or {0}Request has been registered. You must register your request type using WcfJsBridgeService.RegisterRequestType before using the bridge.", messageName));
            }

            var requestType = GetTypeMapping[messageName];
            var properties = OperationContext.Current.IncomingMessageProperties;
            var property = (HttpRequestMessageProperty)properties[HttpRequestMessageProperty.Name];

            var request = (BaseRequest)QueryStringMapper.Map(property.QueryString, requestType);

            return WcfServices.Process(request);
        }

        [OperationContract]
        [WebInvoke(UriTemplate = "/{messageName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [ServiceKnownType("GetKnownTypes", typeof(WcfJsBridgeService))]
        public Response InvokePost(string messageName)
        {
            if (!PostTypeMapping.ContainsKey(messageName))
            {
                if (GetTypeMapping.ContainsKey(messageName))
                    throw new ColomboException(string.Format("{0} is a side-effect free request. You must invoke using GET, not POST.", GetTypeMapping[messageName]));

                throw new ColomboException(string.Format("No request with the name {0} or {0}Request has been registered. You must register your request type using WcfJsBridgeService.RegisterRequestType before using the bridge.", messageName));
            }

            var requestType = PostTypeMapping[messageName];
            var properties = OperationContext.Current.IncomingMessageProperties;
            var property = (HttpRequestMessageProperty)properties[HttpRequestMessageProperty.Name];

            var request = (BaseRequest)QueryStringMapper.Map(property.QueryString, requestType);

            return WcfServices.Process(request);
        }
    }
}
