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
using System.IO;
using Microsoft.AspNet.Identity;

namespace jritchieBugTracker.Controllers
{
    public class TicketAttachmentsController : UniversalController
    {
        // GET: TicketAttachments
        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            //var ticketAttachments = db.TicketAttachments.Include(t => t.Author).Include(t => t.Ticket);
            //return View(ticketAttachments.ToList());

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
        }

        // GET: TicketAttachments/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }

            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Admin"))
            {
                return View(ticketAttachment);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }

        // GET: TicketAttachments/Create
        //[Authorize]
        //public ActionResult Create()
        //{
        //    ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName");
        //    ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title");
        //    return View();
        //}

        // POST: TicketAttachments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TicketId,Description,Created,AuthorId,FileUrl")] TicketAttachment ticketAttachment, HttpPostedFileBase attachment)
        {

            // Validate file.
            if (attachment != null && attachment.ContentLength > 0)                                         // Confirm file has data
            {
                var ext = Path.GetExtension(attachment.FileName).ToLower();
                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif" && ext != ".bmp" &&
                    ext != ".doc" && ext != ".docx" && ext != ".rtf" && ext != ".txt" && ext !=".pdf" &&
                    ext != ".xls" && ext != ".xlsx" && ext != ".ppt" && ext != ".pptx")
                {
                    ModelState.AddModelError("attachment", "Invalid Format.");                              // Validation message
                }

                // Test whether all properties were received.
                if (ModelState.IsValid)
                {
                    var filePath = "/FileAttachments/";                                 // FileUrl
                    var absPath = Server.MapPath("~" + filePath);                       // Physical file

                    ticketAttachment.FileUrl = filePath + attachment.FileName;          // Sets path of file in database
                    attachment.SaveAs(Path.Combine(absPath, attachment.FileName));      // Saves (adds) the physical file to the application

                    ticketAttachment.Created = DateTimeOffset.UtcNow;
                    ticketAttachment.AuthorId = db.Users.Find(User.Identity.GetUserId()).Id;

                    db.TicketAttachments.Add(ticketAttachment);
                    db.SaveChanges();
                    //return RedirectToAction("Index");
                    return RedirectToAction("Details", "Tickets", new { id = ticketAttachment.TicketId });
                }

                return RedirectToAction("Details", "Tickets", new { id = ticketAttachment.TicketId });
            }

            return RedirectToAction("Details", "Tickets", new { id = ticketAttachment.TicketId });

            //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.AuthorId);
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
        }

        // GET: TicketAttachments/Edit/5
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }

            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Admin"))
            {
                return View(ticketAttachment);
            }
            else if (User.IsInRole("ProjectManager") && !ticketAttachment.Ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (ticketAttachment.AuthorId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (user.Roles.Count == 0)
            {
                return View("NoTickets");
            }

            //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.AuthorId);
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            return View(ticketAttachment);
        }

        // POST: TicketAttachments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TicketId,Description,Created,AuthorId,FileUrl")] TicketAttachment ticketAttachment, string fileUrl, HttpPostedFileBase attachment)
        {
            // Validate file.
            if (attachment != null && attachment.ContentLength > 0)                                         // Confirm file has data
            {
                var ext = Path.GetExtension(attachment.FileName).ToLower();
                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg" && ext != ".gif" && ext != ".bmp" &&
                    ext != ".doc" && ext != ".docx" && ext != ".rtf" && ext != ".txt" && ext != ".pdf" &&
                    ext != ".xls" && ext != ".xlsx" && ext != ".ppt" && ext != ".pptx")
                {
                    ModelState.AddModelError("attachment", "Invalid Format.");                              // Validation message
                }

                if (ModelState.IsValid)
                {

                    if (attachment != null)
                    {
                        var filePath = "/FileAttachments/";                                   // FileUrl
                        var absPath = Server.MapPath("~" + filePath);                         // Physical file
                        ticketAttachment.FileUrl = filePath + attachment.FileName;            // Sets path of file in database
                        attachment.SaveAs(Path.Combine(absPath, attachment.FileName));        // Saves (adds) the physical file to the application
                    }
                    else
                    {
                        ticketAttachment.FileUrl = fileUrl;                                   // Sets file to prior file. 
                    }

                    db.Entry(ticketAttachment).State = EntityState.Modified;
                    db.SaveChanges();
                    //return RedirectToAction("Index");
                    return RedirectToAction("Details", "Tickets", new { id = ticketAttachment.TicketId });
                }
            }
            //ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketAttachment.AuthorId);
            //ViewBag.TicketId = new SelectList(db.Tickets, "Id", "Title", ticketAttachment.TicketId);
            return View(ticketAttachment);
        }

        // GET: TicketAttachments/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            if (ticketAttachment == null)
            {
                return HttpNotFound();
            }

            var user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Admin"))
            {
                return View(ticketAttachment);
            }
            else if (User.IsInRole("ProjectManager") && !ticketAttachment.Ticket.Project.Users.Any(u => u.Id == user.Id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (ticketAttachment.AuthorId != user.Id)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else if (user.Roles.Count == 0)
            {
                return View("NoTickets");
            }

            return RedirectToAction("Details", "Tickets", new { id = ticketAttachment.TicketId });
            //return View(ticketAttachment);
        }

        // POST: TicketAttachments/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            TicketAttachment ticketAttachment = db.TicketAttachments.Find(id);
            db.TicketAttachments.Remove(ticketAttachment);
            db.SaveChanges();
            //return RedirectToAction("Index");
            return RedirectToAction("Details", "Tickets", new { id = ticketAttachment.TicketId });
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
