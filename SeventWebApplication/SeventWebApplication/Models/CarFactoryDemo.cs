using SeventWebApplication.Models.VehicleNs;

namespace SeventWebApplication.Models
{
    // CarFactoryDemo.cs
    namespace SeventWebApplication.Demo
    {
        public class CarFactoryDemo
        {
            private readonly CarFactory _carFactory = new CarFactory();
            private readonly List<Vehicle> _createdVehicles = new List<Vehicle>();

            public Car CreateCar(
                string regNr, string brand, string model,
                int year, int horsePower, FuelType fuelType,
                TransmissionType transmissionType, int passengerCapacity)
            {
                var car = (Car)_carFactory.CreateStandard(
                    regNr, brand, model, year, horsePower,
                    fuelType, transmissionType, passengerCapacity);

                _createdVehicles.Add(car);
                return car;
            }

            public ElectricCar CreateElectricCar(
                string regNr, string brand, string model,
                int year, int horsePower, FuelType fuelType,
                TransmissionType transmissionType, int passengerCapacity,
                int batteryCapacityKwh, int rangeKm)
            {
                var baseCar = _carFactory.CreateStandard(
                    regNr, brand, model, year, horsePower,
                    fuelType, transmissionType, passengerCapacity);

                var electricCar = (ElectricCar)_carFactory.CreateElectric(
                    baseCar, batteryCapacityKwh, rangeKm);

                _createdVehicles.Add(electricCar);
                return electricCar;
            }

            public IReadOnlyList<Vehicle> GetAllCreated() => _createdVehicles.AsReadOnly();
        }
    }
}
