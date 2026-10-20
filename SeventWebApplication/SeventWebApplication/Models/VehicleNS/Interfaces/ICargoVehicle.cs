namespace SeventWebApplication.Models.VehicleNS
{
    public interface ICargoVehicle
    {
        double CargoCapacityM3 { get; set; }
        double MaxPayloadKg { get; set; }
    }
}
