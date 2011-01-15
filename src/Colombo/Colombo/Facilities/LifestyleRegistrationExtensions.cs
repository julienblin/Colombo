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
        public static ComponentRegistration<TS> PerRequestHandling<TS>(this LifestyleGroup<TS> @group)
        {
            return group.Custom<PerRequestHandlingLifestyleManager>();
        }
    }
}
