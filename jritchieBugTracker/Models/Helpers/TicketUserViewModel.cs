using jritchieBugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jritchieBugTracker.Models.Helpers
{
    public class TicketUserViewModel
    {
        public Ticket AssignTicket { get; set; }
        public int AssignTicketId { get; set; }
        public MultiSelectList Users { get; set; }
        public string[] SelectedUsers { get; set; }
        public string[] SelectedUsersName { get; set; }
    }
}