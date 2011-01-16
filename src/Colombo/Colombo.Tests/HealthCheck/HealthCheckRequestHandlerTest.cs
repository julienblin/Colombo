using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.HealthCheck;
using NUnit.Framework;

namespace Colombo.Tests.HealthCheck
{
    [TestFixture]
    public class HealthCheckRequestHandlerTest
    {
        [Test]
        public void It_should_return_an_ACKResponse()
        {
            var healthCheckRequestHandler = new HealthCheckRequestHandler();
            var response = healthCheckRequestHandler.Handle(new HealthCheckRequest());

            Assert.That(() => response,
                Is.Not.Null);

            Assert.That(() => response,
                Is.TypeOf<ACKResponse>());
        }
    }
}
