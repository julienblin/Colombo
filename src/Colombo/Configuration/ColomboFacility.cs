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

namespace Colombo.Configuration
{
    public class ColomboFacility : AbstractFacility
    {
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
                Component.For<IRequestHandlerFactory>()
                    .LifeStyle.Singleton
                    .ImplementedBy<KernelRequestHandlerFactory>(),
                Component.For<ILocalMessageProcessor, IMessageProcessor>()
                    .LifeStyle.Singleton
                    .ImplementedBy<LocalMessageProcessor>(),
                Component.For<IMessageProcessor>()
                    .LifeStyle.Singleton
                    .ImplementedBy<WcfClientMessageProcessor>(),
                Component.For<IMessageBus>()
                    .LifeStyle.Singleton
                    .ImplementedBy<MessageBus>()
            );

            WcfService.RegisterKernel(Kernel);
        }
    }
}
