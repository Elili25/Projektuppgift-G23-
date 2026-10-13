namespace SeventWebApplication.Models.Vehicle
{
    public class ElectricCar : Car, IElectricVehicle
    {
        public int BatteryCapacityKwh { get; set; }
        public int RangeKm { get; set; }

        public ElectricCar
            (
            string regNr,
            string brand,
            string model,
            int year,
            int horsePower,
            int passengerCapacity,
            int batteryCapacityKwh,
            int rangeKm
            )
            : base(regNr, brand, model, year, horsePower, passengerCapacity,
                   FuelType.Electric, TransmissionType.Automatic)  // ← enums istället för strings
        {
            BatteryCapacityKwh = batteryCapacityKwh;
            RangeKm = rangeKm;
        }


        //fixa här nedan senare detta är placeholder för nu
        public override decimal GetBasePrice() => 700m;

        public override string GetDescription() =>
            $"{Brand} {Model} ({Year}) - Electric - {BatteryCapacityKwh}kWh - Range: {RangeKm}km";
    }
}
