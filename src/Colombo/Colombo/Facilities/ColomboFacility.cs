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
    public class ColomboFacility : AbstractFacility
    {
        public const int DefaultMaxAllowedNumberOfSendForStatefulMessageBus = 10;
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

        public void ClientOnly()
        {
            registerLocalProcessing = false;
        }

        public void MaxAllowedNumberOfSendForStatefulMessageBus(int max)
        {
            maxAllowedNumberOfSendForStatefulMessageBus = max;
        }

        public void HealthCheckHeartBeatInSeconds(int seconds)
        {
            healthCheckHeartBeatInSeconds = seconds;
        }

        public void DoNotHandleInsideTransactionScope()
        {
            requestHandlerInterceptors.Remove(typeof(TransactionScopeRequestHandleInterceptor));
            notificationHandlerInterceptors.Remove(typeof(TransactionScopeNotificationHandleInterceptor));
        }

        public void DoNotManageCurrentCulture()
        {
            sendInterceptors.Remove(typeof(CurrentCultureSendInterceptor));
            requestHandlerInterceptors.Remove(typeof(CurrentCultureHandleInterceptor));
        }

        public void DoNotEnforceRequiredInContext()
        {
            requestHandlerInterceptors.Remove(typeof(RequiredInContextHandleInterceptor));
        }

        public void DoNotValidateDataAnnotations()
        {
            sendInterceptors.Remove(typeof(DataAnnotationsValidationSendInterceptor));
            requestHandlerInterceptors.Remove(typeof(DataAnnotationsValidationHandleInterceptor));
        }

        public void DoNotAlertInApplicationEventLog()
        {
            alerters.Remove(typeof(EventLogColomboAlerter));
        }

        public void DoNotManageSLA()
        {
            sendInterceptors.Remove(typeof(SLASendInterceptor));
        }

        public void MonitorWithPerformanceCounters()
        {
            sendInterceptors.Add(typeof(PerfCounterSendInterceptor));
            requestHandlerInterceptors.Add(typeof(PerfCounterHandleInterceptor));
        }

        public void EnableInMemoryCaching()
        {
            if (enableMemcachedCaching)
                throw new ColomboException("It seems that caching has been previously enabled with Memcached. Out of the box Colombo doesn't support multiple concurrent caching providers. Try implementing your own ICache.");

            enableInMemoryCaching = true;
            sendInterceptors.Add(typeof(ClientCacheSendInterceptor));
            requestHandlerInterceptors.Add(typeof(CacheHandleInterceptor));
        }

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
