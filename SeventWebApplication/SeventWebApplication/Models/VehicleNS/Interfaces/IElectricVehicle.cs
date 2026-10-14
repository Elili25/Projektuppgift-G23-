namespace SeventWebApplication.Models.VehicleNs
{
    public interface IElectricVehicle
    {
        int BatteryCapacityKwh { get; set; }
        int RangeKm { get; set; }
    }
}
