namespace SeventWebApplication.Models.Observers
{
    public class BookingLogObserver : IBookingObserver
    {
        public void OnBookingEvent(BookingEvent e)
        {
            Console.WriteLine($"[{e.OccurredAt:HH:mm:ss}] Booking {e.Booking.BookingId}: {e.Type}");
        }
    }
}
