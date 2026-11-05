using SeventWebApplication.Models.Bookings;

namespace SeventWebApplication.Models.BookingValidation
{
    public class BookingResult
    {
        // Visar om hela bokningsprocessen lyckades
        public bool Success { get; set; }

        // Håller det skapade bokningsobjektet (kan vara null om Success är false)
        public Booking? CreatedBooking { get; set; }

        // Innehåller eventuella valideringsfel (är tom om Success är true)
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
    }
}
