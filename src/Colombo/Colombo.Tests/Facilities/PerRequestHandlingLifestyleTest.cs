﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Castle.Windsor;
using Colombo.Facilities;
using Castle.Facilities.Logging;
using Castle.MicroKernel.Registration;
using System.Threading;
using System.Threading.Tasks;

namespace Colombo.Tests.Facilities
{
    [TestFixture]
    public class PerRequestHandlingLifestyleTest : BaseTest
    {
        [Test]
        public void It_should_return_instance_without_Task_CurrentId()
        {
            var container = new WindsorContainer();
            container.Register(
                Component.For<InstanceTracker>()
                    .LifeStyle.Singleton
                    .ImplementedBy<InstanceTracker>(),
                Component.For<ISubDependency>()
                    .LifeStyle.PerRequestHandling()
                    .ImplementedBy<SubDependency>(),
                Component.For<IDependency>()
                    .LifeStyle.PerRequestHandling()
                    .ImplementedBy<Dependency>()
            );
             
            var subDependency1 = container.Resolve<ISubDependency>();
            var subDependency2 = container.Resolve<ISubDependency>();
            var dependency1 = container.Resolve<IDependency>();
            var dependency2 = container.Resolve<IDependency>();

            Assert.That(() => subDependency1,
                Is.Not.Null);

            Assert.That(() => subDependency2,
                Is.EqualTo(subDependency1));

            Assert.That(() => dependency1.SubDependency,
                Is.EqualTo(subDependency1));

            Assert.That(() => dependency2.SubDependency,
                Is.EqualTo(subDependency1));
        }

        [Test]
        public void It_should_manage_Lifestyle_properly_inside_handling()
        {
            const int NumberOfRequests = 100;

            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            container.Register(
                Component.For<InstanceTracker>()
                    .LifeStyle.Singleton
                    .ImplementedBy<InstanceTracker>(),
                Component.For<ISubDependency>()
                    .LifeStyle.PerRequestHandling()
                    .ImplementedBy<SubDependency>(),
                Component.For<IDependency>()
                    .LifeStyle.PerRequestHandling()
                    .ImplementedBy<Dependency>(),
                Component.For<ISideEffectFreeRequestHandler<TestRequest, TestResponse>>()
                    .LifeStyle.Transient
                    .ImplementedBy<TestRequestHandler>()
            );

            var messageBus = container.Resolve<IMessageBus>();

            for (int i = 0; i < NumberOfRequests; i++)
            {
                messageBus.Send(new TestRequest());
            }

            var instanceTracker = container.Resolve<InstanceTracker>();

            Assert.That(() => instanceTracker.NumInstanceCreated,
                Is.EqualTo(NumberOfRequests));
        }

        [Test]
        public void It_should_manage_Lifestyle_properly_inside_handling_with_parallelism()
        {
            const int NumberOfRequests = 100;

            var container = new WindsorContainer();
            container.AddFacility<ColomboFacility>();

            container.Register(
                Component.For<InstanceTracker>()
                    .LifeStyle.Singleton
                    .ImplementedBy<InstanceTracker>(),
                Component.For<ISubDependency>()
                    .LifeStyle.PerRequestHandling()
                    .ImplementedBy<SubDependency>(),
                Component.For<IDependency>()
                    .LifeStyle.PerRequestHandling()
                    .ImplementedBy<Dependency>(),
                Component.For<ISideEffectFreeRequestHandler<TestRequest, TestResponse>>()
                    .LifeStyle.Transient
                    .ImplementedBy<TestRequestHandler>()
            );

            var messageBus = container.Resolve<IMessageBus>();

            for (int i = 0; i < NumberOfRequests; i++)
            {
                messageBus.Send(new TestRequest(), new TestRequest(), new TestRequest(), new TestRequest());
            }

            var instanceTracker = container.Resolve<InstanceTracker>();

            Assert.That(() => instanceTracker.NumInstanceCreated,
                Is.EqualTo(NumberOfRequests * 4));
        }

        public class TestRequest : SideEffectFreeRequest<TestResponse>
        {
            public override string GetGroupName()
            {
                return "asdfghjk";
            }
        }

        public class TestRequestHandler : SideEffectFreeRequestHandler<TestRequest, TestResponse>
        {
            private readonly InstanceTracker tracker;
            private readonly IDependency dependency;
            private readonly ISubDependency subDependency;

            public TestRequestHandler(InstanceTracker tracker, IDependency dependency, ISubDependency subDependency)
            {
                this.tracker = tracker;
                this.dependency = dependency;
                this.subDependency = subDependency;
            }

            protected override void Handle()
            {
                dependency.Verify();
                tracker.ItShouldExists(subDependency);
            }
        }

        public interface IDependency
        {
            void Verify();
            ISubDependency SubDependency { get; }
        }

        public class Dependency : IDependency
        {
            private readonly InstanceTracker tracker;
            private readonly ISubDependency subDependency;

            public Dependency(InstanceTracker tracker, ISubDependency subDependency)
            {
                this.tracker = tracker;
                this.subDependency = subDependency;
            }

            public void Verify()
            {
                tracker.ItShouldNotExists(subDependency);
            }

            public ISubDependency SubDependency { get { return subDependency; } }
        }

        public interface ISubDependency { }

        public class SubDependency : ISubDependency { }

        public class InstanceTracker
        {
            private IList<ISubDependency> subDependencies = new List<ISubDependency>();

            public void ItShouldNotExists(ISubDependency subDependency)
            {
                Assert.That(() => subDependencies.Contains(subDependency),
                    Is.False);
                subDependencies.Add(subDependency);
            }

            public void ItShouldExists(ISubDependency subDependency)
            {
                Assert.That(() => subDependencies.Contains(subDependency),
                    Is.True);
            }

            public void AddInstanceIfNotThere(ISubDependency subDependency)
            {
                if (!subDependencies.Contains(subDependency))
                    subDependencies.Add(subDependency);
            }

            public int NumInstanceCreated { get { return subDependencies.Count; } }
        }
    }
}
