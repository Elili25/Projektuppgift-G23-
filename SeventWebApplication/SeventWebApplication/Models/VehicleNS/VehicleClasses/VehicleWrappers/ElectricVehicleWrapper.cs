using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.VehicleNS
{
    public abstract class ElectricVehicleWrapper : Vehicle, IElectricVehicle
    {
        public int BatteryCapacityKwh { get; set; }
        public int RangeKm { get; set; }
        public double BatteryLevelPct { get; set; }

        protected ElectricVehicleWrapper(Vehicle vehicle, int kwh, int rangeKm)
        : base(
         vehicle.RegNr,
         vehicle.Brand,
         vehicle.Model,
         vehicle.Year,
         vehicle.HorsePower,
         vehicle.PassengerCapacity,
         vehicle.LuggageCapacity,
         vehicle.CountryCode)
        {
            BatteryCapacityKwh = kwh;
            RangeKm = rangeKm;
        }

    }
}
