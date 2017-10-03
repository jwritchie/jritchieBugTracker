using jritchieBugTracker.Models.CodeFirst;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jritchieBugTracker.Models.Helpers
{
    public class TicketHelper
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // Check whether User is on a Project.
        public bool IsUserOnProject(string userId, int projectId)
        {
            var project = db.Projects.Find(projectId);
            var userBool = project.Users.Any(u => u.Id == userId);
            return userBool;
        }

        //***************************************************************************************************
        public bool IsUserOnTicket(string userId, int ticketId)
        {
            var ticket = db.Tickets.Find(ticketId);
            var userBool = false;
            if (ticket.AssignToUserId == userId)
            {
                userBool = true;
            }
            return userBool;
        }
        //***************************************************************************************************



        // Add a User to a Project.
        public void AddUserToProject(string userId, int projectId)
        {
            var user = db.Users.Find(userId);
            var project = db.Projects.Find(projectId);
            project.Users.Add(user);
            db.SaveChanges();
        }

        //***************************************************************************************************
        //public void AddUserToTicket(string userId, int ticketId)
        //{
        //    var user = db.Users.Find(userId);
        //    var ticket = db.Tickets.Find(ticketId);
        //    ticket.AssignToUser.
        //}
        //***************************************************************************************************


        // Remove a User from a Project.
        public void RemoveUserFromProject(string userId, int projectId)
        {
            var user = db.Users.Find(userId);
            var project = db.Projects.Find(projectId);
            project.Users.Remove(user);
            db.SaveChanges();
        }

        // List a User's Projects.
        public ICollection<Project> ListUserProjects(string userId)
        {
            var user = db.Users.Find(userId);
            return user.Projects.ToList();
        }
        
        // List all Users on a Project.
        public ICollection<ApplicationUser> ListUsersOnProject(int projectId)
        {
            var project = db.Projects.Find(projectId);
            return project.Users.ToList();
        }

        // List all Users not on a Project.
        public ICollection<ApplicationUser> ListUsersNotOnProject(int projectId)
        {
            return db.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToList();
        }

    }
}