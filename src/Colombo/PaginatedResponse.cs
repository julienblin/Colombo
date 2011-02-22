using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace Colombo
{
    /// <summary>
    /// Base class for responses that are paginated.
    /// </summary>
    [DataContract]
    public abstract class PaginatedResponse : Response
    {
        /// <summary>
        /// Current page number, starts at 1 (and not 0).
        /// </summary>
        [DataMember]
        public virtual int CurrentPage { get; set; }

        /// <summary>
        /// Number of entries per page.
        /// </summary>
        [DataMember]
        public virtual int PerPage { get; set; }

        /// <summary>
        /// Total number of entries without pagination.
        /// </summary>
        [DataMember]
        public virtual int TotalEntries { get; set; }

        /// <summary>
        /// Total number of pages.
        /// </summary>
        [DataMember]
        public virtual int TotalPages { get; set; }
    }
}
