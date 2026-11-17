namespace SeventWebApplication.Models.Observers
{
    // Implementationen av subject. Registreras som singleton i Program.cs,
    // så att alla tjänster delar samma lista med observers.
    public class BookingNotifier : IBookingNotifier
    {
        private readonly List<IBookingObserver> _observers = new();

        // Låset behövs eftersom flera webbrequests kan anropa Notify samtidigt
        private readonly object _lock = new();

        public void Subscribe(IBookingObserver observer)
        {
            ArgumentNullException.ThrowIfNull(observer);

            lock (_lock)
            {
                if (!_observers.Contains(observer))
                    _observers.Add(observer);
            }
        }

        public void Unsubscribe(IBookingObserver observer)
        {
            lock (_lock)
            {
                _observers.Remove(observer);
            }
        }

        public void Notify(BookingEvent bookingEvent)
        {
            ArgumentNullException.ThrowIfNull(bookingEvent);

            // Kopiera listan, så att en observer kan avprenumerera mitt i utan att loopen kraschar
            IBookingObserver[] observers;
            lock (_lock)
            {
                observers = _observers.ToArray();
            }

            foreach (var observer in observers)
            {
                // En observer som kraschar får inte stoppa bokningen eller de andra observerna
                try
                {
                    observer.OnBookingEvent(bookingEvent);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Observer {observer.GetType().Name} failed: {ex.Message}");
                }
            }
        }
    }
}