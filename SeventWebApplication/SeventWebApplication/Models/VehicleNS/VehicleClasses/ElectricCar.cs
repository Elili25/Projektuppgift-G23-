    namespace SeventWebApplication.Models.VehicleNS
    {
    public class ElectricCar : ElectricVehicleWrapper
    {
        private readonly Car _car;
        public Car Car => _car;

        public ElectricCar(Car car, int batteryCapacityKwh, int rangeKm)
            : base(car, batteryCapacityKwh, rangeKm)
        {
            _car = car;
            _car.FuelType = FuelType.Electric;
        }
    }
}

