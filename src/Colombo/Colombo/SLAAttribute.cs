using System;

namespace Colombo
{
    /// <summary>
    /// Indicates that a request should complete within a certain amount of time.
    /// </summary>
    /// <see cref="Colombo.Interceptors.SLASendInterceptor"/>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class SLAAttribute : Attribute
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allowedMilliseconds">Number of milliseconds that the time taken to complete the request should not exceed.</param>
        public SLAAttribute(int allowedMilliseconds) : this(new TimeSpan(0, 0, 0, 0, allowedMilliseconds)) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allowed">TimeSpan that the time taken to complete the request should not exceed.</param>
        public SLAAttribute(TimeSpan allowed)
        {
            Allowed = allowed;
        }

        /// <summary>
        /// Allowed time.
        /// </summary>
        public TimeSpan Allowed { get; private set; }
    }
}
