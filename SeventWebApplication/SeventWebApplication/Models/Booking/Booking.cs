using System;
using System.Collections.Generic;	
using System.Linq;
using SeventWebApplication.Models.VehicleNs;
using SeventWebApplication.Models.Addon;
#if false
using SeventWebApplication.Models.Booking.Enums;

namespace SeventWebApplication.Models.Booking
{
    public class Booking
    {
        //public Guid BookingId { get; private set; } = Guid.NewGuid();
        // creates a unique identifier for each booking instance
        //public vehicle vehicle { get; set; } 

        
        public string BookingId { get; set; }
        public string UserId { get; set; }
        public Vehicle Vehicle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IAddon Addon { get; set; } = new();
        public IPricingStrategy PricingStrategy { get; set; }


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
    }
}

public decimal GetTotalPrice()
{
    return PricingStrategy.CalculatePrice(this);
}

// Status of the booking with Clone() method
#endif