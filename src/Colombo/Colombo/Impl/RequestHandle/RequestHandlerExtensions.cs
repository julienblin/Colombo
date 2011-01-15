using System;
using System.Linq;

namespace Colombo.Impl.RequestHandle
{
    public static class RequestHandlerExtensions
    {
        public static TAttribute GetCustomAttribute<TAttribute>(this IRequestHandler requestHandler, bool inherit = false)
            where TAttribute : Attribute
        {
            return (TAttribute)requestHandler.GetType().GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault();
        }
    }
}
