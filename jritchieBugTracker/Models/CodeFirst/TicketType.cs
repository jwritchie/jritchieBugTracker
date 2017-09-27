using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jritchieBugTracker.Models.CodeFirst
{
    public class TicketType
    { 
        // Lookup table.
        public int Id { get; set; }
        public string Name { get; set; }
    }
}