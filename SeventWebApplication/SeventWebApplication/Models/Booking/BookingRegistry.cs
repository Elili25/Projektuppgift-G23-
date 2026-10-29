using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.Booking
{
    // BookingRegistry håller koll på bokningarna.
    // Allt här är flyttat från FleetManager som det var, så att FleetManager bara har fordonen.
    // Själva lagringen/listan sköts av BookingRepository, så det finns bara EN lista med bokningar i hela appen.
    // (Heter BookingRegistry istället för BookingManager för att inte krocka med rollinterfacet IBookingManager som
    // kommer hantera behörigheten för användare som får hantera bokningar, samt en framtida bookingmanagementservice osv.
    // AKA detta kändes mer separat från bokningshantering, registry kändes som det passade bra.)
    public class BookingRegistry
    {
        private static BookingRegistry? _instance = null;

        public static BookingRegistry instance
        {
            // Checks if the instance is not null and returns it or creates a new one if it is null
            get { return _instance ??= new BookingRegistry(); }
        }

        private readonly BookingRepository _bookingRepository = new();

        // Samma repository-objekt ges till Dependency Injection i Program.cs, så att tjänster som tar in
        // BookingRepository (RentalReturnService, VehicleDamageService) ser samma bokningar
        public BookingRepository Repository => _bookingRepository;

        // Private constructor so only one instance can be created (using the instance getter)
        private BookingRegistry() { }

        public void RegisterBooking(Booking booking)// vill nog ha några andra parametrar?
        {
            // TODO: Implementera när booking är klar

            _bookingRepository.Add(booking); //Temp
        }

        public IEnumerable<Booking> GetBookings()
        {
            return _bookingRepository.GetAll();
        }
    }
}
