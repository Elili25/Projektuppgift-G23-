namespace SeventWebApplication.Models.VehicleNS
{
    public abstract class ElectricVehicleWrapper : Vehicle, IElectricVehicle
    {
        public int BatteryCapacityKwh { get; set; }
        public int RangeKm { get; set; }
        public double BatteryLevelPct { get; set; }

        protected ElectricVehicleWrapper(
            Vehicle vehicle,
            int kwh,
            int rangeKm)
            : base(
                vehicle.RegistrationPlate,
                vehicle.Model,
                vehicle.Year,
                vehicle.HorsePower,
                vehicle.PassengerCapacity,
                vehicle.LuggageCapacity)
        {
            BatteryCapacityKwh = kwh;
            RangeKm = rangeKm;
        }
    }
}