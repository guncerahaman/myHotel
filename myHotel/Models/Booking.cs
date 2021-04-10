using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myHotel.Models
{
    public class Booking
    {
        public Nullable<int> CustomerID { get; set; }
        public Nullable<int> roomID  { get; set; }
        public Nullable<int> adults  { get; set; }
        public Nullable<int>  children { get; set; }
       
        public Nullable<DateTime> indate { get; set; }
        public Nullable<DateTime> outdate { get; set; }

        public double totalpay { get; set; }

    }
}