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
using Microsoft.AspNet.Identity;
using System.Net.Mail;
using System.Threading.Tasks;

namespace jritchieBugTracker.Controllers
{
    public class TicketCommentsController : UniversalController
    {
        // GET: TicketComments
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            //var ticketComments = db.TicketComments.Include(t => t.Author).Include(t => t.Ticket);
            //return View(ticketComments.ToList());

            //*************************************************************************************
            //var user = db.Users.Find(User.Identity.GetUserId());
            //if (User.IsInRole("Admin"))
            //{
            //    return View(db.Tickets.ToList());
            //}
            //if (User.IsInRole("ProjectManager"))
            //{
            //    return View(db.Tickets.Where(t => t.Project.Users.Any(u => u.Id == user.Id)).ToList());
            //}
            //if (User.IsInRole("Developer"))
            //{
            //    return View(db.Tickets.Where(t => t.AssignToUserId == user.Id).ToList());
            //}
            //if (User.IsInRole("Submitter"))
            //{
            //    return View(db.Tickets.Where(t => t.OwnerUserId == user.Id).ToList());
            //}

            ////return View("NoTickets");
            //return RedirectToAction("Index", "Home");
            //*************************************************************************************

            List<Ticket> UsersTickets = new List<Ticket>();
            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Admin"))
            {
                return View(db.Tickets.ToList());
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
                return View(UsersTickets.Distinct());
            }

            return View("NoTickets", "Tickets");
            //return RedirectToAction("Index", "Home");
        }

        // GET: TicketComments/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }

            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Admin"))
            {
                return View(ticketComment);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: TicketComments/Create
        //[Authorize]
        //public ActionResult Create()
        //{
        //    ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName");
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
        //    return View();
        //}

        // POST: TicketComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Body,Created,Updated,TicketId,AuthorId")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {
                ticketComment.Created = DateTimeOffset.UtcNow;
                ticketComment.AuthorId = User.Identity.GetUserId();

                db.TicketComments.Add(ticketComment);
                db.SaveChanges();
                //return RedirectToAction("Index");

                // Notify Developer of new Comment
                try
                {
                    var body = "<p>{0}</p><p>{1}</p>";
                    var from = "Resolve()<jritchie.projects@gmail.com>";
                    var developer = db.Users.Find(ticketComment.Ticket.AssignToUserId).Fullname;

                    var email = new MailMessage(from, db.Users.Find(ticketComment.Ticket.OwnerUserId).Email)
                    {
                        Subject = "Resolve() Notification Email: New ticket assigned",
                        Body = string.Format(body, "Message from Resolve():", "Your ticket: '" + ticketComment.Ticket.Title + "', has a new Comment."),
                        IsBodyHtml = true
                    };

                    var svc = new PersonalEmail();
                    await svc.SendAsync(email);
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.FromResult(0);
                }
            }

            //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketComment.AuthorId);
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
            //return View(ticketComment);

            return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId } );
        }

        // GET: TicketComments/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }

            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Admin"))
            {
                return View(ticketComment);
            }
            else if (User.IsInRole("ProjectManager") && !ticketComment.Ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (ticketComment.AuthorId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (user.Roles.Count == 0)
            {
                return View("NoTickets");
            }

            //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketComment.AuthorId);
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
            //return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            return View(ticketComment);
        }

        // POST: TicketComments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Body,Created,Updated,TicketId,AuthorId")] TicketComment ticketComment)
        {
            if (ModelState.IsValid)
            {
                ticketComment.Updated = DateTimeOffset.UtcNow;

                db.Entry(ticketComment).State = EntityState.Modified;
                db.SaveChanges();
                //return RedirectToAction("Index");

                // Notify Developer of edited Comment
                try
                {
                    var body = "<p>{0}</p><p>{1}</p>";
                    var from = "Resolve()<jritchie.projects@gmail.com>";
                    var developer = db.Users.Find(ticketComment.Ticket.AssignToUserId).Fullname;

                    var email = new MailMessage(from, db.Users.Find(ticketComment.Ticket.OwnerUserId).Email)
                    {
                        Subject = "Resolve() Notification Email: New ticket assigned",
                        Body = string.Format(body, "Message from Resolve():", "Your ticket: '" + ticketComment.Ticket.Title + "', has an edited Comment."),
                        IsBodyHtml = true
                    };

                    var svc = new PersonalEmail();
                    await svc.SendAsync(email);
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    await Task.FromResult(0);
                }

                return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            }
            //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketComment.AuthorId);
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketComment.TicketId);
            return View(ticketComment);
        }

        // GET: TicketComments/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }

            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Admin"))
            {
                return View(ticketComment);
            }
            else if (User.IsInRole("ProjectManager") && !ticketComment.Ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (ticketComment.AuthorId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (user.Roles.Count == 0)
            {
                return View("NoTickets");
            }

            //return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            return View(ticketComment);
        }

        // POST: TicketComments/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            TicketComment ticketComment = db.TicketComments.Find(id);
            db.TicketComments.Remove(ticketComment);
            db.SaveChanges();
            //return RedirectToAction("Index");

            // Notify Developer of deleted Comment.
            try
            {
                var body = "<p>{0}</p><p>{1}</p>";
                var from = "Resolve()<jritchie.projects@gmail.com>";
                var developer = db.Users.Find(ticketComment.Ticket.AssignToUserId).Fullname;

                var email = new MailMessage(from, db.Users.Find(ticketComment.Ticket.OwnerUserId).Email)
                {
                    Subject = "Resolve() Notification Email: New ticket assigned",
                    Body = string.Format(body, "Message from Resolve():", "A Comment has been deleted from your ticket: '" + ticketComment.Ticket.Title + "'."),
                    IsBodyHtml = true
                };

                var svc = new PersonalEmail();
                await svc.SendAsync(email);
            }

            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                await Task.FromResult(0);
            }

            return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
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
