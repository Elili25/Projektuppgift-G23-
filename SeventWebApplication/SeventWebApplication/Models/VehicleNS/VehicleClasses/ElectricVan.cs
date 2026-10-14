namespace SeventWebApplication.Models.VehicleNs
{
    public class ElectricVan : ElectricVehicleWrapper
    {
        private readonly Van _van;
        public Van Van => _van;

        public ElectricVan(Van van, int batteryCapacityKwh, int rangeKm)
            : base(van, batteryCapacityKwh, rangeKm)
        {
            _van = van;
        }

    }
}

