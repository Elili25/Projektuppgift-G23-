using System;
using System.Collections.Generic;


namespace SeventWebApplication.Models.Booking.Request

{

    public record BookingRequest
    {

        public Guid UserId { get; init; },
      //public int VehicleCategoryId { get; init; },
      // Vi har inte designat hur vi ska koppla till VehicleCategoryType, priceClass, transmissionType, fuelType, etc.
        public DateTime StartDate { get; init; },
        public DateTime EndDate { get; init; },
        public List<AddonRequest>? Addons { get; init; },
        public string? Notes { get; init; }

    }
    
}



