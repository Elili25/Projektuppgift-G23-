    namespace SeventWebApplication.Models.VehicleNs
    {
    public class ElectricCar : ElectricVehicleWrapper
    {
        private readonly Car _car;
        public Car Car => _car;

        public ElectricCar(Car car, int batteryCapacityKwh, int rangeKm)
            : base(car, batteryCapacityKwh, rangeKm)
        {
            _car = car;
        }
    }
}

