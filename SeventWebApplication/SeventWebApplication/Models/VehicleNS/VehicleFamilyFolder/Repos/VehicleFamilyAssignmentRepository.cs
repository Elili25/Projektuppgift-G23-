namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleFamilyAssignmentRepository
    {
        private readonly List<VehicleFamilyAssignment> _assignments = new();

        public void Assign(Guid vehicleId, Guid familyId)
        {
            var existing = _assignments
                .FirstOrDefault(a => a.VehicleId == vehicleId);

            if (existing != null)
            {
                _assignments.Remove(existing);
            }

            _assignments.Add(
                new VehicleFamilyAssignment(vehicleId, familyId));
        }

        public Guid? GetFamilyId(Guid vehicleId)
        {
            return _assignments
                .FirstOrDefault(a => a.VehicleId == vehicleId)
                ?.FamilyId;
        }

        public IEnumerable<VehicleFamilyAssignment> GetAll()
        {
            return _assignments.ToList();
        }
    }
}

