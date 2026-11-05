using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.Booking
{
    // Hanterar bokningens livscykel: godkänna, avboka, starta och avsluta uthyrning.
    // Metoderna tar IBookingManager, så bara Employee och Admin kan anropa dem.
    // En Customer går inte ens att skicka in (behörighet via typsystemet, ISP/DIP).
    public class BookingManagementService
    {
        private readonly BookingRepository _bookingRepository;
        private readonly VehicleAssignmentService _assignmentService;
        private readonly RentalReturnService _rentalReturnService;

        public BookingManagementService(
            BookingRepository bookingRepository,
            VehicleAssignmentService assignmentService,
            RentalReturnService rentalReturnService)
        {
            _bookingRepository = bookingRepository;
            _assignmentService = assignmentService;
            _rentalReturnService = rentalReturnService;
        }

        // Pending → Confirmed. Familjebokningar får en bil tilldelad här.
        public void Approve(IBookingManager actor, string bookingId)
        {
            EnsureActive(actor);
            var booking = GetBooking(bookingId);

            if (booking.Status != BookingStatus.Pending)
                throw new InvalidOperationException("Only pending bookings can be approved.");

            if (booking.Vehicle == null && !_assignmentService.ConfirmVehicleAssignment(booking))
                throw new InvalidOperationException("No available vehicle in the family for this period.");

            booking.Status = BookingStatus.Confirmed;
        }

        // Pending/Confirmed → Cancelled
        public void Cancel(IBookingManager actor, string bookingId)
        {
            EnsureActive(actor);
            var booking = GetBooking(bookingId);

            if (booking.Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
                throw new InvalidOperationException("Only pending or confirmed bookings can be cancelled.");

            booking.Status = BookingStatus.Cancelled;
        }

        // Confirmed → Active (pickup-inspektion, rental record och bilen spärras)
        public void StartRental(
            IBookingManager actor,
            string bookingId,
            int mileage,
            decimal fuelLevel,
            decimal batteryLevel,
            string notes = "")
        {
            EnsureActive(actor);
            _rentalReturnService.RegisterPickup(bookingId, mileage, fuelLevel, batteryLevel, notes);
        }

        // Active → Completed (retur-inspektion, skador, bilen släpps om den saknar skador).
        // Returnerar förseningsavgiften (0 om bilen kom i tid).
        public decimal CompleteRental(
            IBookingManager actor,
            string bookingId,
            int mileage,
            decimal? fuelLevel,
            decimal? batteryLevel,
            InspectionResult inspectionResult,
            IEnumerable<string>? damageDescriptions = null,
            string notes = "")
        {
            EnsureActive(actor);
            _rentalReturnService.RegisterReturn(
                bookingId, mileage, fuelLevel, batteryLevel,
                inspectionResult, damageDescriptions, notes);

            return _rentalReturnService.CalculateLateFee(bookingId);
        }

        // --- Hjälpmetoder ---

        private static void EnsureActive(IBookingManager actor)
        {
            ArgumentNullException.ThrowIfNull(actor);

            if (!actor.IsActive)
                throw new UnauthorizedAccessException("Deactivated accounts cannot manage bookings.");
        }

        private Booking GetBooking(string bookingId)
        {
            return _bookingRepository.GetById(bookingId)
                ?? throw new InvalidOperationException("Booking not found.");
        }
    }
}