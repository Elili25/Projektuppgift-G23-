namespace SeventWebApplication.Models.VehicleNS.Engine;

public class DieselEngine : Engine
{
    public override FuelType FuelType => FuelType.Diesel;
    public int FuelCapacityLiters { get;}

    public DieselEngine(int horsePower, int fuelCapacityLiters) : base(horsePower)
    { 
        PositiveValueValidator(fuelCapacityLiters);
        FuelCapacityLiters = fuelCapacityLiters;
    }
}