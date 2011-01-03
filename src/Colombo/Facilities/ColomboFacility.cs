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
        bool addCultureInfoInterceptor = true;

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

            if (addCultureInfoInterceptor)
            {
                Kernel.Register(
                    Component.For<IMessageBusSendInterceptor>()
                        .LifeStyle.Singleton
                        .ImplementedBy<CurrentCultureSendInterceptor>()
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

                if (addCultureInfoInterceptor)
                {
                    Kernel.Register(
                        Component.For<IRequestHandlerInterceptor>()
                            .LifeStyle.Singleton
                            .ImplementedBy<CurrentCultureHandlerInterceptor>()
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
            addCultureInfoInterceptor = false;
        }
    }
}
