﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Impl
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
