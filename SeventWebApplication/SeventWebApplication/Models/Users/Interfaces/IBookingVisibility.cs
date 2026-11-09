using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Users
{
    // Strategy-interfacet tar in alla bokningar och visar endast de man får se.
    // Aliaset behövs eftersom både namespace och klassen heter Booking.
    public interface IBookingVisibility
    {
        IEnumerable<BookingEntity> Filter(IEnumerable<BookingEntity> bookings);
    }
}
