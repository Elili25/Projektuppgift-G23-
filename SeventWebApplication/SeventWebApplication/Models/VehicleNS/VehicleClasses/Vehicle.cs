using SeventWebApplication.Models.VehicleNS.Engine;

namespace SeventWebApplication.Models.VehicleNS
{
    public abstract class Vehicle
    {
        public Guid Id { get; private set; }

        public RegistrationPlate RegistrationPlate { get; private set; }

        public VehicleModel Model { get; private set; }

        public VehicleCategoryType VehicleCategoryType { get; private set; }

        public TransmissionType TransmissionType { get; set; }

        public int Year { get; private set; }
        public int PassengerCapacity { get; private set; }
        public decimal LuggageCapacity { get; private set; }

        public bool IsAvailable { get; set; } = true;

        protected Vehicle(
            RegistrationPlate registrationPlate,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity)
        {
            Id = Guid.NewGuid();

            RegistrationPlate = registrationPlate
                ?? throw new ArgumentNullException(nameof(registrationPlate));

            Model = model
                ?? throw new ArgumentNullException(nameof(model));

            VehicleCategoryType = vehicleCategoryType;

            Year = year;
            HorsePower = horsePower;
            PassengerCapacity = passengerCapacity;
            LuggageCapacity = luggageCapacity;
        }

        protected Vehicle(RegistrationPlate registrationPlate, VehicleModel model, int year, int horsePower, int passengerCapacity, decimal luggageCapacity)
        {
            RegistrationPlate = registrationPlate;
            Model = model;
            Year = year;
            HorsePower = horsePower;
            PassengerCapacity = passengerCapacity;
            LuggageCapacity = luggageCapacity;
        }
    }
}