using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Demo
{
    public class CarFactoryDemo
    {
        private readonly CarFactory _carFactory;
        private readonly List<Vehicle> _createdVehicles = new();

        public CarFactoryDemo()
        {
            var countryValidator = RegNrValidatorFactory.Create(CountryCode.SE);

            _carFactory = new CarFactory(
                countryValidator,
                new BrandModelValidator(),
                new YearValidator(),
                new HorsePowerValidator()
            );
        }

        public Car CreateCar(
            string regNr,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            PassengerCarCategoryType passengerCarCategoryType,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity,
            FuelType fuelType,
            TransmissionType transmissionType,
            CountryCode countryCode)
        {
            var car = (Car)_carFactory.CreateStandard(
                regNr,
                model,
                vehicleCategoryType,
                passengerCarCategoryType,
                year,
                horsePower,
                passengerCapacity,
                luggageCapacity,
                fuelType,
                transmissionType,
                countryCode);

            _createdVehicles.Add(car);
            return car;
        }

        public Car CreateElectricCar(
            string regNr,
            VehicleModel model,
            VehicleCategoryType vehicleCategoryType,
            PassengerCarCategoryType passengerCarCategoryType,
            int year,
            int horsePower,
            int passengerCapacity,
            decimal luggageCapacity,
            TransmissionType transmissionType,
            CountryCode countryCode,
            int batteryCapacityKwh,
            int rangeKm)
        {
            var electricCar = (Car)_carFactory.CreateElectric(
                regNr,
                model,
                vehicleCategoryType,
                passengerCarCategoryType,
                year,
                horsePower,
                passengerCapacity,
                luggageCapacity,
                batteryCapacityKwh,
                rangeKm,
                transmissionType,
                countryCode);

            _createdVehicles.Add(electricCar);
            return electricCar;
        }

        public IReadOnlyList<Vehicle> GetAllCreated()
            => _createdVehicles.AsReadOnly();
    }
}