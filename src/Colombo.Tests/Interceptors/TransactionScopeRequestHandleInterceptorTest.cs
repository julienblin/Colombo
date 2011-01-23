using System.Transactions;
using Colombo.Interceptors;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class TransactionScopeRequestHandleInterceptorTest
    {
        [Test]
        public void It_should_create_a_transaction_scope_for_handling()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();

            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();

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
                var interceptor = new TransactionScopeRequestHandleInterceptor();
                interceptor.Intercept(invocation);
                Assert.That(() => Transaction.Current,
                        Is.Null);
            });
        }

        public delegate void ProceedDelegate();
    }
}
