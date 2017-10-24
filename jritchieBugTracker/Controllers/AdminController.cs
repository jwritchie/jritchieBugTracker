using jritchieBugTracker.Models;
using jritchieBugTracker.Models.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace jritchieBugTracker.Controllers
{
    [RequireHttps]
    [Authorize(Roles = "Admin")]
    public class AdminController : UniversalController
    {
        //GET: Admin
        public ActionResult Index()
        {
            List<AdminUserViewModel> users = new List<AdminUserViewModel>();
            foreach (var user in db.Users.ToList())
            {
                UserRoleHelper helper = new UserRoleHelper();
                AdminUserViewModel eachUser = new AdminUserViewModel();
                eachUser.User = user;
                eachUser.SelectedRoles = helper.ListUserRoles(user.Id).ToArray();

                users.Add(eachUser);
            }
            return View(users.OrderBy(u => u.User.LastName).ToList());
        }

        //GET: EditUserRoles
        public ActionResult EditUserRoles (string id)
        {
            var user = db.Users.Find(id);
            UserRoleHelper helper = new UserRoleHelper();
            AdminUserViewModel model = new AdminUserViewModel();
            model.User = user;
            model.SelectedRoles = helper.ListUserRoles(id).ToArray();
            model.Roles = new MultiSelectList(db.Roles, "Name", "Name", model.SelectedRoles);

            return View(model);
        }

        //POST: EditUserRoles
        [HttpPost]
        public ActionResult EditUserRoles(AdminUserViewModel model)
        {
            var user = db.Users.Find(model.User.Id);
            UserRoleHelper helper = new UserRoleHelper();

            // Remove existing Roles.
            foreach (var role in db.Roles.Select(r => r.Name).ToList())
            {
                helper.RemoveUserFromRole(user.Id, role);
            }

            // Add new Roles.
            if (model.SelectedRoles != null)
            {
                foreach (var role in model.SelectedRoles)
                {
                    helper.AddUserToRole(user.Id, role);
                }

                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        //POST: RemoveAllUserRoles
        [HttpPost]
        public ActionResult RemoveAllUserRoles(AdminUserViewModel model)
        {
            var user = db.Users.Find(model.User.Id);
            UserRoleHelper helper = new UserRoleHelper();

            foreach (var role in db.Roles.Select(r => r.Name).ToList())
            {
                helper.RemoveUserFromRole(user.Id, role);
            }

            return RedirectToAction("Index");
        }


        //GET: Admin
        public ActionResult DataTables()
        {
            List<AdminUserViewModel> users = new List<AdminUserViewModel>();
            foreach (var user in db.Users.ToList())
            {
                UserRoleHelper helper = new UserRoleHelper();
                AdminUserViewModel eachUser = new AdminUserViewModel();
                eachUser.User = user;
                eachUser.SelectedRoles = helper.ListUserRoles(user.Id).ToArray();

                users.Add(eachUser);
            }
            return View(users.OrderBy(u => u.User.LastName).ToList());
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}