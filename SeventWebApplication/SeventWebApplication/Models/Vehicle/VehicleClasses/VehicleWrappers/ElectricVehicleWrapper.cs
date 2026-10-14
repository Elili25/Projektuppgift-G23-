namespace SeventWebApplication.Models.Vehicle
{
    public abstract class ElectricVehicleWrapper : IElectricVehicle
    {
        protected readonly Vehicle _vehicle;

        public int BatteryCapacityKwh { get; set; }
        public int RangeKm { get; set; }
        public double BatteryLevelPct { get; set; }

        // Gemensam delegation
        public string RegNr => _vehicle.RegNr;
        public string Brand => _vehicle.Brand;
        public string Model => _vehicle.Model;
        public bool IsAvailable => _vehicle.IsAvailable;

        protected ElectricVehicleWrapper(Vehicle vehicle, int kwh, int rangeKm)
        {
            _vehicle = vehicle;
            BatteryCapacityKwh = kwh;
            RangeKm = rangeKm;
        }

        public virtual decimal GetBasePrice() => _vehicle.GetBasePrice() + 200m;
    }
}
