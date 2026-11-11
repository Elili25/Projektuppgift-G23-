namespace SeventWebApplication.Models.Observers
{
    // Subject i observer-mönstret: håller koll på vilka som lyssnar och skickar händelser till dem.
    // Tjänsterna (BookingManagementService m.fl.) känner bara till det här interfacet,
    // inte vilka observers som finns.
    public interface IBookingNotifier
    {
        // Lägg till en observer som ska få alla framtida händelser
        void Subscribe(IBookingObserver observer);

        // Ta bort en observer så att den slutar få händelser
        void Unsubscribe(IBookingObserver observer);

        // Skicka en händelse till alla observers
        void Notify(BookingEvent bookingEvent);
    }
}