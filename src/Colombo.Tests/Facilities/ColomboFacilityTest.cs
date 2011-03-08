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

using System.Linq;
using Castle.MicroKernel;
using Castle.MicroKernel.Lifestyle;
using Castle.Windsor;
using Colombo.Caching;
using Colombo.Caching.Impl;
using Colombo.Facilities;
using Colombo.Impl;
using Colombo.Impl.Alerters;
using Colombo.Interceptors;
using Colombo.Messages;
using Colombo.TestSupport;
using Colombo.Wcf;
using NUnit.Framework;

namespace Colombo.Tests.Facilities
{
    [TestFixture]
    public class ColomboFacilityTest
    {
        [Test]
        public void It_should_register_necessaries_components()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.Resolve<IMessageBus>(), Is.Not.Null);
            Assert.That(container.Resolve<IRequestProcessor>(), Is.Not.Null);
            Assert.That(container.Resolve<ILocalRequestProcessor>(), Is.Not.Null);
            Assert.That(container.Resolve<IRequestHandlerFactory>(), Is.Not.Null);
            Assert.That(container.Resolve<IColomboStatCollector>(), Is.Not.Null);

            Assert.That(container.Resolve<IStatefulMessageBus>(), Is.Not.Null);
            Assert.That(container.Resolve<IStatefulMessageBus>(), Is.Not.SameAs(container.Resolve<IStatefulMessageBus>()));
        }

        [Test]
        public void It_should_position_the_kernel_in_the_wcf_static_field()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(WcfServices.Kernel, Is.SameAs(container.Kernel));
        }

        [Test]
        public void It_should_register_necessaries_components_with_ClientOnly_option()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.ClientOnly());

            Assert.That(container.Resolve<IMessageBus>(), Is.Not.Null);

            Assert.That(container.Resolve<IStatefulMessageBus>(), Is.Not.Null);
            Assert.That(container.Resolve<IStatefulMessageBus>(), Is.Not.SameAs(container.Resolve<IStatefulMessageBus>()));

            var messageProcessors = container.ResolveAll<IRequestProcessor>();
            Assert.That(messageProcessors.Length, Is.AtLeast(1));
            foreach (var processor in messageProcessors)
            {
                Assert.That(processor,
                Is.Not.AssignableFrom<ILocalRequestProcessor>());
            }

            Assert.That(() => container.Resolve<ILocalRequestProcessor>(),
                Throws.Exception.TypeOf<ComponentNotFoundException>());
            Assert.That(() => container.Resolve<IRequestHandlerFactory>(),
                Throws.Exception.TypeOf<ComponentNotFoundException>());
        }

        [Test]
        public void It_should_not_register_WcfClientRequestProcessor_and_ColomboServiceFactory_when_DisableSendingThroughWcf()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IRequestProcessor>().Any(x => x is WcfClientRequestProcessor));
            Assert.That(container.Resolve<IColomboServiceFactory>(), Is.TypeOf<ColomboServiceFactory>());

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.DisableSendingThroughWcf());

            Assert.That(!container.ResolveAll<IRequestProcessor>().Any(x => x is WcfClientRequestProcessor));
            Assert.That(!container.Kernel.HasComponent(typeof(IColomboServiceFactory)));
        }

        [Test]
        public void It_should_position_value_for_MaxAllowedNumberOfSendForStatefulMessageBus()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.Resolve<IStatefulMessageBus>().MaxAllowedNumberOfSend,
                Is.EqualTo(ColomboFacility.DefaultMaxAllowedNumberOfSendForStatefulMessageBus));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.MaxAllowedNumberOfSendForStatefulMessageBus(50));

            Assert.That(container.Resolve<IStatefulMessageBus>().MaxAllowedNumberOfSend, Is.EqualTo(50));
        }

        [Test]
        public void It_should_position_value_for_HealthCheckHeartBeatInSeconds()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            var wcfClientMessageProcessor = (WcfClientRequestProcessor)container.ResolveAll<IRequestProcessor>().Where(x => x is WcfClientRequestProcessor).FirstOrDefault();
            Assert.That(wcfClientMessageProcessor, Is.Not.Null);

            Assert.That(wcfClientMessageProcessor.HealthCheckHeartBeatInSeconds,
                Is.EqualTo(ColomboFacility.DefaultHealthCheckHeartBeatInSeconds));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.HealthCheckHeartBeatInSeconds(50));

            wcfClientMessageProcessor = (WcfClientRequestProcessor)container.ResolveAll<IRequestProcessor>().Where(x => x is WcfClientRequestProcessor).FirstOrDefault();
            Assert.That(wcfClientMessageProcessor, Is.Not.Null);
            Assert.That(wcfClientMessageProcessor.HealthCheckHeartBeatInSeconds, Is.EqualTo(50));
        }

        [Test]
        public void It_should_register_HealthCheckRequestHandler()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.Resolve<ISideEffectFreeRequestHandler<HealthCheckRequest, HealthCheckResponse>>(),
                Is.TypeOf<HealthCheckRequestHandler>());

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.ClientOnly());

            Assert.That(() => container.Resolve<ISideEffectFreeRequestHandler<HealthCheckRequest, HealthCheckResponse>>(),
                Throws.Exception.TypeOf<ComponentNotFoundException>());
        }

        [Test]
        public void It_should_register_GetStatsRequestHandler()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.Resolve<ISideEffectFreeRequestHandler<GetStatsRequest, GetStatsResponse>>(),
                Is.TypeOf<GetStatsRequestHandler>());

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.ClientOnly());

            Assert.That(() => container.Resolve<ISideEffectFreeRequestHandler<GetStatsRequest, GetStatsResponse>>(),
                Throws.Exception.TypeOf<ComponentNotFoundException>());
        }

        [Test]
        public void It_should_register_TransactionScopeHandleInterceptors()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is TransactionScopeRequestHandleInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.DoNotHandleInsideTransactionScope());
            Assert.That(!container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is TransactionScopeRequestHandleInterceptor));
        }

        [Test]
        public void It_should_register_CurrentCulture_interceptors()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is CurrentCultureSendInterceptor));
            Assert.That(container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is CurrentCultureHandleInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.ClientOnly());
            Assert.That(container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is CurrentCultureSendInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.DoNotManageCurrentCulture());
            Assert.That(!container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is CurrentCultureSendInterceptor));
            Assert.That(!container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is CurrentCultureHandleInterceptor));
        }

        [Test]
        public void It_should_register_RequiredInContextHandlerInterceptor()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is RequiredInContextHandleInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.DoNotEnforceRequiredInContext());
            Assert.That(!container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is RequiredInContextHandleInterceptor));
        }

        [Test]
        public void It_should_register_DataAnnotationInterceptors()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is DataAnnotationsValidationSendInterceptor));
            Assert.That(container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is DataAnnotationsValidationHandleInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.DoNotValidateDataAnnotations());
            Assert.That(!container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is DataAnnotationsValidationSendInterceptor));
            Assert.That(!container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is DataAnnotationsValidationHandleInterceptor));
        }

        [Test]
        public void It_should_register_EventLogColomboAlerter()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IColomboAlerter>().Any(x => x is EventLogColomboAlerter));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.DoNotAlertInApplicationEventLog());
            Assert.That(!container.ResolveAll<IColomboAlerter>().Any(x => x is EventLogColomboAlerter));
        }

        [Test]
        public void It_should_register_SLASendInterceptor()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is SLASendInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.DoNotManageSLA());
            Assert.That(!container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is SLASendInterceptor));
        }

        [Test]
        public void It_should_register_ExceptionsInterceptor()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is ExceptionsSendInterceptor));
            Assert.That(container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is ExceptionsHandleInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.DoNotAlertOnExceptions());
            Assert.That(!container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is ExceptionsSendInterceptor));
            Assert.That(!container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is ExceptionsHandleInterceptor));
        }

        [Test]
        public void It_should_register_PerfCounterHandlerInterceptor()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(!container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is PerfCounterHandleInterceptor));
            Assert.That(!container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is PerfCounterSendInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.MonitorWithPerformanceCounters());
            Assert.That(container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is PerfCounterHandleInterceptor));
            Assert.That(container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is PerfCounterSendInterceptor));
        }

        [Test]
        public void It_should_register_CachingComponents_MemCached()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(!container.ResolveAll<IColomboCache>().Any(x => x is MemcachedCache));
            Assert.That(!container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is CacheSendInterceptor));
            Assert.That(!container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is CacheHandleInterceptor));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.EnableMemcachedCaching(new string[] { "localhost" }));

            Assert.That(container.ResolveAll<IColomboCache>().Any(x => x is MemcachedCache));
            Assert.That(container.ResolveAll<IMessageBusSendInterceptor>().Any(x => x is CacheSendInterceptor));
            Assert.That(container.ResolveAll<IRequestHandlerHandleInterceptor>().Any(x => x is CacheHandleInterceptor));
        }

        //[Test]
        //public void It_should_not_allow_multiple_ICache()
        //{
        //    var container = new WindsorContainer();
        //    Assert.That(() => container.AddFacility<ColomboFacility>(f =>
        //        {
        //            f.EnableInMemoryCaching();
        //            f.EnableMemcachedCaching(new string[] { "localhost" });
        //        }),
        //        Throws.Exception.TypeOf<ColomboException>()
        //        .With.Message.Contains("ICache"));
        //}

        [Test]
        public void It_should_register_components_in_TestSupportMode()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();
            Assert.That(container.Resolve<IMessageBus>(), Is.TypeOf<MessageBus>());
            Assert.That(() => container.Resolve<IStubMessageBus>(), Throws.Exception.TypeOf<ComponentNotFoundException>());

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.TestSupportMode());

            Assert.That(container.Resolve<IMessageBus>(), Is.TypeOf<StubMessageBus>());
            Assert.That(container.Resolve<IStubMessageBus>(), Is.TypeOf<StubMessageBus>());
        }

        [Test]
        public void It_should_position_DoNotManageMetaContextKeys_accordingly()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            var messageBus = (IMetaContextKeysManager)container.Resolve<IMessageBus>();
            Assert.That(messageBus.DoNotManageMetaContextKeys, Is.False);

            var localRequestProcessor = (IMetaContextKeysManager)container.Resolve<ILocalRequestProcessor>();
            Assert.That(localRequestProcessor.DoNotManageMetaContextKeys, Is.False);

            Assert.That(WcfServices.DoNotManageMetaContextKeys, Is.False);

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.DoNotManageMetaContextKeys());

            messageBus = (IMetaContextKeysManager)container.Resolve<IMessageBus>();
            Assert.That(messageBus.DoNotManageMetaContextKeys, Is.True);

            localRequestProcessor = (IMetaContextKeysManager)container.Resolve<ILocalRequestProcessor>();
            Assert.That(localRequestProcessor.DoNotManageMetaContextKeys, Is.True);

            Assert.That(WcfServices.DoNotManageMetaContextKeys, Is.True);
        }

        [Test]
        public void It_should_allow_the_customization_of_lifestyle_for_IStatefulMessageBus()
        {
            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            Assert.That(container.Resolve<IStatefulMessageBus>(), Is.Not.Null);
            Assert.That(container.Resolve<IStatefulMessageBus>(), Is.Not.SameAs(container.Resolve<IStatefulMessageBus>()));

            container = new WindsorContainer();
            container.AddFacility<ColomboFacility>(f => f.StatefulMessageBusLifestyle(typeof (SingletonLifestyleManager)));

            Assert.That(container.Resolve<IStatefulMessageBus>(), Is.Not.Null);
            Assert.That(container.Resolve<IStatefulMessageBus>(), Is.SameAs(container.Resolve<IStatefulMessageBus>()));
        }
    }
}
