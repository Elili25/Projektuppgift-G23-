using BookingEntity = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.VehicleNS
{
    /// <summary>
    /// The VehicleAssignmentService class is responsible for managing the assignment of vehicles to vehicle families
    /// and for assigning a concrete vehicle from a family to a booking.
    /// </summary>
    public class VehicleAssignmentService
    {
        private readonly FleetManager _fleetManager;
        private readonly VehicleFamilyRepository _familyRepository;
        private readonly VehicleFamilyAssignmentRepository _assignmentRepository;

        public VehicleAssignmentService(
            FleetManager fleetManager,
            VehicleFamilyRepository familyRepository,
            VehicleFamilyAssignmentRepository assignmentRepository)
        {
            _fleetManager = fleetManager;
            _familyRepository = familyRepository;
            _assignmentRepository = assignmentRepository;

            // Varje ny bil i flottan kopplas automatiskt till en matchande familj
            _fleetManager.VehicleAdded += vehicle => AutoAssignFamily(vehicle);
        }

        // Mittenkolumnen: koppla en ny bil till första familjen den matchar
        public bool AutoAssignFamily(Vehicle vehicle)
        {
            ArgumentNullException.ThrowIfNull(vehicle);

            var family = _familyRepository.GetAll().FirstOrDefault(f => f.Matches(vehicle));
            if (family == null)
                return false;

            _assignmentRepository.Assign(vehicle.VehicleId, family.FamilyId);
            return true;
        }

        /// <summary>
        /// Kopplar alla bilar i flottan som saknar familj till en matchande familj.
        /// Behövs vid uppstart (bilar som lades till före familjerna) och när nya familjer skapas.
        /// Returnerar antal bilar som kopplades.
        /// </summary>
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

        /// <summary>
        /// Returns the vehicles assigned to the family. A vehicle is only assigned
        /// if it matched the family at assignment time, so Matches is not checked again.
        /// </summary>
        public IEnumerable<Vehicle> FindMatchingVehicles(Guid familyId)
        {
            return _fleetManager.GetAllVehicles()
                .Where(vehicle => _assignmentRepository.GetFamilyId(vehicle.VehicleId) == familyId)
                .ToList();
        }

        // Finns det minst en ledig bil i familjen under perioden?
        public bool IsFamilyAvailable(Guid familyId, DateTime start, DateTime end)
        {
            return FindMatchingVehicles(familyId)
                .Any(v => _fleetManager.IsAvailable(v, start, end));
        }

        // Välj en ledig bil ur bokningens familj och sätt den på bokningen
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

        // Kopplar en bil till en familj (var tidigare AssignVehicle(vehicleId, familyId))
        public bool AssignVehicleToFamily(Guid vehicleId, Guid familyId)
        {
            var family = _familyRepository.GetById(familyId);

            var vehicle = _fleetManager.GetVehicle(vehicleId);

            if (family == null || vehicle == null)
                return false;

            if (!family.Matches(vehicle))
                return false;

            _assignmentRepository.Assign(vehicleId, familyId);
            return true;
        }
    }
}
