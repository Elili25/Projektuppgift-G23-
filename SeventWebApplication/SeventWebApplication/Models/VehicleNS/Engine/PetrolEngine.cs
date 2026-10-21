namespace SeventWebApplication.Models.VehicleNS.Engine;

public class PetrolEngine : Engine
{
    public int HorsePower { get; }
    public FuelType FuelType => FuelType.Petrol;
    public double FuelCapacityLiters { get; }
    
    public PetrolEngine(int horsepower, double fuelCapacityLiters) : base(horsepower)
    {
        HorsePower = horsepower;
        
        PositiveValueValidator(fuelCapacityLiters);
        FuelCapacityLiters = fuelCapacityLiters;
    }
}