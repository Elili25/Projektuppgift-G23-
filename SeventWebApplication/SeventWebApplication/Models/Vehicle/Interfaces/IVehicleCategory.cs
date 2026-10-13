namespace SeventWebApplication.Models.Vehicle
{
    public interface IVehicleCategory
    {
        string name { get; }
        public bool Matches (Vehicle vehicle);
    }
}
