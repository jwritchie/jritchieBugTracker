using jritchieBugTracker.Models;
using jritchieBugTracker.Models.CodeFirst;
using jritchieBugTracker.Models.Helpers;
using Microsoft.AspNet.Identity;
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
    [Authorize]
    public class HomeController : UniversalController
    {
        [AllowAnonymous]
        public ActionResult LandingPage()
        {
            return View();
        }

        public ActionResult Index()
        {
            var user = db.Users.Find(User.Identity.GetUserId());

            DashboardViewModel dashboard = new DashboardViewModel();
            dashboard.Users = db.Users.ToList();

            // Projects.
            //dashboard.Projects = db.Projects.ToList();
            if (User.IsInRole("Admin") || User.IsInRole("ProjectManager"))
            {
                dashboard.Projects = db.Projects.ToList();
            }
            
            ProjectAssignHelper helper = new ProjectAssignHelper();
            dashboard.AssignedProjects = helper.ListUserProjects(user.Id).ToList();

            //***********************************************************

                // Tickets.
                List<Ticket> UsersTickets = new List<Ticket>();
            if (User.IsInRole("Admin"))
            {
                dashboard.Tickets = db.Tickets.ToList();
            }
            if (User.IsInRole("ProjectManager"))
            {
                UsersTickets.AddRange(db.Tickets.Where(t => t.Project.Users.Any(u => u.Id == user.Id)).ToList());
            }
            if (User.IsInRole("Developer"))
            {
                UsersTickets.AddRange(db.Tickets.Where(t => t.AssignToUserId == user.Id).ToList());
            }
            if (User.IsInRole("Submitter"))
            {
                UsersTickets.AddRange(db.Tickets.Where(t => t.OwnerUserId == user.Id).ToList());
            }

            if (UsersTickets.Count != 0)
            {
                dashboard.Tickets = UsersTickets.Distinct().ToList();
            }


            return View(dashboard);
        }


        //*****************************************************************************

        //public ActionResult About()
        //{
        //    ViewBag.Message = "Your application description page.";

        //    return View();
        //}

        //public ActionResult Contact()
        //{
        //    ViewBag.Message = "Your contact page.";

        //    return View();
        //}


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<ActionResult> Contact(EmailModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var body = "<p>Email From: <bold>{0}</bold>({1})</p><p>Message:</p><p>{2}</p>";
        //            var from = "Resolve()<jritchie.projects@gmail.com>";
        //            //model.Body = "This is a message from Resolve().  The name and the email of the contacting person is above.";
        //            string subject = null;
        //            if (model.Subject != null)
        //            {
        //                subject = "Resolve() Contact Email: " + model.Subject;
        //            }
        //            else
        //            {
        //                subject = "Resolve() Contact Email";
        //            }


        //            var email = new MailMessage(from, ConfigurationManager.AppSettings["emailto"])
        //            {
        //                //Subject = "Resolve() Contact Email" + model.Subject,
        //                Subject = subject,
        //                Body = string.Format(body, model.FromName, model.FromEmail, model.Body),
        //                IsBodyHtml = true
        //            };

        //            var svc = new PersonalEmail();
        //            await svc.SendAsync(email);             // Sends email.

        //            return View(new EmailModel());          // Return to View (need to send empty Email model)
        //        }
        //        catch (Exception ex)
        //        {
        //            Console.WriteLine(ex.Message);
        //            await Task.FromResult(0);
        //        }
        //    }
        //    return View(model);
        //}

    }
}