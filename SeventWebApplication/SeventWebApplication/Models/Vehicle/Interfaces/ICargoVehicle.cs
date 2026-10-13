namespace SeventWebApplication.Models.Vehicle
{
    public interface ICargoVehicle
    {
        double CargoCapacityM3 { get; set; }
        double MaxPayloadKg { get; set; }
    }
}
