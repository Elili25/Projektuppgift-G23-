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
            _fleetManager.VehicleRemoved += vehicle => _assignmentRepository.Unassign(vehicle.VehicleId);
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

        // Familjen bilen är kopplad till, eller null
        public VehicleFamily? GetFamilyOf(Guid vehicleId)
        {
            return _assignmentRepository.GetFamilyId(vehicleId) is Guid id ? _familyRepository.GetById(id) : null;
        }

        // Räknar om kopplingen för en bil (t.ex. efter redigering): släpper den gamla och kopplar om
        public bool ReassignFamily(Vehicle vehicle)
        {
            _assignmentRepository.Unassign(vehicle.VehicleId);
            return AutoAssignFamily(vehicle);
        }

        // Räknar om alla bilars koppling (t.ex. efter ändrade minimikrav på en familj)
        public void ReassignAllVehicles()
        {
            foreach (var vehicle in _fleetManager.GetAllVehicles())
                ReassignFamily(vehicle);
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
        // excluding = en bokning som inte ska räknas som väntande, t.ex. den som just ska godkännas
        public bool IsFamilyAvailable(Guid familyId, DateTime start, DateTime end, BookingEntity? excluding = null)
        {
            int freeVehicles = FindMatchingVehicles(familyId)
                .Count(v => _fleetManager.IsAvailable(v, start, end));

            return freeVehicles > CountWaitingFamilyBookings(familyId, start, end, excluding);
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

        // Bilar som personal kan välja mellan för en bokning: bilar ur bokningens familj, eller alla bilar om
        // bokningen gjorts på en specifik bil. Bara bilar som är i drift och lediga under perioden, och den bil
        // bokningen redan har (som annars skulle räknas som upptagen av just den här bokningen).
        public IEnumerable<Vehicle> GetAssignableVehicles(BookingEntity booking)
        {
            ArgumentNullException.ThrowIfNull(booking);

            IEnumerable<Vehicle> pool = booking.VehicleFamilyId is Guid familyId
                ? FindMatchingVehicles(familyId)
                : _fleetManager.GetAllVehicles();

            return pool.Where(v => CanBeAssigned(booking, v)).ToList();
        }

        public bool CanBeAssigned(BookingEntity booking, Vehicle vehicle)
        {
            ArgumentNullException.ThrowIfNull(booking);
            ArgumentNullException.ThrowIfNull(vehicle);

            if (!vehicle.IsOperational)
                return false;

            if (booking.VehicleFamilyId is Guid familyId
                && _assignmentRepository.GetFamilyId(vehicle.VehicleId) != familyId)
                return false;

            // Bokningens egen bil är redan reserverad av just den här bokningen
            return vehicle.VehicleId == booking.VehicleId
                || _fleetManager.IsAvailable(vehicle, booking.StartDate, booking.EndDate);
        }

        // --- Hjälpmetoder ---

        // Familjebokningar som ännu inte fått en bil och som överlappar perioden
        private int CountWaitingFamilyBookings(Guid familyId, DateTime start, DateTime end, BookingEntity? excluding = null)
        {
            return _bookingRepository.GetAll().Count(b =>
                b != excluding
                && b.VehicleFamilyId == familyId
                && b.Vehicle == null
                && b.Status != BookingStatus.Cancelled
                && b.StartDate < end
                && start < b.EndDate);
        }
    }
}