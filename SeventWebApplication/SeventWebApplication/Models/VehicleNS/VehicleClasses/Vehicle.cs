using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.VehicleNS

{
    public abstract class Vehicle
    {
        public Guid Id { get; private set; }

        public RegistrationPlate RegistrationPlate { get; private set; }

        public VehicleBrand Brand { get; private set; }
        public string Model { get; private set; }
        public int Year { get; private set; }
        public int HorsePower { get; private set; }
        public int PassengerCapacity { get; private set; }
        public decimal LuggageCapacity { get; private set; }
        public bool IsAvailable { get; set; } = true;

        protected Vehicle(
            RegistrationPlate registrationPlate,
            VehicleBrand brand,
            string model,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity)
        {
            Id = Guid.NewGuid();

            RegistrationPlate = registrationPlate
                ?? throw new ArgumentNullException(
                    nameof(registrationPlate));

            Brand = brand
                ?? throw new ArgumentNullException(nameof(brand));

            Model = model;
            Year = year;
            HorsePower = horsePower;
            PassengerCapacity = passengerCapacity;
            LuggageCapacity = luggageCapacity;
        }
    }
}