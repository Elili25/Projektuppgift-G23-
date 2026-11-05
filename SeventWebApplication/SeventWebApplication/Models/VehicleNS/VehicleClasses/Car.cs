using SeventWebApplication.Models.VehicleNS.Engine;

namespace SeventWebApplication.Models.VehicleNS
{
    // En personbil. Kategori (Sedan, SUV, ...) och komfort (Economy, Luxury, ...) avgör
    // både vilken fordonsfamilj bilen hamnar i (PassengerCarFamily.Matches)
    // och dagspriset (CarRateCatalog.GetDailyPrice).
    public class Car : Vehicle,
        IPassengerVehicle,
        IVehicleTransmissionType
    {
        public PassengerCarCategoryType PassengerCarCategoryType { get; private set; }

        // private set: om komforten ändras i efterhand stämmer bilens familjekoppling och pris inte längre
        public VehicleComfortType VehicleComfortType { get; private set; }

        public Car(
            RegistrationPlate registrationPlate,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            PassengerCarCategoryType passengerCarCategoryType,
            VehicleComfortType vehicleComfortType,
            int year,
            int passengerCapacity,
            decimal luggageCapacity,
            IEngine engine,
            TransmissionType transmissionType)
            : base(
                registrationPlate,
                model,
                vehicleCategoryType,
                year,
                passengerCapacity,
                luggageCapacity,
                engine,
                transmissionType)
        {
            if (vehicleCategoryType != VehicleCategoryType.PassengerCar)
                throw new ArgumentException("A car must have category PassengerCar.", nameof(vehicleCategoryType));

            if (passengerCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(passengerCapacity), "Passenger capacity must be greater than 0.");

            if (luggageCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(luggageCapacity), "Luggage capacity cannot be negative.");

            PassengerCarCategoryType = passengerCarCategoryType;
            VehicleComfortType = vehicleComfortType;
        }
    }
}