using System;
using System.Collections.Generic;	
using System.Linq;
using SeventWebApplication.Models.VehicleNs;
using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Bookings.Enums;

namespace SeventWebApplication.Models.Bookings
{
    public class Booking
    {

        
        public Guid UserId { get; init; }
        public Guid BookingId { get; init; }
        
        // public VehiclecategoryType VehicleCategoryType { get; set; }
        // Denna connectar ej då vi inte har designat hur vi ska koppla till VehicleCategoryType, priceClass, transmissionType, fuelType, etc.

        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public IReadOnlyList<Addon> Addons { get; init; } = new List<Addon>().AsReadOnly();
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        public string? Notes { get; init; }
        public required IPricingStrategy PricingStrategy { get; init; }
        public decimal GetTotalPrice() => PricingStrategy.CalculatePrice(this); 



    }
}