﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.TestSupport
{
    /// <summary>
    /// Expectation that is used to execute a <see cref="IRequestHandler"/>.
    /// </summary>
    public class RequestHandlerExpectation<THandler> : BaseExpectation
        where THandler : IRequestHandler
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RequestHandlerExpectation(IStubMessageBus stubMessageBus) : base(stubMessageBus)
        {
            ExpectedNumCalled = 1;
        }

        internal override object Execute(object parameter)
        {
            ++NumCalled;
            var requestHandler = StubMessageBus.Kernel.Resolve<THandler>();
            return requestHandler.Handle((BaseRequest)parameter);
        }

        /// <summary>
        /// Indicates that this expectation should be repeated n <paramref name="times"/>.
        /// </summary>
        public RequestHandlerExpectation<THandler> Repeat(int times)
        {
            ExpectedNumCalled = times;
            return this;
        }

        /// <summary>
        /// Verify that all the operations meet what this expectation planned.
        /// </summary>
        public override void Verify()
        {
            if (ExpectedNumCalled != NumCalled)
                throw new ColomboExpectationException(string.Format("Expected request handler {0} to be invoked {1} time(s), actual: {2}", typeof(THandler), ExpectedNumCalled, NumCalled));
        }
    }
}
