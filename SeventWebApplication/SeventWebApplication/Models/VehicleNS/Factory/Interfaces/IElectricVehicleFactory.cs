namespace SeventWebApplication.Models.VehicleNs
{
    public interface IElectricVehicleFactory : IVehicleFactory
    {
        Vehicle CreateElectric(Vehicle baseVehicle, int kwh, int rangeKm);
    }
}
