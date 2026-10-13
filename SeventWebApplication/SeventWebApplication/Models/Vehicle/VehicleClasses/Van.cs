namespace SeventWebApplication.Models.Vehicle
{
    public class Van : Vehicle, IPassengerVehicle, IVehicleWithEngine,
                                IVehicleTransmissionType, ICargoVehicle
    {
        public int PassengerCapacity { get; set; }
        public string FuelType { get; set; }
        public string TransmissionType { get; set; }
        public double CargoCapacityM3 { get; set; }
        public double MaxPayloadKg { get; set; }

        public Van(string regNr, string brand, string model, int year, int horsePower,
                   int passengerCapacity, string fuelType, string transmissionType,
                   double cargoCapacityM3, double maxPayloadKg)
            : base(regNr, brand, model, year, horsePower)
        {
            PassengerCapacity = passengerCapacity;
            FuelType = fuelType;
            TransmissionType = transmissionType;
            CargoCapacityM3 = cargoCapacityM3;
            MaxPayloadKg = maxPayloadKg;
        }

        public override decimal GetBasePrice() => 900m;

        public override string GetDescription() =>
            $"{Brand} {Model} ({Year}) - Cargo: {CargoCapacityM3}m³ - Payload: {MaxPayloadKg}kg";
    }
}
