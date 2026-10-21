namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleBrand : IVehicleBrand
    {
        public Guid BrandId { get; private set; } = Guid.NewGuid();
        public string BrandName { get; private set; }
        public List<VehicleType> SupportedTypes { get; private set; } = new();

        public VehicleBrand(string name, params VehicleType[] types)
        {
            BrandName = name;
            SupportedTypes = types.ToList();
        }
    }
}
