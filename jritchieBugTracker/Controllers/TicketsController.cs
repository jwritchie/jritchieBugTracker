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
using jritchieBugTracker.Models.Helpers;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Configuration;

namespace jritchieBugTracker.Controllers
{
    public class TicketsController : UniversalController
    {
        // GET: Tickets
        [Authorize]
        public ActionResult Index()
        {
            //var tickets = db.Tickets.ToList();
            //return View(tickets);

            //var tickets = db.Tickets.Include(t => t.AssignToUser).Include(t => t.OwnerUser).Include(t => t.Project).Include(t => t.TicketPriority).Include(t => t.TicketStatus).Include(t => t.TicketType);
            //return View(tickets.ToList());

            //*************************************************************************
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
            //return View("NoTickets");

            //return RedirectToAction("Index", "Home");
            //return View();
            //*************************************************************************

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

            return View("NoTickets");
        }

        // GET: Tickets/Details/5
        [Authorize]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            var user = db.Users.Find(User.Identity.GetUserId());
            ViewBag.UserId = user.Id;
            if (user.Roles.Count == 0)
            {
                return View("NoTickets");
            }

            if (User.IsInRole("Admin"))
            {
                return View(ticket);
            }
            else if (User.IsInRole("ProjectManager") && ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return View(ticket);
            }
            else if (ticket.AssignToUserId == user.Id || ticket.OwnerUserId == user.Id)
            {
                return View(ticket);
            }
            //return View(ticket);
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }


        // GET: Tickets/Create
        [Authorize(Roles = "Submitter")]
        public ActionResult Create()
        {
            // Restrict to only Projects to which a Submitter belongs.
            ProjectAssignHelper helper = new ProjectAssignHelper();
            var projects = helper.ListUserProjects(db.Users.Find(User.Identity.GetUserId()).Id);
            ViewBag.ProjectId = new SelectList(projects, "Id", "Title");
            //ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title");

            ViewBag.AssignToUserId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name");
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name");
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name");
            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "Submitter")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignToUserId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                var user = db.Users.Find(User.Identity.GetUserId());
                ticket.OwnerUserId = user.Id;

                ticket.Created = DateTimeOffset.UtcNow;
                ticket.TicketStatusId = db.TicketStatuses.FirstOrDefault(t => t.Name == "Unassigned").Id;

                db.Tickets.Add(ticket);

                //*****************************************************************
                TicketHistory ticketHistory = new TicketHistory();
                ticketHistory.TicketId = ticket.Id;
                ticketHistory.Property = "Ticket Creation";
                ticketHistory.OldValue = "-";
                ticketHistory.NewValue = "Ticket Created";
                ticketHistory.Created = DateTimeOffset.UtcNow;
                ticketHistory.AuthorId = User.Identity.GetUserId();
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                ticketHistory.Property = "Ticket Name";
                ticketHistory.OldValue = "-";
                ticketHistory.NewValue = ticket.Title.ToString();
                ticketHistory.Created = DateTimeOffset.UtcNow;
                ticketHistory.AuthorId = User.Identity.GetUserId();
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                ticketHistory.Property = "Ticket Description";
                ticketHistory.OldValue = "-";
                ticketHistory.NewValue = ticket.Description.ToString();
                ticketHistory.Created = DateTimeOffset.UtcNow;
                ticketHistory.AuthorId = User.Identity.GetUserId();
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                ticketHistory.Property = "Ticket's Project";
                ticketHistory.OldValue = "-";

                //ticketHistory.NewValue = ticket.ProjectId.ToString();
                ticketHistory.NewValue = db.Projects.FirstOrDefault(p => p.Id == ticket.ProjectId).Title;

                ticketHistory.Created = DateTimeOffset.UtcNow;
                ticketHistory.AuthorId = User.Identity.GetUserId();
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                ticketHistory.Property = "Ticket Type";
                ticketHistory.OldValue = "-";
                ticketHistory.NewValue = db.TicketTypes.FirstOrDefault(t => t.Id == ticket.TicketTypeId).Name;
                ticketHistory.Created = DateTimeOffset.UtcNow;
                ticketHistory.AuthorId = User.Identity.GetUserId();
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                ticketHistory.Property = "Ticket Priority";
                ticketHistory.OldValue = "-";
                ticketHistory.NewValue = db.TicketPriorities.FirstOrDefault(t => t.Id == ticket.TicketPriorityId).Name;
                ticketHistory.Created = DateTimeOffset.UtcNow;
                ticketHistory.AuthorId = User.Identity.GetUserId();
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                ticketHistory.Property = "Ticket Status";
                ticketHistory.OldValue = "-";
                ticketHistory.NewValue = db.TicketStatuses.FirstOrDefault(t => t.Id == ticket.TicketStatusId).Name;
                ticketHistory.Created = DateTimeOffset.UtcNow;
                ticketHistory.AuthorId = User.Identity.GetUserId();
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                ticketHistory.Property = "Ticket Submitter";
                ticketHistory.OldValue = "-";
                ticketHistory.NewValue = db.Users.FirstOrDefault(u => u.Id == ticket.OwnerUserId).Fullname;
                ticketHistory.Created = DateTimeOffset.UtcNow;
                ticketHistory.AuthorId = User.Identity.GetUserId();
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                ticketHistory.Property = "Ticket Developer";
                ticketHistory.OldValue = "-";
                ticketHistory.NewValue = "No Developer assigned";
                ticketHistory.Created = DateTimeOffset.UtcNow;
                ticketHistory.AuthorId = User.Identity.GetUserId();
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();
                //*****************************************************************

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            // Restrict to only Projects to which a Submitter belongs.
            ProjectAssignHelper helper = new ProjectAssignHelper();
            var projects = helper.ListUserProjects(db.Users.Find(User.Identity.GetUserId()).Id);
            ViewBag.ProjectId = new SelectList(projects, "Id", "Title");

            ViewBag.AssignToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            //ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            ViewBag.Description = ticket.Description;
            ViewBag.AssignToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);

            var user = db.Users.Find(User.Identity.GetUserId());
            if (user.Roles.Count == 0)
            {
                return View("NoTickets");
            }

            if (User.IsInRole("Admin"))
            {
                return View(ticket);
            }
            else if (User.IsInRole("ProjectManager") && ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return View(ticket);
            }
            else if (ticket.AssignToUserId == user.Id || ticket.OwnerUserId == user.Id)
            {
                return View(ticket);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignToUserId")] Ticket ticket)
        {

            //******************
            // '.AsNoTracking' returns a new query where the entities returned will not be cached in the DbContext,
            //  this allows the db. to see 'ticket' and 'existingTicket' as two different objects.
            Ticket existingTicket = db.Tickets.AsNoTracking().FirstOrDefault(t => t.Id == ticket.Id);
            //******************

            if (ModelState.IsValid)
            {
                ticket.Updated = DateTimeOffset.UtcNow;

                //****************
                if (ticket.Title != existingTicket.Title)
                {
                    TicketHistory ticketHistory = new TicketHistory();

                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.Property = "Ticket Name";
                    ticketHistory.OldValue = existingTicket.Title.ToString();
                    ticketHistory.NewValue = ticket.Title.ToString();
                    ticketHistory.Created = DateTimeOffset.UtcNow;
                    ticketHistory.AuthorId = User.Identity.GetUserId();
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();
                }
                if (ticket.Description != existingTicket.Description)
                {
                    TicketHistory ticketHistory = new TicketHistory();

                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.Property = "Ticket Description";
                    ticketHistory.OldValue = existingTicket.Description.ToString();
                    ticketHistory.NewValue = ticket.Description.ToString();
                    ticketHistory.Created = DateTimeOffset.UtcNow;
                    ticketHistory.AuthorId = User.Identity.GetUserId();
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();
                }
                if (ticket.ProjectId != existingTicket.ProjectId)
                {
                    TicketHistory ticketHistory = new TicketHistory();

                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.Property = "Ticket's Project Id";
                    ticketHistory.OldValue = existingTicket.ProjectId.ToString();
                    ticketHistory.NewValue = ticket.ProjectId.ToString();
                    ticketHistory.Created = DateTimeOffset.UtcNow;
                    ticketHistory.AuthorId = User.Identity.GetUserId();
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();

                }
                if (ticket.TicketTypeId != existingTicket.TicketTypeId)
                {
                    TicketHistory ticketHistory = new TicketHistory();

                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.Property = "Ticket Type";
                    ticketHistory.OldValue = existingTicket.TicketType.Name;
                    ticketHistory.NewValue = db.TicketTypes.FirstOrDefault(t => t.Id == ticket.TicketTypeId).Name;
                    ticketHistory.Created = DateTimeOffset.UtcNow;
                    ticketHistory.AuthorId = User.Identity.GetUserId();
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();

                }
                if (ticket.TicketPriorityId != existingTicket.TicketPriorityId)
                {
                    TicketHistory ticketHistory = new TicketHistory();

                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.Property = "Ticket Priority";
                    ticketHistory.OldValue = existingTicket.TicketPriority.Name;
                    ticketHistory.NewValue = db.TicketPriorities.FirstOrDefault(t => t.Id == ticket.TicketPriorityId).Name;
                    ticketHistory.Created = DateTimeOffset.UtcNow;
                    ticketHistory.AuthorId = User.Identity.GetUserId();
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();

                }
                if (ticket.TicketStatusId != existingTicket.TicketStatusId)
                {
                    TicketHistory ticketHistory = new TicketHistory();

                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.Property = "Ticket Status";
                    ticketHistory.OldValue = existingTicket.TicketStatus.Name;
                    ticketHistory.NewValue = db.TicketStatuses.FirstOrDefault(t => t.Id == ticket.TicketStatusId).Name;
                    ticketHistory.Created = DateTimeOffset.UtcNow;
                    ticketHistory.AuthorId = User.Identity.GetUserId();
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();

                }
                if (ticket.OwnerUserId != existingTicket.OwnerUserId)
                {
                    TicketHistory ticketHistory = new TicketHistory();

                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.Property = "Ticket Submitter";
                    ticketHistory.OldValue = existingTicket.OwnerUser.Fullname;
                    ticketHistory.NewValue = db.Users.FirstOrDefault(u => u.Id == ticket.OwnerUserId).Fullname;
                    ticketHistory.Created = DateTimeOffset.UtcNow;
                    ticketHistory.AuthorId = User.Identity.GetUserId();
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();

                }
                if (ticket.AssignToUserId != existingTicket.AssignToUserId)
                {
                    TicketHistory ticketHistory = new TicketHistory();

                    ticketHistory.TicketId = ticket.Id;
                    ticketHistory.Property = "Ticket Developer";
                    ticketHistory.OldValue = existingTicket.AssignToUser.Fullname;
                    ticketHistory.NewValue = db.Users.FirstOrDefault(u => u.Id == ticket.AssignToUserId).Fullname;
                    ticketHistory.Created = DateTimeOffset.UtcNow;
                    ticketHistory.AuthorId = User.Identity.GetUserId();
                    db.TicketHistories.Add(ticketHistory);
                    db.SaveChanges();
                }
                //****************

                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();


                // Notify assigned Developer that their Ticket has been updated.
                if (ticket.AssignToUserId != User.Identity.GetUserId())
                {
                    try
                    {
                        var body = "<p>{0}</p><p>{1}</p>";
                        var from = "BugTracker<jritchie.projects@gmail.com>";

                        var email = new MailMessage(from, db.Users.Find(ticket.AssignToUserId).Email)
                        {
                            Subject = "BugTracker Notification Email: New ticket assigned",
                            Body = string.Format(body, "Message from BugTracker:", "Your ticket: '" + ticket.Title + "', has been updated by: '" + db.Users.Find(User.Identity.GetUserId()).Fullname +  "'."),
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

                return RedirectToAction("Index");
            }

            ViewBag.AssignToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ticket ticket = db.Tickets.Find(id);
            if (ticket == null)
            {
                return HttpNotFound();
            }

            var user = db.Users.Find(User.Identity.GetUserId());
            if (user.Roles.Count == 0)
            {
                return View("NoTickets");
            }

            if (User.IsInRole("Admin"))
            {
                return View(ticket);
            }
            else if (User.IsInRole("ProjectManager") && ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return View(ticket);
            }
            else if (ticket.AssignToUserId == user.Id || ticket.OwnerUserId == user.Id)
            {
                return View(ticket);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // POST: Tickets/Delete/5
        //[Authorize]
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Ticket ticket = db.Tickets.Find(id);
        //    db.Tickets.Remove(ticket);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}


        //GET: Assign User (Developer) to Ticket.
        [Authorize(Roles = "Admin, ProjectManager")]
        public ActionResult AssignTicketUser(int ticketId)
        {
            Ticket ticket = db.Tickets.Find(ticketId);

            var role = db.Roles.FirstOrDefault(r => r.Name == "Developer");
            var developers = db.Users.Where(u => u.Roles.Any(r => r.RoleId == role.Id));
            var devsInTicketProj = developers.Where(d => d.Projects.Any(p => p.Id == ticket.ProjectId));
            ViewBag.AssignToUserId = new SelectList(devsInTicketProj.OrderBy(d => d.LastName), "Id", "FullName", ticket.AssignToUserId);

            return View(ticket);
        }

        //POST: Assign User (Developer) to Ticket.
        [Authorize(Roles = "Admin, ProjectManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AssignTicketUser([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignToUserId")] Ticket model, string AssignToUserId)
        {
            if (ModelState.IsValid)
            {
                //var ticketCreator = db.Tickets.Find(model.Id).OwnerUserId;
                
                //model.AssignToUserId = AssignToUserId;
                model.TicketStatusId = db.TicketStatuses.FirstOrDefault(t => t.Name == "Assigned").Id;

                TicketHistory ticketHistory = new TicketHistory();
                ticketHistory.TicketId = model.Id;
                ticketHistory.Property = "Ticket Developer";
                ticketHistory.OldValue = "No Developer Assigned";
                ticketHistory.NewValue = db.Users.FirstOrDefault(u => u.Id == model.AssignToUserId).Fullname;
                ticketHistory.Created = DateTimeOffset.UtcNow;
                ticketHistory.AuthorId = User.Identity.GetUserId();
                db.TicketHistories.Add(ticketHistory);
                db.SaveChanges();

                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();


                // Notify Submitter that a Developer has been assigned.
                if (model.OwnerUserId != User.Identity.GetUserId())
                {
                    try
                    {
                        var body = "<p>{0}</p><p>{1}</p>";
                        var from = "BugTracker<jritchie.projects@gmail.com>";
                        var developer = db.Users.Find(model.AssignToUserId).Fullname;

                        var email = new MailMessage(from, db.Users.Find(model.OwnerUserId).Email)
                        {
                            Subject = "BugTracker Notification Email: New ticket assigned",
                            Body = string.Format(body, "Message from BugTracker:", "Your ticket: '" + model.Title + "', has been assigned to '" + developer + "' for resolution."),
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

                // Notify assigned Developer that they have a new Ticket.
                if (model.AssignToUserId != User.Identity.GetUserId())
                {
                    try
                    {
                        var body = "<p>{0}</p><p>{1}</p>";
                        var from = "BugTracker<jritchie.projects@gmail.com>";

                        var email = new MailMessage(from, db.Users.Find(model.AssignToUserId).Email)
                        {
                            Subject = "BugTracker Notification Email: New ticket assigned",
                            Body = string.Format(body, "Message from BugTracker:", "A new ticket: '" + model.Title + "' has been assigned to you. This ticket's priority level is: '" + db.TicketPriorities.FirstOrDefault(p => p.Id == model.TicketPriorityId).Name + "'."),
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



                return RedirectToAction("Index");
            }
            return View(model);
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
