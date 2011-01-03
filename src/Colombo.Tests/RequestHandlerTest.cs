using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests
{
    [TestFixture]
    public class RequestHandlerTest
    {
        [Test]
        public void It_should_create_a_default_response_with_the_CorrelationGuid_of_the_request()
        {
            var mocks = new MockRepository();
            var requestHandler = mocks.StrictMock<RequestHandler<TestRequest, TestResponse>>();
            With.Mocks(mocks).Expecting(() =>
            {
                requestHandler.Handle();
                LastCall.Do(new HandleDelegate(() =>
                {
                    
                }));
            }).Verify(() =>
            {
                var request = new TestRequest();
                var response = requestHandler.Handle(request);
                Assert.AreEqual(request.CorrelationGuid, response.CorrelationGuid);
            });
        }

        public delegate void HandleDelegate();

        public class TestRequest : Request<TestResponse>
        {
        }
    }
}
