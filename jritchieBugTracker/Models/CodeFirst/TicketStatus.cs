﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jritchieBugTracker.Models.CodeFirst
{
    public class TicketStatus
    {
        // Lookup table.
        public int Id { get; set; }
        public string Name { get; set; }
    }
}