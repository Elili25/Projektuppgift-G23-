namespace SeventWebApplication.Models.Vehicle
{
    public class Car : Vehicle, IPassengerVehicle, IVehicleWithEngine, IVehicleTransmissionType
    {
        public int PassengerCapacity { get; set; }
        public string FuelType { get; set; }
        public string TransmissionType { get; set; }

        public Car(string regNr, string brand, string model, int year, int horsePower,
                   int passengerCapacity, string fuelType, string transmissionType)
            : base(regNr, brand, model, year, horsePower)
        {
            PassengerCapacity = passengerCapacity;
            FuelType = fuelType;
            TransmissionType = transmissionType;
        }

        public override decimal GetBasePrice() => 500m;

        public override string GetDescription() =>
            $"{Brand} {Model} ({Year}) - {FuelType} - {TransmissionType} - {PassengerCapacity} seats";
    }
}
