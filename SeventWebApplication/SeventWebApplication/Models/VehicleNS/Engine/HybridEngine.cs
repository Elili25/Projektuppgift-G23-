namespace SeventWebApplication.Models.VehicleNS.Engine;

public class HybridEngine : Engine
{
    public int HorsePower { get; }
    public FuelType FuelType => FuelType.Hybrid;
    public double FuelCapacityLiters { get; }
    public double BatteryCapacityKwh { get; }
    public int RangeKm { get; }

    public HybridEngine(int horsePower, int rangeKm, double fuelCapacityLiters, double batteryCapacityKwh) : base(horsePower)
    {
        HorsePower = horsePower;
        
        PositiveValueValidator(rangeKm);
        RangeKm = rangeKm;
        
        PositiveValueValidator(fuelCapacityLiters);
        FuelCapacityLiters = fuelCapacityLiters;
        
        PositiveValueValidator(batteryCapacityKwh);
        BatteryCapacityKwh = batteryCapacityKwh;
    }
}