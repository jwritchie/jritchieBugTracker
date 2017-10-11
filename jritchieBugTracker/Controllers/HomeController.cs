using jritchieBugTracker.Models;
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
        public ActionResult Index()
        {
            return View();
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