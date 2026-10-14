namespace SeventWebApplication.Models.Vehicle
{
    public class MotorcycleFactory : IVehicleFactory
    {
        public Vehicle CreateStandard(
            string regNr, 
            string brand, 
            string model,
            int year, 
            int horsePower,
            FuelType fuelType,
            int engineDisplacementCc
            )
        {
            return new Motorcycle(regNr, brand, model, year, horsePower, engineDisplacementCc, fuelType);
        }
    }
}
