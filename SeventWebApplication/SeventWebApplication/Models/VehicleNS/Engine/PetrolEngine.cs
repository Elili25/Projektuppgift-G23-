namespace SeventWebApplication.Models.VehicleNS.Engine;

public class PetrolEngine : Engine
{
    public override FuelType FuelType => FuelType.Petrol;
    public double FuelCapacityLiters { get; }
    
    public PetrolEngine(int horsepower, double fuelCapacityLiters) : base(horsepower)
    {
        PositiveValueValidator(fuelCapacityLiters);
        FuelCapacityLiters = fuelCapacityLiters;
    }
}