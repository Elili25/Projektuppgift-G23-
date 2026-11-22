using SeventWebApplication.Models.Observers;
using SeventWebApplication.Models.Payment;
using SeventWebApplication.Models.Pricing;
using SeventWebApplication.Models.Users;
using BookingEntity = SeventWebApplication.Models.Booking.Booking;
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
        private readonly FleetManager _fleetManager;
        private readonly PricingServices _pricing;

        public BookingManagementService(
            BookingRepository bookingRepository,
            VehicleAssignmentService assignmentService,
            RentalReturnService rentalReturnService,
            BookingNotifier bookingNotifier,
            PaymentServices payments,
            FleetManager fleetManager,
            PricingServices pricing)
        {
            _fleetManager = fleetManager;
            _pricing = pricing;
            _payments = payments;
            _bookingRepository = bookingRepository;
            _assignmentService = assignmentService;
            _rentalReturnService = rentalReturnService;
            _bookingNotifier = bookingNotifier;
        }

        // Hur långt före start en bil senast måste vara tilldelad. En familjebokning får vara utan bil
        // tills dess, sen tilldelas en automatiskt (AutoAssignDueVehicles).
        public static readonly TimeSpan VehicleAssignDeadline = TimeSpan.FromHours(2);

        // Pending → Confirmed. Personal väljer hur bilen hanteras:
        //   vehicleId angiven = tilldela just den bilen
        //   assignLater       = lämna familjebokningen utan bil tills VehicleAssignDeadline före start
        //   inget av dem      = välj automatiskt en ledig bil ur familjen (som förut)
        public void Approve(IBookingManager actor, string bookingId, Guid? vehicleId = null, bool assignLater = false)
        {
            EnsureActive(actor);
            var booking = GetBooking(bookingId);

            if (booking.Status != BookingStatus.Pending)
                throw new InvalidOperationException("Only pending bookings can be approved.");

            if (vehicleId is Guid chosen)
            {
                AssignSpecificVehicle(booking, chosen);
            }
            else if (assignLater)
            {
                if (booking.Vehicle == null)
                {
                    if (!CanLeaveUnassigned(booking, DateTime.Now))
                        throw new InvalidOperationException(
                            $"The booking starts within {VehicleAssignDeadline.TotalHours:0} hours, so a vehicle must be assigned now.");

                    // Samma kontroll som vid automatisk tilldelning, så att bokningen inte lovas utan att det finns plats
                    if (booking.VehicleFamilyId is Guid familyId
                        && !_assignmentService.IsFamilyAvailable(familyId, booking.StartDate, booking.EndDate, excluding: booking))
                        throw new InvalidOperationException("No available vehicle in the family for this period.");
                }
            }
            else if (booking.Vehicle == null && !_assignmentService.ConfirmVehicleAssignment(booking))
            {
                throw new InvalidOperationException("No available vehicle in the family for this period.");
            }

            booking.Status = BookingStatus.Confirmed;

            _bookingNotifier.Notify( new BookingEvent( BookingEventType.Confirmed, booking ) );
        }

        // Byter vilken bil som är tilldelad en Pending/Confirmed-bokning, eller tar bort den (vehicleId = null)
        // så att en ny kan väljas senare. Går inte när uthyrningen har startat.
        public void ChangeVehicle(IBookingManager actor, string bookingId, Guid? vehicleId)
        {
            EnsureActive(actor);
            var booking = GetBooking(bookingId);

            if (booking.Status is not (BookingStatus.Pending or BookingStatus.Confirmed))
                throw new InvalidOperationException("The vehicle can only be changed on pending or confirmed bookings.");

            if (vehicleId is Guid chosen)
            {
                AssignSpecificVehicle(booking, chosen);
                return;
            }

            // Ingen bil: bara en familjebokning kan vänta på en bil, och bara så länge det är mer än tidsgränsen kvar
            if (booking.VehicleFamilyId is null)
                throw new InvalidOperationException("A booking for a specific vehicle must keep a vehicle.");

            if (!CanLeaveUnassigned(booking, DateTime.Now))
                throw new InvalidOperationException(
                    $"The booking starts within {VehicleAssignDeadline.TotalHours:0} hours, so a vehicle must be assigned.");

            booking.ClearVehicle();
        }

        // Bilar som går att välja för bokningen just nu (i drift, lediga under perioden, rätt klass/pris)
        public IReadOnlyList<Vehicle> GetAssignableVehicles(BookingEntity booking)
        {
            return _assignmentService.GetAssignableVehicles(booking)
                .Where(v => HasSameRate(booking, v))
                .OrderBy(v => v.Model.Brand.BrandName).ThenBy(v => v.Model.ModelName).ThenBy(v => v.RegistrationPlate.RegNr)
                .ToList();
        }

        // Kan bokningen lämnas utan bil till tidsgränsen? Bara familjebokningar, och bara om det är för tidigt att tilldela.
        public static bool CanLeaveUnassigned(BookingEntity booking, DateTime now) =>
            booking.VehicleFamilyId is not null && now < booking.StartDate - VehicleAssignDeadline;

        // Bekräftade familjebokningar utan bil där det är dags (VehicleAssignDeadline före start) får en ledig bil.
        // Körs regelbundet av VehicleAutoAssignWorker. Returnerar hur många som fick bil.
        public int AutoAssignDueVehicles(DateTime now)
        {
            int assigned = 0;

            foreach (var booking in _bookingRepository.GetByStatus(BookingStatus.Confirmed)
                         .Where(b => b.Vehicle == null && b.VehicleFamilyId != null && now >= b.StartDate - VehicleAssignDeadline)
                         .OrderBy(b => b.StartDate)
                         .ToList())
            {
                if (_assignmentService.ConfirmVehicleAssignment(booking))
                    assigned++;
            }

            return assigned;
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

        // Tilldelar en vald bil, om den är i drift, ledig under perioden och av rätt sort för bokningen
        private void AssignSpecificVehicle(BookingEntity booking, Guid vehicleId)
        {
            var vehicle = _fleetManager.GetVehicle(vehicleId)
                ?? throw new InvalidOperationException("The vehicle does not exist.");

            if (booking.VehicleId == vehicleId)
                return;   // redan tilldelad

            if (!vehicle.IsOperational)
                throw new InvalidOperationException("The vehicle is out of service.");

            if (!HasSameRate(booking, vehicle))
                throw new InvalidOperationException("The vehicle must have the same daily rate as the booked one, otherwise the price changes.");

            if (!_assignmentService.CanBeAssigned(booking, vehicle))
                throw new InvalidOperationException("The vehicle is not available in this booking's vehicle type for the period.");

            booking.AssignVehicle(vehicle);
        }

        // Priset sattes när bokningen skapades. Byter man till en bil med annat dagspris stämmer det inte längre.
        // Familjebokningar har samma pris på alla bilar i familjen, så där behövs ingen kontroll.
        private bool HasSameRate(BookingEntity booking, Vehicle candidate)
        {
            if (booking.VehicleFamilyId is not null || booking.Vehicle is null)
                return true;

            return _pricing.CalculateBaseRate(candidate, 1) == _pricing.CalculateBaseRate(booking.Vehicle, 1);
        }

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