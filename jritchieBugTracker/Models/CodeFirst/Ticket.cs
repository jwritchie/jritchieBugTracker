using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace jritchieBugTracker.Models.CodeFirst
{
    public class Ticket
    {
        public Ticket()
        {
            Comments = new HashSet<TicketComment>();
            Attachments = new HashSet<TicketAttachment>();
            Histories = new HashSet<TicketHistory>();
        }

        // Properties of Ticket - These turn into columns in SQL.
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }

        public DateTimeOffset Created { get; set; }
        public DateTimeOffset? Updated { get; set; }

        [Required]
        [Display(Name = "Project Name")]
        public int ProjectId { get; set; }
        [Required]
        [Display(Name = "Ticket Type")]
        public int TicketTypeId { get; set; }
        [Required]
        [Display(Name = "Ticket Priority")]
        public int TicketPriorityId { get; set; }
        [Required]
        [Display(Name = "Ticket Status")]
        public int TicketStatusId { get; set; }

        public string OwnerUserId { get; set; }

        [Display(Name = "Assigned Developer")]
        public string AssignToUserId { get; set; }
        
        // Navigational properties for our children - These do not end up as SQL columns...
        public virtual Project Project { get; set; }    
        public virtual TicketType TicketType { get; set; }
        public virtual TicketPriority TicketPriority { get; set; }
        public virtual TicketStatus TicketStatus { get; set; }
        public virtual ApplicationUser OwnerUser { get; set; }
        public virtual ApplicationUser AssignToUser { get; set; }

        public virtual ICollection<TicketComment> Comments { get; set; }
        public virtual ICollection<TicketAttachment> Attachments { get; set; }
        public virtual ICollection<TicketHistory> Histories { get; set; }
    }
}