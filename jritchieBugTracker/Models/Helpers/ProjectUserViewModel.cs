using jritchieBugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jritchieBugTracker.Models.Helpers
{
    public class ProjectUserViewModel
    {
        public Project AssignProject { get; set; }
        public int AssignProjectId { get; set; }
        public MultiSelectList Users { get; set; }
        public string[] SelectedUsers { get; set; }
        public string[] SelectedUsersName { get; set; }
    }
}