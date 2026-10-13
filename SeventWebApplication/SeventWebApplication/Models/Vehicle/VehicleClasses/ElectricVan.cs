namespace SeventWebApplication.Models.Vehicle
{
    public class ElectricVan : Van, IElectricVehicle
    {
        public int BatteryCapacityKwh { get; set; }
        public int RangeKm { get; set; }

        public ElectricVan(string regNr, string brand, string model, int year, int horsePower,
                           int passengerCapacity, double cargoCapacityM3, double maxPayloadKg,
                           int batteryCapacityKwh, int rangeKm)
            : base(regNr, brand, model, year, horsePower, passengerCapacity,
                   "Electric", "Automatic", cargoCapacityM3, maxPayloadKg)
        {
            BatteryCapacityKwh = batteryCapacityKwh;
            RangeKm = rangeKm;
        }

        public override decimal GetBasePrice() => 1100m;

        public override string GetDescription() =>
            $"{Brand} {Model} ({Year}) - Electric Van - {BatteryCapacityKwh}kWh - Range: {RangeKm}km";
    }
}
