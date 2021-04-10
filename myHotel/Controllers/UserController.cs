using Microsoft.AspNet.Identity;
using myHotel.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myHotel.Controllers
{
    [Authorize]

    public class UserController : Controller
    {
        private Entities1 db = new Entities1();
        // GET: User
        public ActionResult Index()
        {
            string userid = User.Identity.GetUserId();
            var customer = db.Customer.ToList().Where(c => c.UserID == userid).FirstOrDefault();
            var reservations = db.Reservation.ToList().Where(x=> x.customerID == customer.customerID).Where(r => r.checkout > DateTime.Now).OrderBy(d => d.checkin).Where(y => y.Payment.Count > 0);
            ViewBag.pastres = db.Reservation.ToList().Where(x => x.customerID == customer.customerID).Where(r => r.checkout < DateTime.Now).Where(y => y.Payment.Count>0);
            ViewBag.incomplete = db.Reservation.ToList().Where(a => a.customerID == customer.customerID).Where(x => x.Payment.Count == 0);
            return View(reservations);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index");

            }
            Reservation reservation = db.Reservation.Find(id);
            var payment = db.Payment.ToList().Where(a => a.ReservationID == reservation.ReservationID).FirstOrDefault();

            return View(payment);
        }

        public ActionResult Cancel(int? reservationID)
        {
            if (reservationID == null)
            {
                return RedirectToAction("Index");

            }
            ViewBag.resID = reservationID;
            return View();

        }
        [HttpPost]
        public ActionResult Cancel(Cancellation cancellation)
        {
            if (ModelState.IsValid)
            {
                cancellation.refunded = false;
                db.Cancellation.Add(cancellation);
                Reservation reservation = db.Reservation.Find(cancellation.reservationID);
                reservation.status = "Cancel Requested";
                db.Entry(reservation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View();

        }

        public ActionResult Account()
        {
            
            string userid = User.Identity.GetUserId();
            var customer = db.Customer.ToList().Where(c => c.UserID == userid).FirstOrDefault();

            return View(customer);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Account(Customer customer )
        {
            if (ModelState.IsValid)
            {customer.UserID = User.Identity.GetUserId();
                db.Entry(customer).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            return View();
        }
    }

}
