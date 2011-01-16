using System;
using System.Linq;

namespace Colombo.Impl.RequestHandle
{
    /// <summary>
    /// Holds extension methods the applies to <see cref="IRequestHandler"/>.
    /// </summary>
    public static class RequestHandlerExtensions
    {
        /// <summary>
        /// Return a custom attribute applied to the <paramref name="requestHandler"/>.
        /// Return null if not found.
        /// </summary>
        public static TAttribute GetCustomAttribute<TAttribute>(this IRequestHandler requestHandler, bool inherit = false)
            where TAttribute : Attribute
        {
            return (TAttribute)requestHandler.GetType().GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault();
        }
    }
}
