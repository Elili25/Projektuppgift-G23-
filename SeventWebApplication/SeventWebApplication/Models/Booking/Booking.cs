using System;
using System.Collections.Generic;	
using System.Linq;
using SeventWebApplication.Models.VehicleNS;
using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Addon.Interfaces;

namespace SeventWebApplication.Models.Booking
{
    public class Booking
    {
        
        public string BookingId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } 

        // Kunden kan boka en hel familj (VehicleFamilyId) utan att välja en specifik bil.
        // Själva bilen (VehicleId/Vehicle) är då null tills en bil tilldelas via AssignVehicle.
        public Guid? VehicleFamilyId { get; private set; }
        public Guid? VehicleId { get; private set; }
        public Vehicle? Vehicle { get; private set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AddonPackage Addon { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        //public IPricingStrategy PricingStrategy { get; set; }

        // Notes 
        // ApprovedBy


        // Add booking constructor 
        public Booking(string bookingId, string userId, Vehicle vehicle, DateTime startDate, DateTime endDate, AddonPackage addon) /* IPricingStrategy pricingStrateg*/
        {
            BookingId = bookingId;
            UserId = userId;
            AssignVehicle(vehicle);
            StartDate = startDate;
            EndDate = endDate;
            Addon = addon;
            //PricingStrategy = pricingStrategy;
        }

        // Bokning av en familj: ingen specifik bil är vald än
        public Booking(string bookingId, string userId, Guid vehicleFamilyId, DateTime startDate, DateTime endDate, AddonPackage addon)
        {
            BookingId = bookingId;
            UserId = userId;
            VehicleFamilyId = vehicleFamilyId;
            StartDate = startDate;
            EndDate = endDate;
            Addon = addon;
        }

        // Kopplar en konkret bil till bokningen (t.ex. när en bil väljs ur familjen)
        public void AssignVehicle(Vehicle vehicle)
        {
            Vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
            VehicleId = vehicle.VehicleId;
        }


        //public decimal GetTotalPrice()
        //{
        //    return PricingStrategy.CalculatePrice(this);
        //} 
        // utkommenterad då PricingStrategy inte är implementerad än

        // Status of the booking with Clone() method
    }
}