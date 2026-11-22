using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.BookingValidation
{
    public class BookingResult
    {
        public bool Success { get; set; }
        public BookingEntity? CreatedBooking { get; set; }
        public List<ValidationError> Errors { get; set; } = new();
    }
}
