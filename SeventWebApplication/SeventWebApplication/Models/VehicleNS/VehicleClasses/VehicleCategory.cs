namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleCategory : IVehicleCategory
    {
        public Guid CategoryId { get; private set; }


        VehicleComfortType VehicleComfortType { get; set; }

        public string CategoryName => $"{VehicleComfortType}";

        public bool Matches(Vehicle vehicle)
        {
            // Här ska reglerna avgöra om fordonet
            // uppfyller kriterierna för kategorin.
            throw new NotImplementedException();
        }
    }
}