using SeventWebApplication.Models.Booking;
using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.VehicleNS
{
    /// <summary>
    /// Kopplar bilar till fordonsfamiljer, och tilldelar en konkret bil ur familjen till en bokning.
    /// </summary>
    public class VehicleAssignmentService
    {
        private readonly FleetManager _fleetManager;
        private readonly VehicleFamilyRepository _familyRepository;
        private readonly VehicleFamilyAssignmentRepository _assignmentRepository;
        private readonly BookingRepository _bookingRepository;

        public VehicleAssignmentService(
            FleetManager fleetManager,
            VehicleFamilyRepository familyRepository,
            VehicleFamilyAssignmentRepository assignmentRepository,
            BookingRepository bookingRepository)
        {
            _fleetManager = fleetManager;
            _familyRepository = familyRepository;
            _assignmentRepository = assignmentRepository;
            _bookingRepository = bookingRepository;

            // Varje ny bil i flottan kopplas automatiskt till en matchande familj
            _fleetManager.VehicleAdded += vehicle => AutoAssignFamily(vehicle);
        }

        // --- Bil ↔ familj ---

        // Kopplar en bil till första familjen den matchar
        public bool AutoAssignFamily(Vehicle vehicle)
        {
            ArgumentNullException.ThrowIfNull(vehicle);

            var family = _familyRepository.GetAll().FirstOrDefault(f => f.Matches(vehicle));
            if (family == null)
                return false;

            _assignmentRepository.Assign(vehicle.VehicleId, family.FamilyId);
            return true;
        }

        // Kopplar alla bilar som saknar familj. Behövs vid uppstart och när nya familjer skapas.
        // Returnerar antal bilar som kopplades.
        public int AutoAssignAllVehicles()
        {
            int assigned = 0;

            foreach (var vehicle in _fleetManager.GetAllVehicles())
            {
                if (_assignmentRepository.GetFamilyId(vehicle.VehicleId) == null && AutoAssignFamily(vehicle))
                    assigned++;
            }

            return assigned;
        }

        // Kopplar en bil till en specifik familj, men bara om bilen matchar
        public bool AssignVehicleToFamily(Guid vehicleId, Guid familyId)
        {
            var family = _familyRepository.GetById(familyId);
            var vehicle = _fleetManager.GetVehicle(vehicleId);

            if (family == null || vehicle == null || !family.Matches(vehicle))
                return false;

            _assignmentRepository.Assign(vehicleId, familyId);
            return true;
        }

        // Bilarna som är kopplade till familjen. En bil kopplas bara om den matchade,
        // så Matches behöver inte kollas igen.
        public IEnumerable<Vehicle> FindMatchingVehicles(Guid familyId)
        {
            return _fleetManager.GetAllVehicles()
                .Where(vehicle => _assignmentRepository.GetFamilyId(vehicle.VehicleId) == familyId)
                .ToList();
        }

        // --- Familj ↔ bokning ---

        // Ledig om det finns fler lediga bilar än familjebokningar som väntar på bil under perioden.
        // Förenkling: alla överlappande bokningar räknas som samtidiga, så den kan svara nej i onödan
        // men aldrig ja när det faktiskt är fullt.
        public bool IsFamilyAvailable(Guid familyId, DateTime start, DateTime end)
        {
            int freeVehicles = FindMatchingVehicles(familyId)
                .Count(v => _fleetManager.IsAvailable(v, start, end));

            return freeVehicles > CountWaitingFamilyBookings(familyId, start, end);
        }

        // Väljer en ledig bil ur bokningens familj och sätter den på bokningen
        public bool ConfirmVehicleAssignment(BookingEntity booking)
        {
            ArgumentNullException.ThrowIfNull(booking);

            // Bokningen måste gälla en familj och får inte redan ha en bil
            if (booking.VehicleFamilyId is not Guid familyId || booking.Vehicle != null)
                return false;

            var vehicle = FindMatchingVehicles(familyId)
                .FirstOrDefault(v => _fleetManager.IsAvailable(v, booking.StartDate, booking.EndDate));

            if (vehicle == null)
                return false;

            booking.AssignVehicle(vehicle);   // sätter både Vehicle och VehicleId
            return true;
        }

        // --- Hjälpmetoder ---

        // Familjebokningar som ännu inte fått en bil och som överlappar perioden
        private int CountWaitingFamilyBookings(Guid familyId, DateTime start, DateTime end)
        {
            return _bookingRepository.GetAll().Count(b =>
                b.VehicleFamilyId == familyId
                && b.Vehicle == null
                && b.Status != BookingStatus.Cancelled
                && b.StartDate < end
                && start < b.EndDate);
        }
    }
}