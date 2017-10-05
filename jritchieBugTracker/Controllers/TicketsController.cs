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

            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Admin"))
            {
                return View(db.Tickets.ToList());
            }
            if (User.IsInRole("ProjectManager"))
            {
                return View(db.Tickets.Where(t => t.Project.Users.Any(u => u.Id == user.Id)).ToList());
            }
            if (User.IsInRole("Developer"))
            {
                return View(db.Tickets.Where(t => t.AssignToUserId == user.Id).ToList());
            }
            if (User.IsInRole("Submitter"))
            {
                return View(db.Tickets.Where(t => t.OwnerUserId == user.Id).ToList());
            }
            return View("NoTickets");

            //return RedirectToAction("Index", "Home");
            //return View();
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
            if (User.IsInRole("Admin"))
            {
                return View(ticket);
            }
            else if (User.IsInRole("Project Manager") && !ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Developer") && ticket.AssignToUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Submitter") && ticket.OwnerUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (user.Roles.Count == 0)
            {
                return View("NoTickets");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(ticket);
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

            ViewBag.AssignToUserId = new SelectList(db.Users, "Id", "FirstName", ticket.AssignToUserId);
            ViewBag.OwnerUserId = new SelectList(db.Users, "Id", "FirstName", ticket.OwnerUserId);
            ViewBag.ProjectId = new SelectList(db.Projects, "Id", "Title", ticket.ProjectId);
            ViewBag.TicketPriorityId = new SelectList(db.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewBag.TicketStatusId = new SelectList(db.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewBag.TicketTypeId = new SelectList(db.TicketTypes, "Id", "Name", ticket.TicketTypeId);

            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Admin"))
            {
                return View(ticket);
            }
            else if (User.IsInRole("Project Manager") && !ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Developer") && ticket.AssignToUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Submitter") && ticket.OwnerUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (user.Roles.Count == 0)
            {
                return View("NoTickets");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignToUserId")] Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.Updated = DateTimeOffset.UtcNow;

                db.Entry(ticket).State = EntityState.Modified;
                db.SaveChanges();
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
            if (User.IsInRole("Admin"))
            {
                return View(ticket);
            }
            else if (User.IsInRole("Project Manager") && !ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Developer") && ticket.AssignToUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (User.IsInRole("Submitter") && ticket.OwnerUserId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (user.Roles.Count == 0)
            {
                return View("NoTickets");
                //return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View(ticket);
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
        public ActionResult AssignTicketUser([Bind(Include = "Id,Title,Description,Created,Updated,ProjectId,TicketTypeId,TicketPriorityId,TicketStatusId,OwnerUserId,AssignToUserId")] Ticket model, string AssignToUserId)
        {
            if (ModelState.IsValid)
            {
                model.AssignToUserId = AssignToUserId;
                model.TicketStatusId = db.TicketStatuses.FirstOrDefault(t => t.Name == "Assigned").Id;


                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
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
