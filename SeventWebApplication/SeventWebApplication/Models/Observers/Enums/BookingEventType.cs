using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Observers
{
    // Vad som hände med bokningen
    public enum BookingEventType
    {
        Created,
        Confirmed,
        Cancelled,
        RentalStarted,
        RentalReturned,
        LateReturn
    }

    // Meddelandet som skickas till alla observers.
    // record = oföränderligt, så en observer kan inte ändra händelsen för de andra.
    public record BookingEvent(BookingEventType Type, BookingEntity Booking)
    {
        public DateTime OccurredAt { get; } = DateTime.Now;
    }
}