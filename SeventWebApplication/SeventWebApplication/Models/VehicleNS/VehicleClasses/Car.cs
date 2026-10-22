namespace SeventWebApplication.Models.VehicleNS
{
    public class Car : Vehicle,
        IPassengerVehicle,
        IFuelType,
        IVehicleTransmissionType
    {

        public Car(
            RegistrationPlate registrationPlate,
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
                model,
                vehicleCategoryType,
                year,
                horsePower,
                passengerCapacity,
                luggageCapacity)
        {
            FuelType = fuelType;
            TransmissionType = transmissionType;
        }
    }
}