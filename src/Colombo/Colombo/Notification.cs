using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Colombo
{
    public class Notification : BaseMessage
    {
        private IDictionary<string, string> context;
        /// <summary>
        /// Context of the notification. Garanteed to be non-null.
        /// </summary>
        public virtual IDictionary<string, string> Context
        {
            get { return context ?? (context = new Dictionary<string, string>()); }
            set
            {
                if (value == null) throw new ArgumentNullException("Context");
                Contract.EndContractBlock();
                context = value;
            }
        }

        /// <summary>
        /// Get the name of the group that this notification belongs to.
        /// </summary>
        /// <remarks>
        /// Defaults to the name of the assembly.
        /// </remarks>
        /// <returns></returns>
        public virtual string GetGroupName()
        {
            return GetType().Assembly.GetName().Name;
        }
    }
}
