namespace SeventWebApplication.Models.VehicleNS
{
    public interface IElectricVehicle
    {
        int BatteryCapacityKwh { get; set; }
        int RangeKm { get; set; }
    }
}
