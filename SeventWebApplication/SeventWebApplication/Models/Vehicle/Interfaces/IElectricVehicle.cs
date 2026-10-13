namespace SeventWebApplication.Models.Vehicle
{
    public interface IElectricVehicle
    {
        int BatteryCapacityKwh { get; set; }
        int RangeKm { get; set; }
    }
}
