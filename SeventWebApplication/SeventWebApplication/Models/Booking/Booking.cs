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
        //public Guid BookingId { get; private set; } = Guid.NewGuid();
        // creates a unique identifier for each booking instance
        //public vehicle vehicle { get; set; } 

        
        public string BookingId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } 
        public Vehicle Vehicle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IAddon Addon { get; set; }
        public IPricingStrategy PricingStrategy { get; set; }
        // Status BookingStatus
        // Notes 
        // ApprovedBy


        // Add booking constructor 
        public Booking(string bookingId, string userId, Vehicle vehicle, DateTime startDate, DateTime endDate, IAddon addon, IPricingStrategy pricingStrategy)
        {
            BookingId = bookingId;
            UserId = userId;
            Vehicle = vehicle;
            StartDate = startDate;
            EndDate = endDate;
            Addon = addon;
            PricingStrategy = pricingStrategy;
        }

// Clone() method
}
}