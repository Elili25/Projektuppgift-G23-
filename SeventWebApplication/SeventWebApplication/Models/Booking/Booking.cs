using System;
using System.Collections.Generic;	
using System.Linq;
using SeventWebApplication.Models.VehicleNs;
using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Booking.Enums;

namespace SeventWebApplication.Models.Booking
{
    public class Booking
    {

        
        public Guid UserId { get; set; }
        public Guid BookingId { get; set; }
        public int VehicleCategoryId { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string Notes { get; set; }


     
    }
}