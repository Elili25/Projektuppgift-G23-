using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Users
{
    // För personalen så ser de allt, så vi skickar bara tillbaka listan som den är.
    public class AllBookingVisibility : IBookingVisibility
    {
        public IEnumerable<BookingEntity> Filter(IEnumerable<BookingEntity> bookings)
        {
            return bookings;
        }
    }
}
