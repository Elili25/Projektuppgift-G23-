
namespace SeventWebApplication.Models.Bookings
{
    public class BookingRequest
    {
        // Unikt ID för användaren som försöker boka
        public string TargetUserId { get; set; }

        // ID för fordonet som ska bokas
        public string VehicleId { get; set; }

        // Start- och slutdatum för bokningen
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        // Lista med tillval (AddonRequests) som användaren vill lägga till
        public List<AddonRequest> Addons { get; set; } = new List<AddonRequest>();
    }
}
