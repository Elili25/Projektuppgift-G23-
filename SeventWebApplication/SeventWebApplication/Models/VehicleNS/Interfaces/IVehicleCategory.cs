namespace SeventWebApplication.Models.VehicleNs
{
    public interface IVehicleCategory
    {
        string name { get; }
        public bool Matches (Vehicle vehicle);
    }
}
