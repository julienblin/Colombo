namespace Colombo.TestSupport
{
    /// <summary>
    /// Base class for expectations.
    /// </summary>
    public abstract class BaseExpectation
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stubMessageBus">The <see cref="IStubMessageBus"/> that created the expectation.</param>
        protected BaseExpectation(IStubMessageBus stubMessageBus)
        {
            StubMessageBus = stubMessageBus;
        }

        /// <summary>
        /// The <see cref="IStubMessageBus"/> that created the expectation.
        /// </summary>
        protected IStubMessageBus StubMessageBus { get; private set; }

        /// <summary>
        /// Number of times this expectation have been executed.
        /// </summary>
        public int NumCalled { get; protected set; }

        /// <summary>
        /// Number of times this expectation should be executed.
        /// </summary>
        public int ExpectedNumCalled { get; protected set; }

        internal abstract object Execute(object parameter);

        /// <summary>
        /// Verify that all the operations meet what this expectation planned.
        /// </summary>
        public abstract void Verify();
    }
}
