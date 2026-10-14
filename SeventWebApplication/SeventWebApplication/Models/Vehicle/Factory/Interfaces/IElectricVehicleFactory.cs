namespace SeventWebApplication.Models.Vehicle
{
    public interface IElectricVehicleFactory : IVehicleFactory
    {
        Vehicle CreateElectric(Vehicle baseVehicle, int kwh, int rangeKm);
    }
}
