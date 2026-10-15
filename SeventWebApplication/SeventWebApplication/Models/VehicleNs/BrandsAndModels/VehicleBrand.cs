namespace SeventWebApplication.Models.VehicleNs
{
    public class VehicleBrand : IVehicleBrand
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string Name { get; private set; }
        public List<VehicleType> SupportedTypes { get; private set; } = new();

        public VehicleBrand(string name, params VehicleType[] types)
        {
            Name = name;
            SupportedTypes = types.ToList();
        }
    }
}
