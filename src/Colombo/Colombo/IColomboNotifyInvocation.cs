using System.Collections.Generic;

namespace Colombo
{
    /// <summary>
    /// Invocation used in the interception process for sending/notifying notifications.
    /// </summary>
    public interface IColomboNotifyInvocation
    {
        /// <summary>
        /// The notifications that will be sent.
        /// </summary>
        IList<Notification> Notifications { get; set; }

        /// <summary>
        /// Proceed with the invocation.
        /// </summary>
        void Proceed();
    }
}
