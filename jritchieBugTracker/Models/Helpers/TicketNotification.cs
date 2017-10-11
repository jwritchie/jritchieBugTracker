using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace jritchieBugTracker.Models.Helpers
{
    public static class TicketNotification
    {
        public static MailMessage EmailNotification(EmailModel model)
        {
            //try
            //{
                var body = "<p>Email From: <bold>{0}</bold>({1})</p><p>Message:</p><p>{2}</p>";
                var from = "BugTracker<jritchie.projects@gmail.com>";
                

                var email = new MailMessage(from, ConfigurationManager.AppSettings["emailto"])
                {
                    Subject = model.Subject,
                    Body = string.Format(body, model.Body),
                    IsBodyHtml = true
                };

                return email;             // Sends email.                                                        //return RedirectToAction("Sent");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message);
            //    await Task.FromResult(0);
            //}
        }

    }
}