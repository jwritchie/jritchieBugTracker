using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace jritchieBugTracker.Models.Helpers
{
    public class UserRoleHelper
    {
        // Instantiate userManager, which requires instantiating userStore.
        private UserManager<ApplicationUser> userManager =
            new UserManager<ApplicationUser>(new UserStore<ApplicationUser>
                (new ApplicationDbContext()));

        private ApplicationDbContext db = new ApplicationDbContext();


        // Check whether user is in role.
        public bool IsUserInRole(string userId, string roleName)
        {
            return userManager.IsInRole(userId, roleName);
        }

        // List of user's roles.
        public ICollection<string> ListUserRoles(string userId)
        {
            return userManager.GetRoles(userId);
        }

        // Add role to a user.
        public bool AddUserToRole(string userId, string roleName)
        {
            var result = userManager.AddToRole(userId, roleName);
            return result.Succeeded;
        }

        // Remove role from user.
        public bool RemoveUserFromRole(string userId, string roleName)
        {
            var result = userManager.RemoveFromRole(userId, roleName);
            return result.Succeeded;
        }

        // Find all users in a role.
        public ICollection<ApplicationUser> UsersInRole(string roleName)
        {
            List<ApplicationUser> resultList = new List<ApplicationUser>();
            List<ApplicationUser> List = userManager.Users.ToList();
            foreach(var user in List)
            {
                if(IsUserInRole(user.Id, roleName))
                {
                    resultList.Add(user);
                }
            }
            return resultList;
        }

        // Find users not in a role.
        public ICollection<ApplicationUser> UsersNotInRole(string roleName)
        {
            List<ApplicationUser> resultList = new List<ApplicationUser>();
            List<ApplicationUser> List = userManager.Users.ToList();
            foreach(var user in List)
            {
                if(!IsUserInRole(user.Id, roleName))
                {
                    resultList.Add(user);
                }
            }
            return resultList;
        }

    }
}