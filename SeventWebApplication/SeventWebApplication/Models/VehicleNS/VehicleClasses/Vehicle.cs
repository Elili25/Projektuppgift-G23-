using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.VehicleNS

{
    public abstract class Vehicle
    {
        public Guid Id { get; private set; }
        public string RegNr { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int HorsePower { get; set; }
        public int PassengerCapacity { get; set; }
        public decimal LuggageCapacity { get; set; }
        public bool IsAvailable { get; set; } = true;
        public CountryCode CountryCode { get; set; }

        protected Vehicle(
            string regNr,
            string brand,
            string model,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity,
            CountryCode countryCode)
        {


            Id = Guid.NewGuid();
            RegNr = regNr;
            Brand = brand;
            Model = model;
            Year = year;
            HorsePower = horsePower;
            PassengerCapacity = passengerCapacity;
            LuggageCapacity = luggageCapacity;
            CountryCode = countryCode;
        }
    }
}