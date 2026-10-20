namespace SeventWebApplication.Models.VehicleNS
{
    public class VehicleCategory : IVehicleCategory
    {
        public Guid CategoryId { get; private set; }
        public string Name { get; private set; }
        public decimal DailyRate { get; private set; }

        public VehicleCategory(string name, decimal dailyRate)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException(nameof(name));

            if (dailyRate < 0)
                throw new ArgumentOutOfRangeException(nameof(dailyRate));

            CategoryId = Guid.NewGuid();
            Name = name;
            DailyRate = dailyRate;
        }

        public bool Matches(Vehicle vehicle)
        {
            // Här ska reglerna avgöra om fordonet
            // uppfyller kriterierna för kategorin.
            throw new NotImplementedException();
        }
    }
}