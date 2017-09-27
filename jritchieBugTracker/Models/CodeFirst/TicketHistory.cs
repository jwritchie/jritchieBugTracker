using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jritchieBugTracker.Models.CodeFirst
{
    public class TicketHistory
    {
        public int Id { get; set; }
        public int TicketId { get; set; }
        public string Property { get; set; }                // Property which was changed.
        public string OldValue { get; set; }                // Property's old value.
        public string NewValue { get; set; }                // Property's new value.
        public DateTimeOffset Created { get; set; }
        public string AuthorId { get; set; }

        public virtual Ticket Ticket { get; set; }
        public virtual ApplicationUser Author { get; set; }
    }
}