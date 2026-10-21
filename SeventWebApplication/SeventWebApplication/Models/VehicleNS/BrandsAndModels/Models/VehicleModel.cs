namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleModel : IVehicleModel
    {
        public Guid ModelId { get; private set; } = Guid.NewGuid();
        public string ModelName { get; private set; }
        public List<VehicleType> SupportedTypes { get; private set; } = new();

        public VehicleModel(string name, params VehicleType[] types)
        {
            ModelName = name;
            SupportedTypes = types.ToList();
        }
    }
}
