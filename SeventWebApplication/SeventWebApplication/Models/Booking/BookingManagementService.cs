using SeventWebApplication.Models.Observers;
using SeventWebApplication.Models.Payment;
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
        private readonly BookingNotifier _bookingNotifier;
        private readonly PaymentServices _payments;

        public BookingManagementService(
            BookingRepository bookingRepository,
            VehicleAssignmentService assignmentService,
            RentalReturnService rentalReturnService,
            BookingNotifier bookingNotifier,
            PaymentServices payments)
        {
            _payments = payments;
            _bookingRepository = bookingRepository;
            _assignmentService = assignmentService;
            _rentalReturnService = rentalReturnService;
            _bookingNotifier = bookingNotifier;
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

            _bookingNotifier.Notify( new BookingEvent( BookingEventType.Confirmed, booking ) );
        }

        // Pending/Confirmed → Cancelled
        public void Cancel(IBookingManager actor, string bookingId)
        {
            EnsureActive(actor);
            var booking = GetBooking(bookingId);

            if (booking.Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
                throw new InvalidOperationException("Only pending or confirmed bookings can be cancelled.");

            MarkCancelled(booking);
        }

        // Kunden avbokar sin egen bokning (UC5). Måste vara ägaren, Pending/Confirmed
        // och mer än CustomerCancelWindow kvar till start.
        public void CancelOwn(IActor customer, string bookingId)
        {
            ArgumentNullException.ThrowIfNull(customer);

            if (!customer.IsActive)
                throw new UnauthorizedAccessException("Deactivated accounts cannot cancel bookings.");

            var booking = GetBooking(bookingId);

            if (!string.Equals(booking.UserId, customer.UserId.ToString(), StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("You can only cancel your own bookings.");

            if (booking.Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
                throw new InvalidOperationException("Only pending or confirmed bookings can be cancelled.");

            if (!IsWithinCancelWindow(booking, DateTime.Now))
                throw new InvalidOperationException(
                    $"Bookings can only be cancelled more than {CustomerCancelWindow.TotalHours:0} hours before the start.");

            MarkCancelled(booking);
        }

        // Hur långt före start en kund senast får avboka
        public static readonly TimeSpan CustomerCancelWindow = TimeSpan.FromHours(8);

        // Samma regel används av vyn för att bestämma om avboka-knappen visas
        public static bool CanCustomerCancel(Booking booking, DateTime now) =>
            booking.Status is BookingStatus.Pending or BookingStatus.Confirmed
            && IsWithinCancelWindow(booking, now);

        private static bool IsWithinCancelWindow(Booking booking, DateTime now) =>
            now < booking.StartDate - CustomerCancelWindow;

        private void MarkCancelled(Booking booking)
        {
            booking.Status = BookingStatus.Cancelled;

            // Det som är betalt betalas tillbaka
            _payments.RefundBooking(booking);

            _bookingNotifier.Notify( new BookingEvent( BookingEventType.Cancelled, booking ) );
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
            string notes = "",
            decimal damageFee = 0m,
            DateTime? returnedAt = null)
        {
            EnsureActive(actor);
            _rentalReturnService.RegisterReturn(
                bookingId, mileage, fuelLevel, batteryLevel,
                inspectionResult, damageDescriptions, notes, damageFee, returnedAt);

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