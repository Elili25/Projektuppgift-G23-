namespace SeventWebApplication.Models.Vehicle
{
    public class ElectricVan : IElectricVehicle
    {
        private readonly Van _van;

        // Elektriska egenskaper
        public int BatteryCapacityKwh { get; set; }
        public int RangeKm { get; set; }

        public ElectricVan(Van van, int batteryCapacityKwh, int rangeKm)
        {
            _van = van;
            BatteryCapacityKwh = batteryCapacityKwh;
            RangeKm = rangeKm;
        }

        // Delegera allt till _van
        public string RegNr => _van.RegNr;
        public string Brand => _van.Brand;
        public string Model => _van.Model;
        public int Year => _van.Year;
        public int HorsePower => _van.HorsePower;
        public bool IsAvailable => _van.IsAvailable;
        public int PassengerCapacity => _van.PassengerCapacity;
        public FuelType FuelType => _van.FuelType;
        public TransmissionType TransmissionType => _van.TransmissionType;
        public double CargoCapacityM3 => _van.CargoCapacityM3;
        public double MaxPayloadKg => _van.MaxPayloadKg;

        public decimal GetBasePrice() => _van.GetBasePrice() + 200m;

        public string GetDescription() =>
            $"{Brand} {Model} ({Year}) - Elektrisk - {TransmissionType} " +
            $"- Batteri: {BatteryCapacityKwh}kWh - Räckvidd: {RangeKm}km " +
            $"- Cargo: {CargoCapacityM3}m³ - Payload: {MaxPayloadKg}kg";
    }
}
