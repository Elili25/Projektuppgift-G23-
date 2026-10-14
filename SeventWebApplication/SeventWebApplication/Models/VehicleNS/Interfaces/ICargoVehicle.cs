namespace SeventWebApplication.Models.VehicleNs
{
    public interface ICargoVehicle
    {
        double CargoCapacityM3 { get; set; }
        double MaxPayloadKg { get; set; }
    }
}
