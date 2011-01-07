using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.Registration;
using Colombo.Wcf;
using Colombo.Impl;
using Colombo.Interceptors;

namespace Colombo.Facilities
{
    public class ColomboFacility : AbstractFacility
    {
        public const int DefaultMaxAllowedNumberOfSendForStatefulMessageBus = 10;

        bool registerLocalProcessing = true;
        int maxAllowedNumberOfSendForStatefulMessageBus = DefaultMaxAllowedNumberOfSendForStatefulMessageBus;

        IList<Type> sendInterceptors = new List<Type>()
        {
            typeof(CurrentCultureSendInterceptor)
        };

        IList<Type> handlerInterceptors = new List<Type>()
        {
            typeof(TransactionScopeHandleInterceptor),
            typeof(CurrentCultureHandleInterceptor)
        };

        protected override void Init()
        {
            Kernel.Resolver.AddSubResolver(new ArrayResolver(Kernel, true));

            Kernel.Register(
                Component.For<IWcfClientBaseServiceFactory>()
                    .ImplementedBy<WcfConfigClientBaseServiceFactory>(),
                Component.For<IRequestProcessor>()
                    .ImplementedBy<WcfClientRequestProcessor>(),
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

            if (registerLocalProcessing)
            {
                Kernel.Register(
                    Component.For<IRequestHandlerFactory>()
                        .LifeStyle.Singleton
                        .ImplementedBy<KernelRequestHandlerFactory>(),
                    Component.For<ILocalRequestProcessor, IRequestProcessor>()
                        .LifeStyle.Singleton
                        .ImplementedBy<LocalRequestProcessor>()
                );

                WcfService.RegisterKernel(Kernel);

                foreach (var handlerType in handlerInterceptors)
                {
                    Kernel.Register(
                        Component.For<IRequestHandlerHandleInterceptor>()
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

        public void MaxAllowedNumberOfSendForStatefulMessageBus(int max)
        {
            maxAllowedNumberOfSendForStatefulMessageBus = max;
        }

        public void DoNotHandleInsideTransactionScope()
        {
            handlerInterceptors.Remove(typeof(TransactionScopeHandleInterceptor));
        }

        public void DoNotManageCurrentCulture()
        {
            sendInterceptors.Remove(typeof(CurrentCultureSendInterceptor));
            handlerInterceptors.Remove(typeof(CurrentCultureHandleInterceptor));
        }
    }
}
