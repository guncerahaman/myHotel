using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using myHotel.Models;

namespace myHotel.Controllers
{

    //[Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
    
        // GET: Admin
        private Entities1 db = new Entities1();
 public ActionResult Index()
        {
            var Occupied =
                from room in db.Room
                join reservation in db.Reservation on room.roomNumber equals reservation.roomNumber
                where reservation.checkin < DateTime.Now && reservation.checkout > DateTime.Now
                select room;


            var rReservations = db.Reservation.ToList().Take(10);
            var Reservations = db.Reservation.ToList().Where(r => r.checkin >= DateTime.Now).OrderByDescending(r=>r.checkin).Take(10);
             var Cancellations = db.Cancellation.ToList().Where(r => r.refunded == false);
            ViewBag.Occupied = Occupied;
            ViewBag.rReservations = rReservations;
            ViewBag.Reservations = Reservations;
            ViewBag.Cancellations = Cancellations;
            ViewBag.OccupiedLength = Occupied.Count();
            ViewBag.CancellationsLength = Cancellations.Count();
            return View();
        }
        public   ActionResult Cancellations()
        {
            var cancellations = db.Cancellation.ToList();
            return View(cancellations);
        }
        public ActionResult Cancel(int id)
        {
            ViewBag.reservationid = id;

            return View();
        }
        [HttpPost]
        public ActionResult Cancel(Cancellation cancellation)
        {
            if (ModelState.IsValid)
            {
                db.Cancellation.Add(cancellation);
                Reservation reservation = db.Reservation.Find(cancellation.reservationID);
                if (cancellation.refunded == false)
                {
                    reservation.status = "Cancelled";
                }
                else if (cancellation.refunded == true)
                {
                    reservation.status = "Refunded";
                }
               
                db.Entry(reservation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Cancellations");
            }

            return View();
        }
        public ActionResult Refunded(int id)
        {
            Cancellation cancellation = db.Cancellation.Find(id);
            cancellation.refunded = true;
            Reservation reservation = db.Reservation.Find(cancellation.reservationID);
            reservation.status = "Refunded";
            db.Entry(cancellation).State = EntityState.Modified;
            db.Entry(reservation).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Cancellations");
        }
        public ActionResult CancelRequests()
        {
            var cancelrequests = db.Reservation.ToList().Where(r=>r.status == "Cancel Requested");
            return View(cancelrequests);
        }
        public ActionResult Accepted(int id)
        {
            Reservation reservation = db.Reservation.Find(id);
            reservation.status = "Cancellation Accepted";
            db.Entry(reservation).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("CancelRequests");

        }
        public ActionResult Rejected(int id)
        {
            Reservation reservation = db.Reservation.Find(id);
            reservation.status = "Cancellation Rejected";
            Cancellation cancellation = db.Cancellation.ToList().Where(a => a.reservationID == reservation.ReservationID).FirstOrDefault();
            db.Entry(reservation).State = EntityState.Modified;
            db.Entry(cancellation).State = EntityState.Deleted;
            db.SaveChanges();
            return RedirectToAction("CancelRequests");

        }
        public ActionResult Users()
        {


            return View();

        }
        public ActionResult NewReservation()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult NewReservation(Customer customer)
        {
            if (ModelState.IsValid)
            {
                db.Customer.Add(customer);
                db.SaveChanges();
                return RedirectToAction("ReservationForExisting", new { id = customer.customerID});
            }
            return View();
        }
    

        public ActionResult ReservationForExisting(int? id)
        {
            //Session for rooms 
            if ((id != null))
            {
             ViewBag.customername = db.Customer.Find(id).customerName;
            ViewBag.customerID = db.Customer.Find(id).customerID;
             Session["customerID"]= id;
            }
            else
            {
                if (Session["customerID"]!=null)
                {
                    ViewBag.customername = db.Customer.Find(Session["customerID"]).customerName;
                    ViewBag.customerID = Session["customerID"];
               
                }
            
                else
                {
 return RedirectToAction("NewReservation");
                }
                

            }
            if (Session["rooms"] != null)
            {
                List<Room> rooms = (List<Room>)Session["rooms"];
            ViewBag.roomNumber = new SelectList(rooms, "roomNumber", "roomType");
            }

            return View();
        }
        public ActionResult SearchRooms(DateTime checkin, DateTime checkout)
        {
            List<Room> rooms = new List<Room>();
            var roomlist = db.Room.ToList().Where(x => x.Reservation.All(r => r.checkout < checkin || r.checkin > checkout));
            foreach (var item in roomlist)
            {
                rooms.Add(item);
            }
            Session["rooms"] = rooms;
            Session["indate"] = checkin;
            Session["outdate"] = checkout;
            return RedirectToAction("ReservationForExisting");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReservationForExisting(Reservation reservation)
        {
           
          
            if (ModelState.IsValid)
            {
                reservation.resDate = DateTime.Now;
                reservation.checkin = (DateTime)Session["indate"];
                reservation.checkout = (DateTime)Session["outdate"];
                db.Reservation.Add(reservation);
                Payment payment = new Payment();
                payment.customerID = reservation.customerID;
                payment.paymentDate = reservation.resDate;
                payment.paymentType = "offline";
                payment.totalPaid = 0;
                db.Payment.Add(payment);
                db.SaveChanges();
                Session["indate"] = "";
                Session["outdate"] = "";
                return RedirectToAction("Index", "Reservations");
            }
  ViewBag.roomNumber = new SelectList(db.Room, "roomNumber", "roomType", reservation.roomNumber);
            return View();
          
        }
       public   ActionResult Photos()
        {
            ViewBag.generic = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/generic"))
                               .Select(fn => "~/images/gallery/generic/" + Path.GetFileName(fn));
            ViewBag.family = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/family"))
                                        .Select(fn => "~/images/gallery/family/" + Path.GetFileName(fn));
            ViewBag.triple = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/triple"))
                                          .Select(fn => "~/images/gallery/triple/" + Path.GetFileName(fn));
            ViewBag.double_ = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/double_"))
                                          .Select(fn => "~/images/gallery/double_/" + Path.GetFileName(fn));
            ViewBag.twin = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/twin"))
                                          .Select(fn => "~/images/gallery/twin/" + Path.GetFileName(fn));

            return View();
        }
        [HttpPost]
        public ActionResult Photos(string folder, HttpPostedFileBase ImageFile)
        {
            string imagename = Path.GetFileNameWithoutExtension(ImageFile.FileName);
            string extension = Path.GetExtension(ImageFile.FileName);
            imagename = imagename + extension;
            imagename = Path.Combine(Server.MapPath("~/images/gallery/"+folder+"/"), imagename);
            ImageFile.SaveAs(imagename);

            ViewBag.generic = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/generic"))
                                .Select(fn => "~/images/gallery/generic/" + Path.GetFileName(fn));
            ViewBag.family = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/family"))
                                        .Select(fn => "~/images/gallery/family/" + Path.GetFileName(fn));
            ViewBag.triple = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/triple"))
                                          .Select(fn => "~/images/gallery/triple/" + Path.GetFileName(fn));
            ViewBag.double_ = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/double_"))
                                          .Select(fn => "~/images/gallery/double_/" + Path.GetFileName(fn));
            ViewBag.twin = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/twin"))
                                          .Select(fn => "~/images/gallery/twin/" + Path.GetFileName(fn));

            return View();
        }

        [HttpPost]
        public ActionResult DeletePhoto(string image)
        {
            System.IO.File.Delete(Server.MapPath(image));
            return RedirectToAction("Photos");
        }
        public ActionResult Info()
        {
           ViewBag.htmltext= System.IO.File.ReadAllText(Server.MapPath("~/about.html"));
            return View();
        }
       [HttpPost, ValidateInput(false)]
        public ActionResult Info(string htmltext)
        {

        System.IO.File.WriteAllText(Server.MapPath("~/about.html"), htmltext);
            ViewBag.htmltext = System.IO.File.ReadAllText(Server.MapPath("~/about.html"));
            ViewBag.message = "Changes saved.";
            return View();
        }

        public ActionResult Features()
        {
            ViewBag.htmltext = System.IO.File.ReadAllText(Server.MapPath("~/features.html"));
            return View();

        }
        [HttpPost, ValidateInput(false)]
        public ActionResult Features(string htmltext)
        {

            System.IO.File.WriteAllText(Server.MapPath("~/features.html"), htmltext);
            ViewBag.htmltext = System.IO.File.ReadAllText(Server.MapPath("~/features.html"));
            ViewBag.message = "Changes saved.";
            return View();
        }

      public   ActionResult Messages()
        {
            
            return View(db.messages.ToList());
        }
        public ActionResult ReadMessage(int? id)
        {
     
            if (id!=null)
            {
             
 return View(db.messages.Find(id));
            }
            else
            {
                return RedirectToAction("Messages");
            }
            
           
        }
       
        public ActionResult DeleteMessage(int id)
        {
            messages message = db.messages.Find(id);
            db.Entry(message).State = EntityState.Deleted;
            db.SaveChanges();
            return RedirectToAction("Messages");

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