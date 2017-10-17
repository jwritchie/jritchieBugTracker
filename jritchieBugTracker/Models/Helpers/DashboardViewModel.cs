using jritchieBugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jritchieBugTracker.Models.Helpers
{
    public class DashboardViewModel
    {
        //public string[] UsersName { get; set; }

        //public ApplicationUser User { get; set; }
        //public Project Project { get; set; }
        //public Ticket Ticket { get; set; }

        public DashboardViewModel()
        {
            // HashSets allow faster access to the data.
            Users = new HashSet<ApplicationUser>();     // Assign multiple users to projects.
            Tickets = new HashSet<Ticket>();            // Assign multiple tickets.
            Projects = new HashSet<Project>();
        }

        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}