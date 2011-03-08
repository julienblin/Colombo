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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Facilities.Startable;
using Castle.MicroKernel;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Lifestyle;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Colombo.Caching;
using Colombo.Caching.Impl;
using Colombo.Impl;
using Colombo.Impl.Alerters;
using Colombo.Impl.RequestHandle;
using Colombo.Interceptors;
using Colombo.Messages;
using Colombo.TestSupport;
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
        bool disableSendingThroughWcf;
        int maxAllowedNumberOfSendForStatefulMessageBus = DefaultMaxAllowedNumberOfSendForStatefulMessageBus;
        int healthCheckHeartBeatInSeconds = DefaultHealthCheckHeartBeatInSeconds;
        Type statefulMessageBusLifestyleManager = typeof(TransientLifestyleManager);

        bool enableMemcachedCaching;
        string[] memCachedServers;
        bool testSupportMode;
        bool doNotManageMetaContextKeys;

        readonly IList<Type> sendInterceptors = new List<Type>
                                                    {
            typeof(CurrentCultureSendInterceptor),
            typeof(DataAnnotationsValidationSendInterceptor),
            typeof(SLASendInterceptor),
            typeof(ExceptionsSendInterceptor)
        };

        readonly IList<Type> requestHandlerInterceptors = new List<Type>
                                                              {
            typeof(TransactionScopeRequestHandleInterceptor),
            typeof(CurrentCultureHandleInterceptor),
            typeof(RequiredInContextHandleInterceptor),
            typeof(DataAnnotationsValidationHandleInterceptor),
            typeof(ExceptionsHandleInterceptor)
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
            
            if (!Kernel.GetFacilities().Any(x => x is StartableFacility))
                Kernel.AddFacility<StartableFacility>();

            if (testSupportMode)
            {
                Kernel.Register(
                    Component.For<IStubMessageBus, IMessageBus>()
                        .ImplementedBy<StubMessageBus>(),
                    Component.For<IStatefulMessageBus>()
                        .LifeStyle.Transient
                        .ImplementedBy<StatefulMessageBus>()
                        .OnCreate((kernel, item) => item.MaxAllowedNumberOfSend = maxAllowedNumberOfSendForStatefulMessageBus)
                );
            }
            else
            {
                Kernel.Register(
                    Component.For<IMessageBus, IMetaContextKeysManager>()
                        .ImplementedBy<MessageBus>()
                        .Unless((k, m) => k.HasComponent(typeof(IMessageBus))),
                    Component.For<IStatefulMessageBus>()
                        .LifeStyle.Custom(statefulMessageBusLifestyleManager)
                        .ImplementedBy<StatefulMessageBus>()
                        .Unless((k, m) => k.HasComponent(typeof(IStatefulMessageBus)))
                        .OnCreate((kernel, item) => item.MaxAllowedNumberOfSend = maxAllowedNumberOfSendForStatefulMessageBus),

                    Component.For<IColomboStatCollector>()
                        .ImplementedBy<InMemoryStatCollector>()
                );

                if (!disableSendingThroughWcf)
                {
                    Kernel.Register(
                        Component.For<IColomboServiceFactory>()
                            .ImplementedBy<ColomboServiceFactory>()
                            .Unless((k, m) => k.HasComponent(typeof(IColomboServiceFactory))),
                        Component.For<IRequestProcessor>()
                            .ImplementedBy<WcfClientRequestProcessor>()
                            .OnCreate((kernel, item) => ((WcfClientRequestProcessor)item).HealthCheckHeartBeatInSeconds = healthCheckHeartBeatInSeconds)
                    );
                }
            }

            WcfServices.Kernel = Kernel;

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
                if (!testSupportMode)
                {
                    Kernel.Register(
                        Component.For<IRequestHandlerFactory>()
                            .ImplementedBy<KernelRequestHandlerFactory>()
                            .Unless((k, m) => k.HasComponent(typeof(IRequestHandlerFactory))),
                        Component.For<ILocalRequestProcessor, IRequestProcessor, IMetaContextKeysManager>()
                            .ImplementedBy<LocalRequestProcessor>()
                            .Unless((k, m) => k.HasComponent(typeof(ILocalRequestProcessor))),
                        Component.For<ISideEffectFreeRequestHandler<HealthCheckRequest, HealthCheckResponse>>()
                            .LifeStyle.Transient
                            .ImplementedBy<HealthCheckRequestHandler>()
                            .Unless((k, m) => k.HasComponent(typeof(ISideEffectFreeRequestHandler<HealthCheckRequest, HealthCheckResponse>))),
                        Component.For<ISideEffectFreeRequestHandler<GetStatsRequest, GetStatsResponse>>()
                            .LifeStyle.Transient
                            .ImplementedBy<GetStatsRequestHandler>()
                            .Unless((k, m) => k.HasComponent(typeof(ISideEffectFreeRequestHandler<GetStatsRequest, GetStatsResponse>)))
                        );

                    if (enableMemcachedCaching)
                    {
                        Kernel.Register(
                            Component.For<IColomboCache>()
                            .ImplementedBy<MemcachedCache>()
                            .UsingFactoryMethod(() => new MemcachedCache(memCachedServers))
                        );
                    }
                }

                foreach (var handlerType in requestHandlerInterceptors)
                {
                    Kernel.Register(
                        Component.For<IRequestHandlerHandleInterceptor>()
                            .LifeStyle.Singleton
                            .ImplementedBy(handlerType)
                    );
                }
            }

            foreach (var metaContextKeysManager in Kernel.ResolveAll<IMetaContextKeysManager>())
                metaContextKeysManager.DoNotManageMetaContextKeys = doNotManageMetaContextKeys;

            WcfServices.DoNotManageMetaContextKeys = doNotManageMetaContextKeys;
        }

        /// <summary>
        /// Only register the components needed to act as a client, which means no message handling can happen locally.
        /// </summary>
        public void ClientOnly()
        {
            registerLocalProcessing = false;
        }

        /// <summary>
        /// Disable the send operations using WCF.
        /// </summary>
        public void DisableSendingThroughWcf()
        {
            disableSendingThroughWcf = true;
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
        /// Disable alerts when a operation throws an exception.
        /// </summary>
        public void DoNotAlertOnExceptions()
        {
            sendInterceptors.Remove(typeof(ExceptionsSendInterceptor));
            requestHandlerInterceptors.Remove(typeof(ExceptionsHandleInterceptor));
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
        /// Enable caching of the requests marked with <see cref="EnableCacheAttribute"/> using memcached servers.
        /// </summary>
        /// <param name="servers">List of memcached servers addresses.</param>
        /// <example>
        /// <code>
        ///     EnableMemcachedCaching(new[] { "localhost:11211", "cacheserver:1234" });
        /// </code>
        /// </example>
        public void EnableMemcachedCaching(string[] servers)
        {
            enableMemcachedCaching = true;
            memCachedServers = servers;
            sendInterceptors.Add(typeof(CacheSendInterceptor));
            requestHandlerInterceptors.Add(typeof(CacheHandleInterceptor));
        }

        /// <summary>
        /// Registers Colombo components in test mode. It means that no real send operations will be performed,
        /// But using the <see cref="IStubMessageBus"/> you will be able to assert certain operations.
        /// </summary>
        public void TestSupportMode()
        {
            testSupportMode = true;
        }

        /// <summary>
        /// Disable the management of MetaContextKeys. No value will be set in the Context if disabled.
        /// </summary>
        public void DoNotManageMetaContextKeys()
        {
            doNotManageMetaContextKeys = true;
        }

        /// <summary>
        /// Allow the customization of the <see cref="ILifestyleManager"/> associated with the <see cref="IStatefulMessageBus"/>.
        /// Default value is Transient.
        /// </summary>
        /// <param name="lifestyleManagerType">The type of <see cref="ILifestyleManager"/> to associate with the <see cref="IStatefulMessageBus"/>.</param>
        public void StatefulMessageBusLifestyle(Type lifestyleManagerType)
        {
            statefulMessageBusLifestyleManager = lifestyleManagerType;
        }
    }
}
