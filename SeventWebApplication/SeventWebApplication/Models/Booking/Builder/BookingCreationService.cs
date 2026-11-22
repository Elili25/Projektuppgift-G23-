using System;

namespace SeventWebApplication.Models.Booking
{

    public interface IBookingCreationService
    {
        Booking CreateBooking(IBookingCreator actor, BookingRequest req);
    }


}
