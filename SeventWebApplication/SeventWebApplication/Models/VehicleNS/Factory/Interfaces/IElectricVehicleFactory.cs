namespace SeventWebApplication.Models.VehicleNS
{
    public interface IElectricVehicleFactory : IVehicleFactory
    {
        Vehicle CreateElectric(Vehicle baseVehicle, int kwh, int rangeKm);
    }
}
