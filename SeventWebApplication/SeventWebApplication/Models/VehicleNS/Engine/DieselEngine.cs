namespace SeventWebApplication.Models.VehicleNS.Engine;

public class DieselEngine : Engine
{
    public override FuelType FuelType => FuelType.Diesel;
    public double FuelCapacityLiters { get;}

    public DieselEngine(int horsePower, double fuelCapacityLiters) : base(horsePower)
    { 
        PositiveValueValidator(fuelCapacityLiters);
        FuelCapacityLiters = fuelCapacityLiters;
    }
}