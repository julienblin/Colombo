namespace Colombo
{
    /// <summary>
    /// Base class for side effect-free requests - non generic version.
    /// </summary>
    public abstract class BaseSideEffectFreeRequest : BaseRequest
    {
        /// <summary>
        /// <c>true</c>.
        /// </summary>
        public override bool IsSideEffectFree
        {
            get { return true; }
        }
    }
}
