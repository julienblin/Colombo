using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Interceptors
{
    public static class RequestExtensions
    {
        public static TAttribute GetCustomAttribute<TAttribute>(this BaseRequest request, bool inherit = false)
            where TAttribute : Attribute
        {
            return (TAttribute)request.GetType().GetCustomAttributes(typeof(TAttribute), inherit).FirstOrDefault();
        }

        public static TAttribute[] GetCustomAttributes<TAttribute>(this BaseRequest request, bool inherit = false)
            where TAttribute : Attribute
        {
            return  (TAttribute[])request.GetType().GetCustomAttributes(typeof(TAttribute), inherit);
        }
    }
}
