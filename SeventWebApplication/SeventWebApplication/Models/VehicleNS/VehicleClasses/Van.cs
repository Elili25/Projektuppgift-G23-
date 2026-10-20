namespace SeventWebApplication.Models.VehicleNS
{
    public class Van : Vehicle, IPassengerVehicle, IFuelType, IVehicleTransmissionType, ICargoVehicle
    {
        public FuelType FuelType { get; set; }
        public TransmissionType TransmissionType { get; set; }
        public double CargoCapacityM3 { get; set; }
        public double MaxPayloadKg { get; set; }

        public Van(
            string regNr,
            string brand,
            string model,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity,
            FuelType fuelType,
            TransmissionType transmissionType,
            double cargoCapacityM3,
            double maxPayloadKg,
            CountryCode countryCode)
            : base(regNr, brand, model, year, horsePower, passengerCapacity, luggageCapacity, countryCode)
        {
            FuelType = fuelType;
            TransmissionType = transmissionType;
            CargoCapacityM3 = cargoCapacityM3;
            MaxPayloadKg = maxPayloadKg;
        }
    }
}
