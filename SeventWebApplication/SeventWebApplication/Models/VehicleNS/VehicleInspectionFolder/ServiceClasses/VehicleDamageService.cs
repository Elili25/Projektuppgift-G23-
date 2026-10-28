namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleDamageService
    {
        private readonly FleetManager _fleetManager;
        private readonly VehicleInspectionRepository _inspectionRepository;
        private readonly VehicleDamageRepository _damageRepository;

        public VehicleDamageService(
            FleetManager fleetManager,
            VehicleInspectionRepository inspectionRepository,
            VehicleDamageRepository damageRepository)
        {
            _fleetManager = fleetManager;
            _inspectionRepository = inspectionRepository;
            _damageRepository = damageRepository;
        }

        public bool RegisterDamage(VehicleDamage damage)
        {
            if (damage == null || string.IsNullOrWhiteSpace(damage.Description))
                return false;

            var vehicle = _fleetManager.GetVehicle(damage.VehicleId);
            if (vehicle == null)
                return false;

            // Inspektion är valfri, men om den anges måste den finnas och gälla samma bil
            if (damage.InspectionId.HasValue)
            {
                var inspection = _inspectionRepository.GetById(damage.InspectionId.Value);

                if (inspection == null || inspection.VehicleId != damage.VehicleId)
                    return false;
            }

            _damageRepository.Add(damage);
            return true;
        }

        public IEnumerable<VehicleDamage> GetDamagesByVehicle(Guid vehicleId)
        {
            return _damageRepository.GetByVehicleId(vehicleId);
        }

        public IEnumerable<VehicleDamage> GetDamagesByInspection(Guid inspectionId)
        {
            return _damageRepository.GetByInspectionId(inspectionId);
        }

        public bool HasOpenDamages(Guid vehicleId)
        {
            return _damageRepository.GetByVehicleId(vehicleId)
                .Any(d => !d.IsResolved);
        }

        public bool ResolveDamage(Guid damageId)
        {
            var damage = _damageRepository.GetById(damageId);

            if (damage == null || damage.IsResolved)
                return false;

            damage.Resolve();
            return true;
        }
    }
}