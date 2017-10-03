using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using jritchieBugTracker.Models;
using jritchieBugTracker.Models.CodeFirst;
using jritchieBugTracker.Models.Helpers;
using Microsoft.AspNet.Identity;


namespace jritchieBugTracker.Controllers
{
    public class ProjectsController : UniversalController
    {
        // GET: Projects
        //[Authorize]
        //public ActionResult Index()
        //{
        //    return View(db.Projects.OrderBy(p => p.Id).ToList());
        //}

        // GET: Projects
        [Authorize]
        public ActionResult Index(bool? allProjects)
        {
            string userId = User.Identity.GetUserId();

            if ((User.IsInRole("Admin") || User.IsInRole("ProjectManager")) && allProjects == true)
            {
                ViewBag.IndexType = "All";
                List<ProjectUserViewModel> projects = new List<ProjectUserViewModel>();
                foreach (var project in db.Projects.ToList())
                {
                    //ProjectAssignHelper helper = new ProjectAssignHelper();
                    ProjectUserViewModel projectUserVM = new ProjectUserViewModel();

                    projectUserVM.AssignProject = project;
                    projectUserVM.AssignProjectId = project.Id;
                    projectUserVM.SelectedUsers = project.Users.Select(u => u.Id).ToArray();    // existing users.
                    projectUserVM.Users = new MultiSelectList(db.Users.ToList(), "Id", "Fullname", projectUserVM.SelectedUsers);
                    projectUserVM.SelectedUsersName = project.Users.OrderBy(u => u.LastName).Select(u => u.Fullname).ToArray();

                    projects.Add(projectUserVM);
                }

                return View(projects.OrderBy(p => p.AssignProjectId).ToList());
            }
            else
            {
                ViewBag.IndexType = "Assigned";
                List<ProjectUserViewModel> usersProjects = new List<ProjectUserViewModel>();

                ProjectAssignHelper helper = new ProjectAssignHelper();
                List<Project> Projects = new List<Project>();
                Projects = helper.ListUserProjects(userId).ToList();

                foreach (var project in Projects)
                {
                    //ProjectAssignHelper helper = new ProjectAssignHelper();
                    ProjectUserViewModel projectUserVM = new ProjectUserViewModel();

                    projectUserVM.AssignProject = project;
                    projectUserVM.AssignProjectId = project.Id;
                    projectUserVM.SelectedUsers = project.Users.Select(u => u.Id).ToArray();
                    projectUserVM.Users = new MultiSelectList(db.Users.ToList(), "Id", "Fullname", projectUserVM.SelectedUsers);
                    projectUserVM.SelectedUsersName = project.Users.OrderBy(u => u.LastName).Select(u => u.Fullname).ToArray();

                    usersProjects.Add(projectUserVM);
                }

                return View(usersProjects.OrderBy(p => p.AssignProjectId).ToList());
            }
        }
        
        // GET: Projects/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            var user = db.Users.Find(User.Identity.GetUserId());

            ProjectAssignHelper helper = new ProjectAssignHelper();
            if (helper.IsUserOnProject(user.Id, project.Id))
            {
                ProjectUserViewModel projectUserVM = new ProjectUserViewModel();
                projectUserVM.AssignProject = project;
                projectUserVM.AssignProjectId = project.Id;
                projectUserVM.SelectedUsers = project.Users.Select(u => u.Id).ToArray();
                projectUserVM.Users = new MultiSelectList(db.Users.ToList(), "Id", "Fullname", projectUserVM.SelectedUsers);
                projectUserVM.SelectedUsersName = project.Users.OrderBy(u => u.LastName).Select(u => u.Fullname).ToArray();

                return View(projectUserVM);
            }
            return RedirectToAction("Index");
            
            //return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Created,Updated,Title,Description,AuthorId,Author")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.Created = DateTimeOffset.Now;
                project.AuthorId = User.Identity.GetUserId();
                project.Author = ViewBag.Fullname;

                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Created,Updated,Title,Description,AuthorId,Author")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.Updated = DateTimeOffset.Now;

                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View();
        }

        // POST: Projects/Delete/5
        //[Authorize(Roles = "Admin")]
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Project project = db.Projects.Find(id);
        //    db.Projects.Remove(project);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}


        //GET: Assign user(s) to project.
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult AssignProjectUser(int id)
        {
            var project = db.Projects.Find(id);
            ProjectUserViewModel projectUserVM = new ProjectUserViewModel();
            projectUserVM.AssignProject = project;
            projectUserVM.AssignProjectId = id;
            projectUserVM.SelectedUsers = project.Users.Select(u => u.Id).ToArray();    // existing users.
            projectUserVM.Users = new MultiSelectList(db.Users.OrderBy(u => u.LastName).ToList(), "Id", "FullName", projectUserVM.SelectedUsers);    //collection, submitted value, displayed value, existing values are highlighted
            return View(projectUserVM);
        }

        //POST: Assign user(s) to project.
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignProjectUser(ProjectUserViewModel model)
        {
            ProjectAssignHelper helper = new ProjectAssignHelper();

            // Remove existing users.
            foreach (var userId in db.Users.Select(r => r.Id).ToList())
            {
                helper.RemoveUserFromProject(userId, model.AssignProjectId);
            }

            // Assign new users.
            foreach(var userId in model.SelectedUsers)
            {
                helper.AddUserToProject(userId, model.AssignProjectId);
            }

            return RedirectToAction("Index");
        }

        //POST: Remove All User(s) from project.
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveAllUsers(ProjectUserViewModel model)
        {
            ProjectAssignHelper helper = new ProjectAssignHelper();

            // Remove existing users.
            foreach (var userId in db.Users.Select(r => r.Id).ToList())
            {
                helper.RemoveUserFromProject(userId, model.AssignProjectId);
            }

            return RedirectToAction("Index");
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
