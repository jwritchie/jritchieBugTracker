using jritchieBugTracker.Models.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jritchieBugTracker.Controllers
{
    public class AdminController : Controller
    {
        // Instantiate Helper class.
        private UserRoleHelper helper = new UserRoleHelper();


        // GET: Admin
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            helper.IsUserInRole(userId, "Admin");
            return View();
        }



        // GET: Is user in role?
        public ActionResult UserInRole(string userId, string roleType)
        {
            helper.IsUserInRole(userId, roleType);
            return View();
        }

        // GET: List user's roles.
        public ActionResult UserRoles(string userId)
        {
            helper.ListUserRoles(userId);
            return View();
        }


        // GET: Edit (add) list of user's roles.
        //[Authorize(Roles = "Admin")]
        public ActionResult AddRole(string userId)
        {
            helper.ListUserRoles(userId);
            return View();
        }

        // POST: Edit (add) role to a user.
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRole(string userId, string roleName)
        {
            helper.AddUserToRole(userId, roleName);
            return View();
        }

        // GET: Edit (remove) list of user's roles.
        //[Authorize(Roles = "Admin")]
        public ActionResult RemoveRole(string userId)
        {
            helper.ListUserRoles(userId);
            return View();
        }

        // POST: Edit (remove) role to a user.
        //[Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveRole(string userId, string roleName)
        {
            helper.RemoveUserFromRole(userId, roleName);
            return View();
        }


    }
}