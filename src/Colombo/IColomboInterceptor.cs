using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    /// <summary>
    /// Common interface for all the interceptors.
    /// </summary>
    public interface IColomboInterceptor
    {
        /// <summary>
        /// Priority of the interceptor execution. Please use values from <see cref="InterceptorPrority"/>.
        /// </summary>
        int InterceptionPriority { get; }
    }

    public static class InterceptorPrority
    {
        public const int ReservedHigh = 0;
        public const int High = 1;
        public const int Medium = 5;
        public const int Low = 10;
        public const int ReservedLow = 11;
    }
}
