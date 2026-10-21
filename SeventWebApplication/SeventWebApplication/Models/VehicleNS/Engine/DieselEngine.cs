namespace SeventWebApplication.Models.VehicleNS.Engine;

public class DieselEngine : Engine
{
    public int HorsePower;
    public FuelType FuelType => FuelType.Diesel;
    public int FuelCapacityLiters { get;}

    public DieselEngine(int horsePower, int fuelCapacityLiters) : base(horsePower)
    { 
        HorsePower = horsePower;
        
        PositiveValueValidator(fuelCapacityLiters);
        FuelCapacityLiters = fuelCapacityLiters;
    }
}