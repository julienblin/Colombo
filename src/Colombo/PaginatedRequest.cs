using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Colombo
{
    /// <summary>
    /// Base class for requests that are side-effect free and paginated.
    /// Associated reponses must be <see cref="PaginatedResponse"/>.
    /// </summary>
    public abstract class PaginatedRequest<TResponse> : SideEffectFreeRequest<TResponse>
        where TResponse : PaginatedResponse, new()
    {

        private int currentPage = 1;
        /// <summary>
        /// Current page number, starts at 1 (and not 0).
        /// Defaults to 1.
        /// </summary>
        [Range(1, Int32.MaxValue)]
        public virtual int CurrentPage
        {
            get { return currentPage; }
            set { currentPage = value; }
        }

        private int perPage = 30;
        /// <summary>
        /// Number of entries per page.
        /// Defaults to 30.
        /// </summary>
        [Range(1, Int32.MaxValue)]
        public virtual int PerPage
        {
            get { return perPage; }
            set { perPage = value; }
        }
    }
}
