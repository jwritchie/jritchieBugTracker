using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace jritchieBugTracker.Models
{
    public class EmailModel
    {
        [Required, Display(Name = "Name")]                      // Displays 'Name' instead of property's name.
        public string FromName { get; set; }
        [Required, Display(Name = "Email"), EmailAddress]       // Validates email address.
        public string FromEmail { get; set; }
        public string Subject { get; set; }                     // Remove [Required] attribute to allow HomeController's Contact Action to generate Subject line.
        [Required]
        public string Body { get; set; }
    }
}