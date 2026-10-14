namespace SeventWebApplication.Models.VehicleNs
{
    public abstract class ElectricVehicleWrapper : Vehicle, IElectricVehicle
    {
        public int BatteryCapacityKwh { get; set; }
        public int RangeKm { get; set; }
        public double BatteryLevelPct { get; set; }

        protected ElectricVehicleWrapper(Vehicle vehicle, int kwh, int rangeKm)
            : base(vehicle.RegNr, vehicle.Brand, vehicle.Model, vehicle.Year, vehicle.HorsePower)
        {
            BatteryCapacityKwh = kwh;
            RangeKm = rangeKm;
        }

    }
}
