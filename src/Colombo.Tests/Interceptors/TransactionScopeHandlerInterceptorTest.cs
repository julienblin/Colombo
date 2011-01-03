using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Colombo.Interceptors;
using System.Transactions;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class TransactionScopeHandlerInterceptorTest
    {
        [Test]
        public void It_should_create_a_transaction_scope_for_handling()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();

            var invocation = mocks.StrictMock<IColomboInvocation>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
                LastCall.Do(new ProceedDelegate(() =>
                {
                    Assert.That(() => Transaction.Current,
                        Is.Not.Null);
                }));
            }).Verify(() =>
            {
                Assert.That(() => Transaction.Current,
                        Is.Null);
                var interceptor = new TransactionScopeHandlerInterceptor();
                interceptor.Intercept(invocation);
                Assert.That(() => Transaction.Current,
                        Is.Null);
            });
        }

        public delegate void ProceedDelegate();
    }
}
