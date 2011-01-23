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
    /// <summary>
    /// Service that exposes requests to Javascript GET and POST XMLHTTPRequests.
    /// Must be used inside an ASP.NET application, il will then use <see cref="IMessageBus"/> to send the requests normally.
    /// Requests must be registered to be exposed.
    /// </summary>
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class ClientRestService
    {
        internal readonly static Dictionary<string, Type> GetTypeMapping = new Dictionary<string, Type>();
        internal readonly static Dictionary<string, Type> PostTypeMapping = new Dictionary<string, Type>();
        private readonly static List<Type> KnownTypes = new List<Type>();

        /// <summary>
        /// Register a request of type <typeparamref name="TRequest"/>, it will be available to <see cref="ClientRestService"/>.
        /// </summary>
        public static void RegisterRequest<TRequest>()
            where TRequest : BaseRequest, new()
        {
            RegisterRequest(typeof(TRequest));
        }

        /// <summary>
        /// Register a request of type <paramref name="requestType"/>, it will be available to <see cref="ClientRestService"/>.
        /// </summary>
        public static void RegisterRequest(Type requestType)
        {
            if (!typeof(BaseRequest).IsAssignableFrom(requestType))
                throw new ColomboException(string.Format("{0} is no assignable to BaseRequest. You can only register BaseRequest types.", requestType));

            var request = (BaseRequest)Activator.CreateInstance(requestType);
            var responseType = request.GetResponseType();

            var messageName = requestType.Name.Replace("Request", string.Empty);

            if (request.IsSideEffectFree)
            {
                if (GetTypeMapping.ContainsKey(requestType.Name) || GetTypeMapping.ContainsKey(messageName))
                    throw new ColomboException(string.Format("Unable to register request {0} because a request with the same name (either {1} or {2}) is already registered.", requestType, messageName, requestType.Name));
                GetTypeMapping[requestType.Name] = requestType;
                GetTypeMapping[messageName] = requestType;
            }
            else
            {
                if (PostTypeMapping.ContainsKey(requestType.Name) || PostTypeMapping.ContainsKey(messageName))
                    throw new ColomboException(string.Format("Unable to register request {0} because a request with the same name (either {1} or {2}) is already registered.", requestType, messageName, requestType.Name));
                PostTypeMapping[requestType.Name] = requestType;
                PostTypeMapping[messageName] = requestType;
            }

            if (!KnownTypes.Contains(requestType))
                KnownTypes.Add(requestType);

            if (!KnownTypes.Contains(responseType))
                KnownTypes.Add(responseType);
        }

        /// <summary>
        /// Clear all the previous requests registrations.
        /// </summary>
        public static void ClearRegistrations()
        {
            GetTypeMapping.Clear();
            PostTypeMapping.Clear();
            KnownTypes.Clear();
        }

        /// <summary>
        /// Return the list of Known Types (Response) for the WCF infrastructure.
        /// </summary>
        public static IEnumerable<Type> GetKnownTypes(ICustomAttributeProvider provider)
        {
            return KnownTypes;
        }

        /// <summary>
        /// GET operation.
        /// </summary>
        [OperationContract]
        [WebGet(UriTemplate = "/{messageName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [ServiceKnownType("GetKnownTypes", typeof(ClientRestService))]
        public Response InvokeGet(string messageName)
        {
            if (!GetTypeMapping.ContainsKey(messageName))
            {
                if (PostTypeMapping.ContainsKey(messageName))
                    throw new ColomboException(string.Format("{0} is not a side-effect free request. You must invoke using POST, not GET.", PostTypeMapping[messageName]));

                throw new ColomboException(string.Format("No request with the name {0} or {0}Request has been registered. You must register your request type using ClientRestService.RegisterRequestType before using the bridge.", messageName));
            }

            var requestType = GetTypeMapping[messageName];
            var properties = OperationContext.Current.IncomingMessageProperties;
            var property = (HttpRequestMessageProperty)properties[HttpRequestMessageProperty.Name];

            var request = (BaseRequest)QueryStringMapper.Map(property.QueryString, requestType);

            return WcfServices.Process(request);
        }

        /// <summary>
        /// POST operation.
        /// </summary>
        [OperationContract]
        [WebInvoke(UriTemplate = "/{messageName}", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        [ServiceKnownType("GetKnownTypes", typeof(ClientRestService))]
        public Response InvokePost(string messageName)
        {
            if (!PostTypeMapping.ContainsKey(messageName))
            {
                if (GetTypeMapping.ContainsKey(messageName))
                    throw new ColomboException(string.Format("{0} is a side-effect free request. You must invoke using GET, not POST.", GetTypeMapping[messageName]));

                throw new ColomboException(string.Format("No request with the name {0} or {0}Request has been registered. You must register your request type using ClientRestService.RegisterRequestType before using the bridge.", messageName));
            }

            var requestType = PostTypeMapping[messageName];
            var properties = OperationContext.Current.IncomingMessageProperties;
            var property = (HttpRequestMessageProperty)properties[HttpRequestMessageProperty.Name];

            var request = (BaseRequest)QueryStringMapper.Map(property.QueryString, requestType);

            return WcfServices.Process(request);
        }
    }
}
