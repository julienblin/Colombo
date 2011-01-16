namespace Colombo
{
    /// <summary>
    /// Common interface for all the interceptors.
    /// </summary>
    public interface IColomboInterceptor
    {
        /// <summary>
        /// Priority of the interceptor execution. Please use values from <see cref="InterceptionPrority"/>.
        /// </summary>
        int InterceptionPriority { get; }
    }

    /// <summary>
    /// Defines constant values for <see cref="IColomboInterceptor"/> priorities.
    /// </summary>
    /// <see cref="IColomboInterceptor.InterceptionPriority"/>
    public static class InterceptionPrority
    {
        internal const int ReservedHigh = 0;
        
        /// <summary>
        /// High priority - will run first
        /// </summary>
        public const int High = 1;

        /// <summary>
        /// Medium priority
        /// </summary>
        public const int Medium = 5;

        /// <summary>
        /// Low priority - will run last
        /// </summary>
        public const int Low = 10;

        internal const int ReservedLow = 11;
    }
}
