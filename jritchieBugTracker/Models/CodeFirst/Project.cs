using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jritchieBugTracker.Models.CodeFirst
{
    public class Project
    {
        public Project()
        {
            // HashSets allow faster access to the data.
            Users = new HashSet<ApplicationUser>();     // Assign multiple users to projects.
            Tickets = new HashSet<Ticket>();            // Assign multiple tickets.
        }

        public int Id { get; set; } 
        public DateTimeOffset Created { get; set; } 
        public DateTimeOffset? Updated { get; set; }
        public string Title { get; set; }   
        public string Description { get; set; } 
        public string AuthorId { get; set; }            // Person who created project.
        public string Author { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; } 
        public virtual ICollection<Ticket> Tickets { get; set; }
    }
}