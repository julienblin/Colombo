using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Registration.Lifestyle;

namespace Colombo.Facilities
{
    public static class LifestyleRegistrationExtensions
    {
        /// <summary>
        /// Inside a request handling activity, resolve one instance per handling.
        /// Outside a request handling, resolve one instance per thread.
        /// </summary>
        public static ComponentRegistration<S> PerRequestHandling<S>(this LifestyleGroup<S> @group)
        {
            return group.Custom<PerRequestHandlingLifestyleManager>();
        }
    }
}
