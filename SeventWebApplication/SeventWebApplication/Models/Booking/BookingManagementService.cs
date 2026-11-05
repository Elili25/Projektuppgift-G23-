using SeventWebApplication.Models.Users;

namespace SeventWebApplication.Models.Booking
{
    public class BookingManagementService
    {
        private static void EnsureActive(IBookingManager actor)
        {
            ArgumentNullException.ThrowIfNull(actor);

            if (!actor.IsActive)
                throw new UnauthorizedAccessException("Deactivated accounts cannot manage bookings.");
        }
    }
}
