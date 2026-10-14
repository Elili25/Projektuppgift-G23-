namespace SeventWebApplication.Models.Vehicle
{
    public interface IVehicleFactory
    {
        Vehicle CreateStandard(
            string regNr,
            string brand,
            string model,
            int year,
            int horsePower,
            FuelType fuelType
        );

        Vehicle CreateElectric(
            Vehicle baseVehicle,
            int batteryCapacityKwh,
            int rangeKm
        );
    }
}
