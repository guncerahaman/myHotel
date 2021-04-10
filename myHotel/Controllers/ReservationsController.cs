using System;
using System.Collections.Generic;

using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using myHotel.Models;

namespace myHotel.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class ReservationsController : Controller
    {
        private Entities1 db = new Entities1();

        // GET: Reservations
        public ActionResult Index()
        {
            var reservation = db.Reservation.Include(r => r.Customer).Include(r => r.Room).Where(r=>r.checkout >= DateTime.Now);

            return View(reservation.ToList());
        }
        public ActionResult Incomplete()
        {
            var reservation = db.Reservation.Include(r => r.Customer).Include(r => r.Room).Where(r => r.Payment.Count() <1 );

            return View(reservation.ToList());
        }
        public ActionResult PastReservations()
        {
            var reservation = db.Reservation.Include(r => r.Customer).Include(r => r.Room).Where(r => r.checkout < DateTime.Now);

            return View(reservation.ToList());
        }
        // GET: Reservations/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservation.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // GET: Reservations/Create
        public ActionResult Create()
        {
            ViewBag.customerID = new SelectList(db.Customer, "customerID", "customerName");
            ViewBag.roomNumber = new SelectList(db.Room, "roomNumber", "roomType");
            return View();
        }

        // POST: Reservations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ReservationID,customerID,roomNumber,adult,children,checkin,checkout,resDate,breakfast,shuttle,status")] Reservation reservation)
        {
            if (ModelState.IsValid)
            {
                db.Reservation.Add(reservation);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.customerID = new SelectList(db.Customer, "customerID", "customerName", reservation.customerID);
            ViewBag.roomNumber = new SelectList(db.Room, "roomNumber", "roomType", reservation.roomNumber);
            return View(reservation);
        }

        // GET: Reservations/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservation.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            ViewBag.customerID = new SelectList(db.Customer, "customerID", "customerName", reservation.customerID);
            ViewBag.roomNumber = new SelectList(db.Room, "roomNumber", "roomType", reservation.roomNumber);
            return View(reservation);
        }

        // POST: Reservations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Reservation reservation)
        {
            if (ModelState.IsValid)
              
            {
                db.Entry(reservation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.customerID = new SelectList(db.Customer, "customerID", "customerName", reservation.customerID);
            ViewBag.roomNumber = new SelectList(db.Room, "roomNumber", "roomType", reservation.roomNumber);
            return View(reservation);
        }

        // GET: Reservations/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Reservation reservation = db.Reservation.Find(id);
            if (reservation == null)
            {
                return HttpNotFound();
            }
            return View(reservation);
        }

        // POST: Reservations/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
           
            Reservation reservation = db.Reservation.Find(id);
            db.Reservation.Remove(reservation);
            db.SaveChanges();
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
