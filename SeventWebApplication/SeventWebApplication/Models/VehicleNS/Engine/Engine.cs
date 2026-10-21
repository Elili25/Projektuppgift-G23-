namespace SeventWebApplication.Models.VehicleNS.Engine;

public abstract class Engine : IEngine
{
    public FuelType FuelType { get; }  
    public int HorsePower { get; }

    protected Engine(int horsepower)
    {
        if (horsepower <= 0) throw new ArgumentOutOfRangeException(nameof(horsepower)); 
        HorsePower = horsepower;
    }

    protected void PositiveValueValidator(double value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
    }
}