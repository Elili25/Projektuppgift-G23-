using System;
using System.Collections.Generic;	
using System.Linq;
using SeventWebApplication.Models.VehicleNS;
using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Addon.Interfaces;

namespace SeventWebApplication.Models.Bookings
{
    public class Booking
    {
        
        public string BookingId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } 
        public Vehicle Vehicle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AddonPackage Addons { get; set; } = new AddonPackage();  // bytte ut IAddon till AddonPackage och tog bort från konstruktorn
        public string Status { get; set; } = "Pending"; // la till status så jag kudne n testa bokningar i olika status i viewern, pending e default
        //public IPricingStrategy PricingStrategy { get; set; }

        // Notes 
        // ApprovedBy


        // Add booking constructor 
        public Booking(string bookingId, string userId, Vehicle vehicle, DateTime startDate, DateTime endDate) /* IPricingStrategy pricingStrateg*/
        {
            BookingId = bookingId;
            UserId = userId;
            Vehicle = vehicle;
            StartDate = startDate;
            EndDate = endDate;
            //PricingStrategy = pricingStrategy;
        }


        //public decimal GetTotalPrice()
        //{
        //    return PricingStrategy.CalculatePrice(this);
        //} 
        // utkommenterad då PricingStrategy inte är implementerad än

        // Status of the booking with Clone() method
    }
}