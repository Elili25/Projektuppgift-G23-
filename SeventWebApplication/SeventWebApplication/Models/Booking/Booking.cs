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
		public Guid BookingId { get; private set; } = Guid.NewGuid();
        // creates a unique identifier for each booking instance
		public IVehicle Vehicle { get; set; }
		
		public string CustomerName { get; private set; }
		public string CustomerEmail { get; private set; }
		public string CustomerPhone { get; private set; }
        // customer info should be imported from customer class instead of being stored here

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal totalprice { get; private set; }

        // Addons 
		public List<IAddon> Addons { get; private set; } = new ();
   
// Add booking constructor 
public Booking(Ivehicle vehicle, string customerName, string customerEmail, string customerPhone, DateTime startDate, DateTime endDate, List<Addon>? addons = null)
{
	Vehicle = vehicle;
	CustomerName = customerName;
	CustomerEmail = customerEmail;
	CustomerPhone = customerPhone;
	StartDate = startDate;
	EndDate = endDate; 

	if (addons != null)
		Addons = addons

        CalculatePrice();
}

	}
}

/// Calculate price
private void CalculatePrice()
{

	decimal days = (decimal)(EndDate - StartDate).TotalDays;

    decimal basePrice = Vehicle.BasePricePerDay * days;	
    decimal addonsPrice = Addons.Sum(addon => addon.PricePerDay * days);
    TotalPrice = basePrice + addonsPrice;
}



// Status of the booking with Clone() method
#endif