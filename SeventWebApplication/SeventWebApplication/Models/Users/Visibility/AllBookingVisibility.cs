using BookingModel = SeventWebApplication.Models.Bookings.Booking;

namespace SeventWebApplication.Models.Users
{
    // För personalen så ser de allt, så vi skickar bara tillbaka listan som den är.
    public class AllBookingVisibility : IBookingVisibility
    {
        public IEnumerable<BookingModel> Filter(IEnumerable<BookingModel> bookings)
        {
            return bookings;
        }
    }
}
