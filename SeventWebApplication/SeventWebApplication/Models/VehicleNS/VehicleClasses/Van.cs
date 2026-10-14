namespace SeventWebApplication.Models.VehicleNs
{
    public class Van : Vehicle, IPassengerVehicle, IVehicleWithEngine, IVehicleTransmissionType, ICargoVehicle
    {
        public int PassengerCapacity { get; set; }
        public FuelType FuelType { get; set; }
        public TransmissionType TransmissionType { get; set; }
        public double CargoCapacityM3 { get; set; }
        public double MaxPayloadKg { get; set; }
        public Van
            (
            string regNr,
            string brand,
            string model,
            int year,
            int horsePower,
            int passengerCapacity,
            FuelType fuelType,
            TransmissionType transmissionType,
            double cargoCapacityM3,
            double maxPayloadKg
            )
            : base(regNr, brand, model, year, horsePower)
        {
            PassengerCapacity = passengerCapacity;
            FuelType = fuelType;
            TransmissionType = transmissionType;
            CargoCapacityM3 = cargoCapacityM3;
            MaxPayloadKg = maxPayloadKg;
        }


    }
}
