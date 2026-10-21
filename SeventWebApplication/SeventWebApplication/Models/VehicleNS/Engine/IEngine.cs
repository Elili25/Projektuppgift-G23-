namespace SeventWebApplication.Models.VehicleNS.Engine;

public interface IEngine
{
    int HorsePower { get; }
    FuelType FuelType { get; }
}