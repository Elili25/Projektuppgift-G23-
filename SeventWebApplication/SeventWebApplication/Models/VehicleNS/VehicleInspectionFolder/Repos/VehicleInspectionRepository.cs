namespace SeventWebApplication.Models.VehicleNS
{

        public class VehicleInspectionRepository
        {
            private readonly List<VehicleInspection> _inspections = new();

            public void Add(VehicleInspection inspection)
            {
                _inspections.Add(inspection);
            }

            public VehicleInspection? GetById(Guid id)
            {
                return _inspections.FirstOrDefault(
                    inspection => inspection.VehicleInspectionId == id);
            }

            public IEnumerable<VehicleInspection> GetByVehicleId(
                Guid vehicleId)
            {
                return _inspections
                    .Where(inspection => inspection.VehicleId == vehicleId)
                    .ToList();
            }

            public IEnumerable<VehicleInspection> GetAll()
            {
                return _inspections.ToList();
            }
        }
}

