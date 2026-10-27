namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleFamilyRepository
    {
        private readonly Dictionary<Guid, VehicleFamily> _families = new();

        public void Add(VehicleFamily family)
        {
            ArgumentNullException.ThrowIfNull(family);

            if (!_families.TryAdd(family.FamilyId, family))
            {
                throw new InvalidOperationException(
                    $"En fordonsfamilj med ID {family.FamilyId} finns redan.");
            }
        }

        public IEnumerable<VehicleFamily> GetAll()
        {
            return _families.Values.ToList();
        }

        public VehicleFamily? GetById(Guid id)
        {
            return _families.TryGetValue(id, out var family)
                ? family
                : null;
        }
    }
}
