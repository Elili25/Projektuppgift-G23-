namespace SeventWebApplication.Models.VehicleNS
{
    public interface IVehicleCategory
    {
        string name { get; }
        public bool Matches (Vehicle vehicle);
    }
}
