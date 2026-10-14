namespace SeventWebApplication.Models.Vehicle
{
    public class ElectricCar : IElectricVehicle
    {
        // Den wrappade bilen — delegera allt till den
        private readonly Car _car;

        // Elektriska egenskaper
        public int BatteryCapacityKwh { get; set; }
        public int RangeKm { get; set; }

        public ElectricCar(Car car, int batteryCapacityKwh, int rangeKm)
        {
            _car = car;
            BatteryCapacityKwh = batteryCapacityKwh;
            RangeKm = rangeKm;
        }

        // Delegera Vehicle-egenskaper till den wrappade bilen
        public string RegNr => _car.RegNr;
        public string Brand => _car.Brand;
        public string Model => _car.Model;
        public int Year => _car.Year;
        public bool IsAvailable => _car.IsAvailable;
        public int PassengerCapacity => _car.PassengerCapacity;
        public TransmissionType TransmissionType => _car.TransmissionType;

        // Elpris ersätter bensinbilens pris
        public decimal GetBasePrice() => _car.GetBasePrice() + 200m; // elprispåslag
    }
}
