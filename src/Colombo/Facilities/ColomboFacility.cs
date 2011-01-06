using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Facilities;
using Castle.MicroKernel.Resolvers.SpecializedResolvers;
using Castle.MicroKernel.Registration;
using Colombo.Wcf;
using Colombo.Impl;

namespace Colombo.Facilities
{
    public class ColomboFacility : AbstractFacility
    {
        bool registerLocalProcessing = true;
        bool allowMultipleFutureSendBatches = false;

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
                    .OnCreate((kernel, item) => item.AllowMultipleFutureSendBatches = allowMultipleFutureSendBatches)
            );

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
            }
        }

        public void ClientOnly()
        {
            registerLocalProcessing = false;
        }

        public void AllowMultipleFutureSendBatches()
        {
            allowMultipleFutureSendBatches = true;
        }
    }
}
