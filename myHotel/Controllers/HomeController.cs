using Microsoft.AspNet.Identity;
using myHotel.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace myHotel.Controllers
{
    public class HomeController : Controller
    {
        private Entities1 db = new Entities1();
        public ActionResult Index()
        {

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(DateTime? indate, DateTime? outdate, int? adults, int? children)
        {

            return RedirectToAction("SearchResults", new { indate = indate, outdate = outdate, adults = adults, children = children });
            
        }
        public ActionResult SearchResults(DateTime? indate, DateTime? outdate, int? adults, int? children)
        {
            if (!(indate == null) || !(outdate == null) || !(adults == null) || !(children == null))
            {
               
          
                ViewBag.message = "";
                ViewBag.adults = adults;
                ViewBag.children = children;
                ViewBag.indate = indate;
                ViewBag.outdate = outdate;
                Room doubles= db.Room.ToList().Where(x => x.Reservation.All(r => r.checkout <= indate || r.checkin >= outdate)).Where(x => x.roomType.Contains("Double")).LastOrDefault();
                Room twins = db.Room.ToList().Where(x => x.Reservation.All(r => r.checkout <= indate || r.checkin >= outdate)).Where(x => x.roomType.Contains("Twin")).LastOrDefault(); ;
                Room triples = db.Room.ToList().Where(x => x.Reservation.All(r => r.checkout <= indate || r.checkin >= outdate)).Where(x => x.roomType.Contains("Triple")).LastOrDefault();
                Room familys = db.Room.ToList().Where(x => x.Reservation.All(r => r.checkout <= indate || r.checkin >= outdate)).Where(x => x.roomType.Contains("Family")).LastOrDefault();
               


                if (adults == 1 && children ==0)
                {
                    ViewBag.doubles = doubles;

                }
               else if( adults == 2 && children == 0 || (adults == 1 && children <= 1))
                {
                    ViewBag.doubles = doubles;
                    ViewBag.twins = twins;
                }
                else if ((adults ==3 && children ==0)|| (adults == 2 && children == 1) || (adults == 1 && children == 2))
                {
                    ViewBag.triples = triples;
                }
                if(children > 0)
                {
                    ViewBag.familys = familys;
                }
                Booking booking = new Booking();
                booking.indate = indate; ;
                booking.outdate = outdate; ;
                booking.adults = adults;
                booking.children = children;
               Session["booking"]=booking;
                return View();

            }
            return RedirectToAction("Index");
        }

        public ActionResult Book(int? roomID)
        {
           

            if (roomID == null)
            {
                return RedirectToAction("Index");
            }
            else
            {
                Booking booking = (Booking)Session["booking"];
                booking.roomID = roomID;
                Session["booking"] = booking;
                if (Request.IsAuthenticated)
                {
                   string userid = User.Identity.GetUserId();
           var customercheck = 
           (from c in db.Customer
           where (c.UserID == userid)
           select c).Count();
                    if (customercheck >=1)
                    {
                        ViewBag.preuser = true;
                        ViewBag.customer = db.Customer.ToList().Where(c => c.UserID == userid).Last();

                    }
                }
            }
          
            return View();
        }
        [HttpPost]

        
            public ActionResult Book(Models.Customer customer)
            {
                if (ModelState.IsValid)
                {
                    db.Customer.Add(customer);
                    db.SaveChanges();
                
                    return RedirectToAction("ContinueToBook", new { customerId = customer.customerID} );
                }
                return View();
            }

        public ActionResult ContinueToBook(int? CustomerID)
        {

         
           if  (CustomerID == null)
            {

                return RedirectToAction("Index");
            }
            Booking booking = (Booking)Session["booking"];
            booking.CustomerID = CustomerID;
            Session["booking"] = booking;
            ViewBag.customer = db.Customer.Find(CustomerID);
            ViewBag.room = db.Room.Find(booking.roomID);
            ViewBag.booking = booking;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ContinueToBook(Reservation reservation)
        {


            if (ModelState.IsValid)
            {
                Booking booking = (Booking)Session["booking"];
                reservation.customerID = Convert.ToInt32(booking.CustomerID);
                reservation.resDate = DateTime.Now;
                reservation.roomNumber = Convert.ToInt32(booking.roomID);
                reservation.checkin = Convert.ToDateTime(booking.indate);
                reservation.checkout = booking.outdate;
                reservation.adult = Convert.ToInt32(booking.adults);
                reservation.children = Convert.ToInt32(booking.children);
                db.Reservation.Add(reservation);
                db.SaveChanges();
                
                
                return RedirectToAction("Payment", new { reservationID = reservation.ReservationID});
            }

                 return View();

        }

        public ActionResult Payment(int? reservationID )
        {
     

            if (reservationID == null)
            {
                return RedirectToAction("ReservationError");
            }
            else if(db.Reservation.Find(reservationID).Payment.Count>0)
            {
                return RedirectToAction("Index", "User");
            }
            else
            {
              
                Reservation currentres = db.Reservation.Find(reservationID);
                Room currentroom = db.Room.Find(currentres.roomNumber);
                double totalpay = (Convert.ToDateTime(currentres.checkout) - currentres.checkin).Days  * Convert.ToDouble(currentroom.price);
              
                
                if (currentres.breakfast == true )
                {
                    totalpay += 5.99;

                }
                if (currentres.shuttle == true)
                {
                    totalpay += 22.99;
                }
                if (Session["booking"]==null)
                {
                    Booking booking = new Booking();
                    booking.totalpay = totalpay;
                    booking.CustomerID = currentres.customerID;
                    booking.indate = currentres.checkin;
                    booking.outdate = currentres.checkout;
                    Session["booking"] = booking;
                    ViewBag.booking = booking;

                    ViewBag.customer = db.Customer.Find(booking.CustomerID);
                }
                else
                {
               Booking booking = (Booking)Session["booking"];  
                booking.totalpay = totalpay;
                Session["booking"] = booking;      
                ViewBag.booking = booking;
                ViewBag.customer = db.Customer.Find(booking.CustomerID);
                }              
         
                return View();
            }

        
       }

        [HttpPost]


        public ActionResult Payment(Payment payment)
        {
            
            if (ModelState.IsValid)
            {
                
                Booking booking = (Booking)Session["booking"];
                payment.paymentDate = DateTime.Now;
                payment.paymentType = "offline";
               payment.totalPaid = booking.totalpay;
              payment.customerID = booking.CustomerID;
                db.Payment.Add(payment);
                db.SaveChanges();
                return RedirectToAction("Confirmation", new { PaymentId = payment.PaymentID});
            }

            return View();
         
        }

        public ActionResult Confirmation(int? paymentID)
        {

            if (paymentID == null)
            {
                return RedirectToAction("PaymentError");
            }
            Payment payment = db.Payment.Find(paymentID);

            //create the mail message 
            MailMessage mail = new MailMessage();

            //set the addresses 
            mail.From = new MailAddress("your email address", "MyHotel Bookings"); //IMPORTANT: This must be same as your smtp authentication address.
            mail.To.Add(payment.Customer.Email);
            mail.Bcc.Add(new MailAddress("BCC address"));
            //set the content 
            mail.Subject = "Your booking";
            mail.Body = "Thanks for your booking." + "\n" + "Your check in date: " + payment.Reservation.checkin.ToShortDateString() + " your checkout date: " + Convert.ToDateTime(payment.Reservation.checkout).ToShortDateString() + "\n Total to pay: " + payment.totalPaid + "\n You will pay at the hotel. Please bring cash or card with you, you will be required to pay prior to check in. ";

            //send the message 
            SmtpClient smtp = new SmtpClient("your mail server");

            //IMPORANT:  Your smtp login email MUST be same as your FROM address. 
            NetworkCredential Credentials = new NetworkCredential("your email address", "your password");
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = Credentials;
            smtp.Port = 25;    //alternative port number is 8889
            smtp.EnableSsl = false;
            smtp.Send(mail); 

            ViewBag.message2 = "Thank you. A confirmation email has been sent to " + payment.Customer.Email + " You will pay at the hotel. Please bring cash or card with you, you will be required to pay prior to check in. ";


            return View();
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

           

  
            return View();
        }

        public ActionResult Contact(string result)
        {
            if (result!=null)
            {
                ViewBag.msg = result;
            }

            return View();
        }

        [HttpPost]
        public ActionResult Contact(messages messages)
        {

            if (ModelState.IsValid)
            {
                messages.date = DateTime.UtcNow;
                db.messages.Add(messages);
                db.SaveChanges();

                return RedirectToAction("Contact", new { result = "success" });
            }

            return View();
        }
        public ActionResult Features()
        {

            return View();
        }
        public ActionResult Gallery()
        {
            IEnumerable<string> generic = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/generic"))
                                .Select(fn => "~/images/gallery/generic/" + Path.GetFileName(fn));
          IEnumerable<string> family = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/family"))
                                       .Select(fn => "~/images/gallery/family/" + Path.GetFileName(fn));
            IEnumerable<string> triple = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/triple"))
                                          .Select(fn => "~/images/gallery/triple/" + Path.GetFileName(fn));
            IEnumerable<string> double_ = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/double_"))
                                          .Select(fn => "~/images/gallery/double_/" + Path.GetFileName(fn));
            IEnumerable<string> twin = Directory.EnumerateFiles(Server.MapPath("~/images/gallery/twin"))
                                          .Select(fn => "~/images/gallery/twin/" + Path.GetFileName(fn));

          ViewBag.imagelist=  generic.Concat(family).Concat(triple).Concat(double_).Concat(twin);
            return View();
        }

        public ActionResult CustomerRegister()
        {



            return View();
        }
      


        [Authorize]
        public ActionResult CustomerLogin()
        {

            Booking booking = (Booking)Session["booking"];
            return  RedirectToAction("Book", new {roomId = booking.roomID});
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