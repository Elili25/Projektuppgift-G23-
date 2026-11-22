namespace SeventWebApplication.Models.VehicleNS
{
    public interface ICargoVehicle
    {
        double CargoCapacityM3 { get; }
        double MaxPayloadKg { get; }
    }
}