using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Colombo.Caching;
using Colombo.Caching.Impl;
using Colombo.HealthCheck;
using Colombo.Impl;
using Colombo.Impl.Alerters;
using Colombo.Impl.NotificationHandle;
using Colombo.Impl.RequestHandle;
using Colombo.Interceptors;
using Colombo.Wcf;

namespace Colombo.Facilities
{
    /// <summary>
    /// <see cref="Castle.MicroKernel.IFacility"/> that simplifies the registration of Colombo.
    /// </summary>
    /// <example>
    /// <code>
    /// var container = new WindsorContainer();
    /// container.AddFacility&lt;ColomboFacility&gt;(f => f.ClientOnly());
    /// </code>
    /// </example>
    public class ColomboFacility : AbstractFacility
    {
        /// <summary>
        /// Default value for the number of send allowed in <see cref="IStatefulMessageBus"/>.
        /// <see cref="IStatefulMessageBus.MaxAllowedNumberOfSend"/>
        /// </summary>
        public const int DefaultMaxAllowedNumberOfSendForStatefulMessageBus = 10;

        /// <summary>
        /// Default interval in seconds between health check for endpoints.
        /// </summary>
        public const int DefaultHealthCheckHeartBeatInSeconds = 30;

        bool registerLocalProcessing = true;
        int maxAllowedNumberOfSendForStatefulMessageBus = DefaultMaxAllowedNumberOfSendForStatefulMessageBus;
        int healthCheckHeartBeatInSeconds = DefaultHealthCheckHeartBeatInSeconds;

        bool enableInMemoryCaching;
        bool enableMemcachedCaching;
        string[] memCachedServers;

        readonly IList<Type> sendInterceptors = new List<Type>
                                                    {
            typeof(CurrentCultureSendInterceptor),
            typeof(DataAnnotationsValidationSendInterceptor),
            typeof(SLASendInterceptor)
        };

        readonly IList<Type> requestHandlerInterceptors = new List<Type>
                                                              {
            typeof(TransactionScopeRequestHandleInterceptor),
            typeof(CurrentCultureHandleInterceptor),
            typeof(RequiredInContextHandleInterceptor),
            typeof(DataAnnotationsValidationHandleInterceptor)
        };

        readonly IList<Type> notificationHandlerInterceptors = new List<Type>
                                                                   {
            typeof(TransactionScopeNotificationHandleInterceptor)
        };

        readonly IList<Type> alerters = new List<Type>
                                            {
            typeof(EventLogColomboAlerter)
        };

        /// <summary>
        /// The custom initialization for the Facility.
        /// </summary>
        protected override void Init()
        {
            Contract.Assume(Kernel != null);
            Contract.Assume(Kernel.Resolver != null);

            Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel, true));

            Kernel.Register(
                Component.For<IWcfColomboServiceFactory>()
                    .ImplementedBy<WcfColomboServiceFactory>(),
                Component.For<IRequestProcessor>()
                    .ImplementedBy<WcfClientRequestProcessor>()
                    .OnCreate((kernel, item) => ((WcfClientRequestProcessor)item).HealthCheckHeartBeatInSeconds = healthCheckHeartBeatInSeconds),

                Component.For<INotificationHandlerFactory>()
                    .ImplementedBy<KernelNotificationHandlerFactory>(),
                Component.For<INotificationProcessor>()
                    .ImplementedBy<LocalNotificationProcessor>(),

                Component.For<IMessageBus>()
                    .ImplementedBy<MessageBus>(),
                Component.For<IStatefulMessageBus>()
                    .LifeStyle.Transient
                    .ImplementedBy<StatefulMessageBus>()
                    .OnCreate((kernel, item) => item.MaxAllowedNumberOfSend = maxAllowedNumberOfSendForStatefulMessageBus)
            );

            foreach (var sendType in sendInterceptors)
            {
                Kernel.Register(
                    Component.For<IMessageBusSendInterceptor>()
                        .LifeStyle.Singleton
                        .ImplementedBy(sendType)
                );
            }

            foreach (var alerterType in alerters)
            {
                Kernel.Register(
                    Component.For<IColomboAlerter>()
                        .LifeStyle.Singleton
                        .ImplementedBy(alerterType)
                );
            }

            if (registerLocalProcessing)
            {
                Kernel.Register(
                    Component.For<IRequestHandlerFactory>()
                        .ImplementedBy<KernelRequestHandlerFactory>(),
                    Component.For<ILocalRequestProcessor, IRequestProcessor>()
                        .ImplementedBy<LocalRequestProcessor>(),
                    Component.For<ISideEffectFreeRequestHandler<HealthCheckRequest, ACKResponse>>()
                        .LifeStyle.Transient
                        .ImplementedBy<HealthCheckRequestHandler>()
                );

                WcfServices.Kernel = Kernel;

                foreach (var handlerType in requestHandlerInterceptors)
                {
                    Kernel.Register(
                        Component.For<IRequestHandlerHandleInterceptor>()
                            .LifeStyle.Singleton
                            .ImplementedBy(handlerType)
                    );
                }

                foreach (var handlerType in notificationHandlerInterceptors)
                {
                    Kernel.Register(
                        Component.For<INotificationHandleInterceptor>()
                            .LifeStyle.Singleton
                            .ImplementedBy(handlerType)
                    );
                }
            }

            if (enableInMemoryCaching)
            {
                Kernel.Register(
                    Component.For<IColomboCache>().ImplementedBy<InMemoryCache>()
                );
            }

            if (enableMemcachedCaching)
            {
                Kernel.Register(
                    Component.For<IColomboCache>()
                    .ImplementedBy<MemcachedCache>()
                    .UsingFactoryMethod(() => new MemcachedCache(memCachedServers))
                );
            }
        }

        /// <summary>
        /// Only register the components needed to act as a client, which means no message handling can happen locally.
        /// </summary>
        public void ClientOnly()
        {
            registerLocalProcessing = false;
        }

        /// <summary>
        /// Number of Send operations allowed in <see cref="IStatefulMessageBus"/>.
        /// Set to 0 or a negative number to disable the check.
        /// </summary>
        public void MaxAllowedNumberOfSendForStatefulMessageBus(int max)
        {
            maxAllowedNumberOfSendForStatefulMessageBus = max;
        }

        /// <summary>
        /// Interval in seconds between health check for endpoints.
        /// Set to 0 or a negative number to disable the check.
        /// </summary>
        public void HealthCheckHeartBeatInSeconds(int seconds)
        {
            healthCheckHeartBeatInSeconds = seconds;
        }

        /// <summary>
        /// Disabled the creation and management of a transcation per message handling.
        /// </summary>
        public void DoNotHandleInsideTransactionScope()
        {
            requestHandlerInterceptors.Remove(typeof(TransactionScopeRequestHandleInterceptor));
            notificationHandlerInterceptors.Remove(typeof(TransactionScopeNotificationHandleInterceptor));
        }

        /// <summary>
        /// Disable the transmission of the CurrentCulture between a client and a server through the "Culture" context key,
        /// and the automatic positionning of the CurrentCulture on the server-side when handling a request.
        /// </summary>
        public void DoNotManageCurrentCulture()
        {
            sendInterceptors.Remove(typeof(CurrentCultureSendInterceptor));
            requestHandlerInterceptors.Remove(typeof(CurrentCultureHandleInterceptor));
        }

        /// <summary>
        /// Disable checks for <see cref="RequiredInContextAttribute"/>.
        /// </summary>
        public void DoNotEnforceRequiredInContext()
        {
            requestHandlerInterceptors.Remove(typeof(RequiredInContextHandleInterceptor));
        }

        /// <summary>
        /// Disable automatic validation for <see cref="System.ComponentModel.DataAnnotations"/>.
        /// </summary>
        public void DoNotValidateDataAnnotations()
        {
            sendInterceptors.Remove(typeof(DataAnnotationsValidationSendInterceptor));
            requestHandlerInterceptors.Remove(typeof(DataAnnotationsValidationHandleInterceptor));
        }

        /// <summary>
        /// Disable the default <see cref="IColomboAlerter"/> that writes alerts as warning in the Application event log.
        /// </summary>
        public void DoNotAlertInApplicationEventLog()
        {
            alerters.Remove(typeof(EventLogColomboAlerter));
        }

        /// <summary>
        /// Disable checks for <see cref="SLAAttribute"/>.
        /// </summary>
        public void DoNotManageSLA()
        {
            sendInterceptors.Remove(typeof(SLASendInterceptor));
        }

        /// <summary>
        /// Enable monitoring of messages sent / handled through performance counters.
        /// Performance counters will be created on-the-fly if allowed, or could be created using <see cref="Colombo.Interceptors.PerfCounterFactory.CreatePerfCounters"/>.
        /// </summary>
        public void MonitorWithPerformanceCounters()
        {
            sendInterceptors.Add(typeof(PerfCounterSendInterceptor));
            requestHandlerInterceptors.Add(typeof(PerfCounterHandleInterceptor));
        }

        /// <summary>
        /// Enable caching of the requests marked with <see cref="EnableClientCachingAttribute"/> in-memory.
        /// Incompatible with <see cref="EnableMemcachedCaching"/>.
        /// </summary>
        public void EnableInMemoryCaching()
        {
            if (enableMemcachedCaching)
                throw new ColomboException("It seems that caching has been previously enabled with Memcached. Out of the box Colombo doesn't support multiple concurrent caching providers. Try implementing your own ICache.");

            enableInMemoryCaching = true;
            sendInterceptors.Add(typeof(ClientCacheSendInterceptor));
            requestHandlerInterceptors.Add(typeof(CacheHandleInterceptor));
        }

        /// <summary>
        /// Enable caching of the requests marked with <see cref="EnableClientCachingAttribute"/> using memcached servers.
        /// Incompatible with <see cref="EnableInMemoryCaching"/>.
        /// </summary>
        /// <param name="servers">List of memcached servers addresses.</param>
        /// <example>
        /// <code>
        ///     EnableMemcachedCaching(new[] { "localhost:11211", "cacheserver:1234" });
        /// </code>
        /// </example>
        public void EnableMemcachedCaching(string[] servers)
        {
            if (enableInMemoryCaching)
                throw new ColomboException("It seems that caching has been previously enabled with InMemory. Out of the box Colombo doesn't support multiple concurrent caching providers. Try implementing your own ICache.");

            enableMemcachedCaching = true;
            memCachedServers = servers;
            sendInterceptors.Add(typeof(ClientCacheSendInterceptor));
            requestHandlerInterceptors.Add(typeof(CacheHandleInterceptor));
        }
    }
}
