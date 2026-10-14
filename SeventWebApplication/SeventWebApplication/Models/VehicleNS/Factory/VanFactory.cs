namespace SeventWebApplication.Models.VehicleNs
{
    public class VanFactory : IElectricVehicleFactory
    {
        public Vehicle CreateStandard(
            string regNr, 
            string brand, 
            string model,
            int year, 
            int horsePower, 
            FuelType fuelType,
            TransmissionType transmissionType, 
            int passengerCapacity,
            double cargoCapacityM3, 
            double maxPayloadKg
            )
        {
            return new Van(regNr, brand, model, year, horsePower,
                passengerCapacity, fuelType, transmissionType,
                cargoCapacityM3, maxPayloadKg);
        }

        public Vehicle CreateElectric(Vehicle baseVehicle, int batteryCapacityKwh, int rangeKm)
        {
            if (baseVehicle is not Van van)
                throw new ArgumentException("BaseVehicle måste vara en Van", nameof(baseVehicle));

            return new ElectricVan(van, batteryCapacityKwh, rangeKm);
        }
    }
}
