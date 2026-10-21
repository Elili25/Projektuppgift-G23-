namespace SeventWebApplication.Models.VehicleNS
{
    public class Car : Vehicle,
        IPassengerVehicle,
        IFuelType,
        IVehicleTransmissionType
    {
        public FuelType FuelType { get; set; }
        public TransmissionType TransmissionType { get; set; }

        public Car(
            RegistrationPlate registrationPlate,
            VehicleBrand brand,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity,
            FuelType fuelType,
            TransmissionType transmissionType)
            : base(
                registrationPlate,
                brand,
                model,
                year,
                horsePower,
                passengerCapacity,
                luggageCapacity)
        {
            VehicleCategoryType = vehicleCategoryType;
            FuelType = fuelType;
            TransmissionType = transmissionType;
        }
    }
}