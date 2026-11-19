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
        public string UserId { get; set; } = string.Empty;

        // Kunden kan boka en hel familj (VehicleFamilyId) utan att välja en specifik bil.
        // Själva bilen (VehicleId/Vehicle) är då null tills en bil tilldelas via AssignVehicle.
        public Guid? VehicleFamilyId { get; private set; }
        public Guid? VehicleId { get; private set; }
        public Vehicle? Vehicle { get; private set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public AddonPackage Addon { get; set; } = new();
        public AddonPackage Addons
        {
            get => Addon;
            set => Addon = value;
        }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        //public IPricingStrategy PricingStrategy { get; set; }

        // Pengar. TotalPrice är hyrespriset (dagar + bokningsavgift + tillägg) och sätts när bokningen skapas.
        // LateFee och DamageFee fylls i när bilen lämnas tillbaka. Null/0 = inte beräknat/ingen avgift.
        public decimal? TotalPrice { get; set; }
        public decimal LateFee { get; set; }
        public decimal DamageFee { get; set; }

        // Betalning (sätts av PaymentServices). PaidAt null = inte betald än.
        public SeventWebApplication.Models.Payment.PaymentMethod? PaymentMethod { get; set; }
        public decimal PaidAmount { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? RefundedAt { get; set; }

        // Allt kunden ska betala: hyra + förseningsavgift + skadeavgift
        public decimal GrandTotal => (TotalPrice ?? 0m) + LateFee + DamageFee;

        // Valfri anteckning från kunden/personalen, t.ex. "hämtas kl 08"
        public string? Notes { get; set; }


        public Booking()
        {
        }

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


        // Pending -> Confirmed. Används när personal skapar en bokning (AutoApprove).
        public void Confirm()
        {
            if (Status != BookingStatus.Pending)
                throw new InvalidOperationException("Only pending bookings can be confirmed.");

            Status = BookingStatus.Confirmed;
        }

        
    }
}