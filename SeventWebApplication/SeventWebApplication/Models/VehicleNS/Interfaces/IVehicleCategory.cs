namespace SeventWebApplication.Models.VehicleNS
{
    public interface IVehicleCategory
    {
        public string Name { get; }
        public bool Matches (Vehicle vehicle);
    }
}
