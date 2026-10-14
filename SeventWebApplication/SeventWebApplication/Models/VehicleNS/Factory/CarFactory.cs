namespace SeventWebApplication.Models.VehicleNs
{
    public class CarFactory : IElectricVehicleFactory
    {
        public Vehicle CreateStandard(
            string regNr, 
            string brand, 
            string model,
            int year, 
            int horsePower, 
            FuelType fuelType,
            TransmissionType transmissionType, 
            int passengerCapacity
            )
        {
            return new Car(regNr, brand, model, year, horsePower,
                passengerCapacity, fuelType, transmissionType);
        }

        public Vehicle CreateElectric(Vehicle baseVehicle, int batteryCapacityKwh, int rangeKm)
        {
            if (baseVehicle is not Car car)
                throw new ArgumentException("BaseVehicle måste vara en Car", nameof(baseVehicle));

            return new ElectricCar(car, batteryCapacityKwh, rangeKm);
        }
    }
}
