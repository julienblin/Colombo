using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Castle.DynamicProxy;
using Colombo.Impl.Async;
using System.Reflection;

namespace Colombo.UnitTestSupport
{
    /// <summary>
    /// Base static classes for unit test support
    /// </summary>
    public static class ColomboTest
    {
        /// <summary>
        /// All assertions
        /// </summary>
        public static class AssertThat
        {
            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if any message (i.e. requests, notifications, responses)
            /// in the assembly that contains <typeparamref name="T"/> cannot be used by Colombo.
            /// </summary>
            public static void AllMessagesAreConformInAssemblyThatContains<T>()
            {
                AllMessagesAreConformInAssembly(typeof(T).Assembly);
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if any message (i.e. requests, notifications, responses)
            /// in the assembly <paramref name="assembly"/> cannot be used by Colombo.
            /// </summary>
            public static void AllMessagesAreConformInAssembly(Assembly assembly)
            {
                AllRequestsAreConformInAssembly(assembly);
                AllNotificationsAreConformInAssembly(assembly);
                AllResponsesAreConformInAssembly(assembly);
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if any request
            /// in the assembly that contains <typeparamref name="T"/> cannot be used by Colombo.
            /// </summary>
            public static void AllRequestsAreConformInAssemblyThatContains<T>()
            {
                AllRequestsAreConformInAssembly(typeof (T).Assembly);
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if any request
            /// in the assembly <paramref name="assembly"/> cannot be used by Colombo.
            /// </summary>
            public static void AllRequestsAreConformInAssembly(Assembly assembly)
            {
                var allRequestsTypes =
                    assembly.GetTypes().Where(
                        x => typeof (BaseRequest).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface);
                foreach (var requestType in allRequestsTypes)
                {
                    RequestIsConform(requestType);
                }
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if the <typeparam name="TRequest" /> cannot be used by Colombo.
            /// </summary>
            public static void RequestIsConform<TRequest>()
                where TRequest: BaseRequest
            {
                RequestIsConform(typeof(TRequest));
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if the <paramref name="requestType"/> cannot be used by Colombo.
            /// </summary>
            public static void RequestIsConform(Type requestType)
            {
                try
                {
                    Activator.CreateInstance(requestType);
                }
                catch (Exception ex)
                {
                    throw new ColomboTestException(string.Format("Request {0} cannot be instantiated. Probably because you forgot to include a default constructor.", requestType), ex);
                }

                using (var stream = new MemoryStream())
                {
                    try
                    {
                        var serializer = new DataContractSerializer(requestType);
                        var request = Activator.CreateInstance(requestType);
                        serializer.WriteObject(stream, request);
                        stream.Position = 0;
                        serializer.ReadObject(stream);
                    }
                    catch (Exception ex)
                    {
                        throw new ColomboTestException(string.Format("Request {0} should be serializable using the DataContractSerializer.", requestType), ex);
                    }
                }
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if any notification
            /// in the assembly that contains <typeparamref name="T"/> cannot be used by Colombo.
            /// </summary>
            public static void AllNotificationsAreConformInAssemblyThatContains<T>()
            {
                AllNotificationsAreConformInAssembly(typeof(T).Assembly);
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if any notification
            /// in the assembly <paramref name="assembly"/> cannot be used by Colombo.
            /// </summary>
            public static void AllNotificationsAreConformInAssembly(Assembly assembly)
            {
                var allNotificationsTypes =
                    assembly.GetTypes().Where(
                        x => typeof(Notification).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface);
                foreach (var notificationType in allNotificationsTypes)
                {
                    NotificationIsConform(notificationType);
                }
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if the <typeparam name="TNotification" /> cannot be used by Colombo.
            /// </summary>
            public static void NotificationIsConform<TNotification>()
                where TNotification : Notification
            {
                NotificationIsConform(typeof(TNotification));
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if the <paramref name="notificationType"/> cannot be used by Colombo.
            /// </summary>
            public static void NotificationIsConform(Type notificationType)
            {
                try
                {
                    Activator.CreateInstance(notificationType);
                }
                catch (Exception ex)
                {
                    throw new ColomboTestException(string.Format("Notification {0} cannot be instantiated. Probably because you forgot to include a default constructor.", notificationType), ex);
                }

                using (var stream = new MemoryStream())
                {
                    try
                    {
                        var serializer = new DataContractSerializer(notificationType);
                        var request = Activator.CreateInstance(notificationType);
                        serializer.WriteObject(stream, request);
                        stream.Position = 0;
                        serializer.ReadObject(stream);
                    }
                    catch (Exception ex)
                    {
                        throw new ColomboTestException(string.Format("Notification {0} should be serializable using the DataContractSerializer.", notificationType), ex);
                    }
                }
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if any response
            /// in the assembly that contains <typeparamref name="T"/> cannot be used by Colombo.
            /// </summary>
            public static void AllResponsesAreConformInAssemblyThatContains<T>()
            {
                AllResponsesAreConformInAssembly(typeof(T).Assembly);
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if any response
            /// in the assembly <paramref name="assembly"/> cannot be used by Colombo.
            /// </summary>
            public static void AllResponsesAreConformInAssembly(Assembly assembly)
            {
                var allResponsesTypes =
                    assembly.GetTypes().Where(
                        x => typeof(Response).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface);
                foreach (var responseType in allResponsesTypes)
                {
                    ResponseIsConform(responseType);
                }
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if the <typeparam name="TResponse" /> cannot be used by Colombo.
            /// </summary>
            public static void ResponseIsConform<TResponse>()
                where TResponse : Response
            {
                ResponseIsConform(typeof(TResponse));
            }

            /// <summary>
            /// Will raise a <see cref="ColomboTestException"/> if the <paramref name="responseType"/> cannot be used by Colombo.
            /// </summary>
            public static void ResponseIsConform(Type responseType)
            {
                try
                {
                    Activator.CreateInstance(responseType);
                }
                catch (Exception ex)
                {
                    throw new ColomboTestException(string.Format("Response {0} cannot be instantiated. Probably because you forgot to include a default constructor.", responseType), ex);
                }

                using (var stream = new MemoryStream())
                {
                    try
                    {
                        var serializer = new DataContractSerializer(responseType);
                        var request = Activator.CreateInstance(responseType);
                        serializer.WriteObject(stream, request);
                        stream.Position = 0;
                        serializer.ReadObject(stream);
                    }
                    catch (Exception ex)
                    {
                        throw new ColomboTestException(string.Format("Response {0} should be serializable using the DataContractSerializer.", responseType), ex);
                    }
                }

                try
                {
                    var options = new ProxyGenerationOptions(new NonVirtualCheckProxyGenerationHook());
                    var proxyGen = new ProxyGenerator();
                    proxyGen.CreateClassProxy(responseType, options);
                }
                catch (Exception ex)
                {
                    throw new ColomboTestException(string.Format("Response {0} cannot be proxied, probably because one or several of its members are not virtual.", responseType), ex);
                }
            }
        }
    }
}
