using jritchieBugTracker.Models;
using jritchieBugTracker.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace jritchieBugTracker.Controllers
{
    public class HomeController : UniversalController
    {
        public ActionResult LandingPage()
        {
            return View();
        }

        public ActionResult Index()
        {
            DashboardViewModel dashboard = new DashboardViewModel();
            dashboard.Users = db.Users.ToList();
            dashboard.Projects = db.Projects.ToList();
            dashboard.Tickets = db.Tickets.ToList();

            //foreach (var project in db.Projects.ToList())
            //{
            //    DashboardViewModel dashVM = new DashboardViewModel();
            //    //dashVM.UsersName = project.Users.OrderBy(u => u.LastName).Select(u => u.Fullname).ToArray();

            //    dashVM.Project = project;

            //    dashboard.Add(dashVM);
            //}

            ViewBag.Dashboard = dashboard.Projects.Count();
            return View(dashboard);

            //dashboard.Projects = db.Projects.

            //List < DashboardViewModel > projects = new List<DashboardViewModel>();
            //foreach (var project in db.Projects.ToList())
            //{
            //    ProjectUserViewModel projectUserVM = new ProjectUserViewModel();

            //    projectUserVM.AssignProject = project;
            //    projectUserVM.AssignProjectId = project.Id;
            //    projectUserVM.SelectedUsers = project.Users.Select(u => u.Id).ToArray();    // existing users.
            //    projectUserVM.Users = new MultiSelectList(db.Users.ToList(), "Id", "Fullname", projectUserVM.SelectedUsers);
            //    projectUserVM.SelectedUsersName = project.Users.OrderBy(u => u.LastName).Select(u => u.Fullname).ToArray();

            //    ViewBag.UserTimeZone = db.Users.Find(User.Identity.GetUserId()).TimeZone;

            //    projects.Add(projectUserVM);
            //}

            //return View(projects.OrderBy(p => p.AssignProjectId).ToList());


            //return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Contact(EmailModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var body = "<p>Email From: <bold>{0}</bold>({1})</p><p>Message:</p><p>{2}</p>";
                    var from = "BugTracker<jritchie.projects@gmail.com>";
                    //model.Body = "This is a message from your portfolio site.  The name and the email of the contacting person is above.";
                    string subject = null;
                    if (model.Subject != null)
                    {
                        subject = "BugTracker Contact Email: " + model.Subject;
                    }
                    else
                    {
                        subject = "BugTracker Contact Email";
                    }


                    var email = new MailMessage(from, ConfigurationManager.AppSettings["emailto"])
                    {
                        //Subject = "Portfolio Contact Email" + model.Subject,
                        Subject = subject,
                        Body = string.Format(body, model.FromName, model.FromEmail, model.Body),
                        IsBodyHtml = true
                    };

                    var svc = new PersonalEmail();
                    await svc.SendAsync(email);             // Sends email.

                    return View(new EmailModel());          // Return to View (need to send empty Email model)
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.FromResult(0);
                }
            }
            return View(model);
        }

    }
}