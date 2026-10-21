namespace SeventWebApplication.Models.VehicleNS.Engine;

public class ElectricEngine : Engine
{
    public int HorsePower { get; }
    public FuelType FuelType => FuelType.Electric;
    public int BatteryCapacityKwh { get;}
    public int RangeKm { get;}

    public ElectricEngine(int horsePower, int batteryCapacityKwh, int rangeKm) : base(horsePower)
    {
        HorsePower = horsePower;
        
        PositiveValueValidator(batteryCapacityKwh);
        BatteryCapacityKwh = batteryCapacityKwh;
        
        PositiveValueValidator(rangeKm);
        RangeKm = rangeKm;
    }
}