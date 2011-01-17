using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.TestSupport
{
    public abstract class BaseExpectation
    {
        protected BaseExpectation(IStubMessageBus stubMessageBus)
        {
            StubMessageBus = stubMessageBus;
        }

        protected IStubMessageBus StubMessageBus { get; private set; }

        public int NumCalled { get; protected set; }

        public int ExpectedNumCalled { get; protected set; }

        internal abstract object Execute(object parameter);

        public abstract void Verify();
    }
}
