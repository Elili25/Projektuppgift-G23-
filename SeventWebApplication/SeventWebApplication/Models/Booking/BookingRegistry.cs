namespace SeventWebApplication.Models.Booking
{
    // BookingRegistry håller koll på bokningarna.
    // Allt här är flyttat från FleetManager, så att FleetManager bara har fordonen.
    // Själva lagringen sköts av BookingRepository, så det finns bara EN lista med bokningar i hela appen.
    // (Heter BookingRegistry istället för BookingManager för att inte krocka med rollinterfacet IBookingManager,
    // som hanterar behörighet för användare som får hantera bokningar, samt en framtida BookingManagementService.)
    public class BookingRegistry
    {
        // Lazy<T> gör att instansen skapas första gången den används, och är trådsäker
        private static readonly Lazy<BookingRegistry> _instance =
            new(() => new BookingRegistry());

        public static BookingRegistry Instance => _instance.Value;

        private readonly BookingRepository _bookingRepository = new();

        // Samma repository-objekt ges till Dependency Injection i Program.cs, så att tjänster som tar in
        // BookingRepository (RentalReturnService, VehicleDamageService) ser samma bokningar
        public BookingRepository Repository => _bookingRepository;

        // Privat konstruktor så att bara en instans kan skapas (via Instance)
        private BookingRegistry() { }

        // --- Skriva ---

        public void RegisterBooking(Booking booking)
        {
            if (booking == null)
                throw new ArgumentNullException(nameof(booking));

            if (booking.Vehicle == null)
                throw new ArgumentException("Booking must have a vehicle.", nameof(booking));

            if (booking.EndDate <= booking.StartDate)
                throw new ArgumentException("End date must be after start date.", nameof(booking));

            if (IsVehicleBooked(booking.Vehicle.VehicleId, booking.StartDate, booking.EndDate))
                throw new InvalidOperationException("Vehicle is already booked for this period.");

            _bookingRepository.Add(booking);   // Add kastar själv fel om id:t redan finns
        }

        // --- Läsa (skickar vidare till repot) ---

        public IEnumerable<Booking> GetBookings()
            => _bookingRepository.GetAll();

        public Booking? GetBookingById(string bookingId)
            => _bookingRepository.GetById(bookingId);

        public IEnumerable<Booking> GetBookingsByUserId(string userId)
            => _bookingRepository.GetByUserId(userId);

        public IEnumerable<Booking> GetBookingsByVehicleId(Guid vehicleId)
            => _bookingRepository.GetByVehicleId(vehicleId);

        public IEnumerable<Booking> GetBookingsByStatus(string status)
            => _bookingRepository.GetByStatus(status);

        // --- Hjälpmetoder ---

        // Kollar om bilen redan har en aktiv bokning som överlappar perioden
        private bool IsVehicleBooked(Guid vehicleId, DateTime start, DateTime end)
        {
            return _bookingRepository.GetByVehicleId(vehicleId).Any(b =>
                b.Status != "Cancelled"
                && b.Status != "Completed"
                && b.StartDate < end
                && start < b.EndDate);
        }
    }
}