namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleFamilyRepository
    {
        private readonly Dictionary<Guid, VehicleFamily> _families = new();


        // Adds a new vehicle family to the repository.

        public void Add(VehicleFamily family)
        {
            ArgumentNullException.ThrowIfNull(family);

            // Kombinationen typ + transmission + kategori (för bil även komfortnivå) måste vara unik,
            // annars blir det oklart vilken familj en bil hamnar i
            var duplicate = FindDuplicate(family);
            if (duplicate != null)
                throw new InvalidOperationException(
                    $"En familj med samma typ, transmission och kategori finns redan: \"{duplicate.Name}\".");

            // Check if a family with the same ID already exists in the repository
            if (!_families.TryAdd(family.FamilyId, family))
            {
                throw new InvalidOperationException(
                    $"A vehicle family with ID {family.FamilyId} already exists.");
            }
        }


        // Befintlig familj med samma unika kombination som den angivna, annars null
        public VehicleFamily? FindDuplicate(VehicleFamily family)
        {
            return _families.Values.FirstOrDefault(f => f.FamilyId != family.FamilyId && SameKey(f, family));
        }

        private static bool SameKey(VehicleFamily a, VehicleFamily b) => (a, b) switch
        {
            (PassengerCarFamily x, PassengerCarFamily y) =>
                x.TransmissionType == y.TransmissionType && x.CategoryType == y.CategoryType && x.ComfortType == y.ComfortType,
            (VanFamily x, VanFamily y) =>
                x.TransmissionType == y.TransmissionType && x.CategoryType == y.CategoryType,
            _ => false
        };

        // Removes a family. Returns false if it did not exist.
        public bool Remove(Guid id) => _families.Remove(id);

        // Returns all vehicle families in the repository.

        public IEnumerable<VehicleFamily> GetAll()
        {
            return _families.Values.ToList();
        }

        // Retrieves a vehicle family from the repository by its ID.  

        public VehicleFamily? GetById(Guid id)
        {
            return _families.TryGetValue(id, out var family)
                ? family
                : null;
        }
    }
}
