namespace SeventWebApplication.Models.VehicleNS
{
    public class Car : Vehicle, IPassengerVehicle, IFuelType, IVehicleTransmissionType
    {
        public FuelType FuelType { get; set; }
        public TransmissionType TransmissionType { get; set; }

        public Car(
            string regNr,
            string brand,
            string model,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity,
            FuelType fuelType,
            TransmissionType transmissionType,
            CountryCode countryCode)
            : base(regNr, brand, model, year, horsePower, passengerCapacity, luggageCapacity, countryCode)
        {
            FuelType = fuelType;
            TransmissionType = transmissionType;
        }
    }
}
