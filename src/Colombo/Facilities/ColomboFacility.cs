using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.Registration;
using System.Configuration;
using Colombo.Impl;
using Colombo.Wcf;
using Colombo.Configuration;
using Colombo.Interceptors;

namespace Colombo.Facilities
{
    public class ColomboFacility : AbstractFacility
    {
        bool registerLocalProcessing = true;

        IList<Type> sendInterceptors = new List<Type>()
        {
            typeof(CurrentCultureSendInterceptor),
            typeof(SLASendInterceptor),
            typeof(DataAnnotationSendInterceptor)
        };

        IList<Type> handlerInterceptors = new List<Type>()
        {
            typeof(CurrentCultureHandlerInterceptor),
            typeof(TransactionScopeHandlerInterceptor),
            typeof(DataAnnotationHandlerInterceptor),
            typeof(RequiredInContextHandlerInterceptor)
        };

        IList<Type> alerters = new List<Type>()
        {
            typeof(EventLogColomboAlerter)
        };

        protected override void Init()
        {
            Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel, true));

            Kernel.Register(
                Component.For<IColomboConfiguration>()
                    .LifeStyle.Singleton
                    .UsingFactoryMethod<IColomboConfiguration>(() =>
                    {
                        var config = (ColomboConfigurationSection)ConfigurationManager.GetSection(ColomboConfigurationSection.SectionName);
                        if (config == null)
                            return new EmptyColomboConfiguration();
                        return config;
                    }),
                Component.For<IMessageProcessor>()
                    .LifeStyle.Singleton
                    .ImplementedBy<WcfClientMessageProcessor>(),
                Component.For<IMessageBus>()
                    .LifeStyle.Singleton
                    .ImplementedBy<MessageBus>()
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
                        .LifeStyle.Singleton
                        .ImplementedBy<KernelRequestHandlerFactory>(),
                    Component.For<ILocalMessageProcessor, IMessageProcessor>()
                        .LifeStyle.Singleton
                        .ImplementedBy<LocalMessageProcessor>()
                );
                
                WcfService.RegisterKernel(Kernel);

                foreach (var handlerType in handlerInterceptors)
                {
                    Kernel.Register(
                        Component.For<IRequestHandlerInterceptor>()
                            .LifeStyle.Singleton
                            .ImplementedBy(handlerType)
                    );
                }
            }
        }

        public void ClientOnly()
        {
            registerLocalProcessing = false;
        }

        public void DoNotManageCurrentCulture()
        {
            sendInterceptors.Remove(typeof(CurrentCultureSendInterceptor));
            handlerInterceptors.Remove(typeof(CurrentCultureHandlerInterceptor));
        }

        public void DoNotHandleInsideTransactionScope()
        {
            handlerInterceptors.Remove(typeof(TransactionScopeHandlerInterceptor));
        }

        public void DoNotManageSLA()
        {
            sendInterceptors.Remove(typeof(SLASendInterceptor));
        }

        public void DoNotAlertInApplicationEventLog()
        {
            alerters.Remove(typeof(EventLogColomboAlerter));
        }

        public void DoNotValidateDataAnnotations()
        {
            sendInterceptors.Remove(typeof(DataAnnotationSendInterceptor));
            handlerInterceptors.Remove(typeof(DataAnnotationHandlerInterceptor));
        }

        public void DoNotEnforceRequiredInContext()
        {
            handlerInterceptors.Remove(typeof(RequiredInContextHandlerInterceptor));
        }

        public void MonitorWithPerformanceCounter()
        {
            handlerInterceptors.Add(typeof(PerfCounterHandlerInterceptor));
        }
    }
}
