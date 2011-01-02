using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Colombo.Configuration;

namespace Colombo.Tests.Configuration
{
    [TestFixture]
    public class EmptyColomboConfigurationTest
    {
        [Test]
        public void It_should_return_null_to_GetTargetAddressFor()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();

            var config = new EmptyColomboConfiguration();
            Assert.That(() => config.GetTargetAddressFor(request, "randomType"),
                Is.Null);
        }
    }
}
