namespace SeventWebApplication.Models.Observers
{
    // Observer i observer-mönstret: allt som vill reagera när något händer med en bokning.
    // Prenumererar via IBookingNotifier.Subscribe och anropas sedan för varje händelse.
    public interface IBookingObserver
    {
        void OnBookingEvent(BookingEvent bookingEvent);
    }
}