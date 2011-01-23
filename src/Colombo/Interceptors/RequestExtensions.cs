using System;
using System.Linq;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Holds extensions methods for <see cref="BaseRequest"/>.
    /// </summary>
    public static class RequestExtensions
    {
        /// <summary>
        /// Return a custom attribute applied to the <paramref name="request"/>.
        /// Return null if not found.
        /// </summary>
        public static TAttribute GetCustomAttribute<TAttribute>(this BaseRequest request, bool inherit = false)
            where TAttribute : Attribute
        {
            return (TAttribute)request.GetType().GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault();
        }

        /// <summary>
        /// Return a list of custom attributes applied to the <paramref name="request"/>.
        /// Return an empty array if none is found.
        /// </summary>
        public static TAttribute[] GetCustomAttributes<TAttribute>(this BaseRequest request, bool inherit = false)
            where TAttribute : Attribute
        {
            return (TAttribute[])request.GetType().GetCustomAttributes(typeof(TAttribute), inherit);
        }
    }
}
