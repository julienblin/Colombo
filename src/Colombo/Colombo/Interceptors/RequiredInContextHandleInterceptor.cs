using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Core.Logging;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Interceptor that validates the request annotated with <see cref="RequiredInContextAttribute"/>.
    /// Throws a <see cref="RequiredInContextException"/> if the key is null or empty.
    /// </summary>
    public class RequiredInContextHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        /// <summary>
        /// Performs the validation.
        /// </summary>
        public void Intercept(IColomboRequestHandleInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            var reqAttributes = nextInvocation.Request.GetCustomAttributes<RequiredInContextAttribute>(true);
            if (reqAttributes.Length > 0)
            {
                var keys = reqAttributes.SelectMany(x => x.GetKeys()).Distinct();
                var missingKeys = keys.Where(key => !nextInvocation.Request.Context.Keys.Contains(key) || string.IsNullOrEmpty(nextInvocation.Request.Context[key])).ToList();

                if (missingKeys.Count > 0)
                    LogAndThrowError("Missing keys in Context: {0}", string.Join(", ", missingKeys));
            }

            nextInvocation.Proceed();
        }

        /// <summary>
        /// Medium.
        /// </summary>
        public int InterceptionPriority
        {
            get { return InterceptionPrority.Medium; }
        }

        private void LogAndThrowError(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");
            Contract.EndContractBlock();

            var errorMessage = string.Format(format, args);
            Logger.Error(errorMessage);
            throw new RequiredInContextException(errorMessage);
        }
    }
}
