#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

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
                    Assert.That(Transaction.Current, Is.Not.Null);
                }));
            }).Verify(() =>
            {
                Assert.That(Transaction.Current,
                        Is.Null);
                var interceptor = new TransactionScopeRequestHandleInterceptor();
                interceptor.Intercept(invocation);
                Assert.That(Transaction.Current, Is.Null);
            });
        }

        public delegate void ProceedDelegate();
    }
}
