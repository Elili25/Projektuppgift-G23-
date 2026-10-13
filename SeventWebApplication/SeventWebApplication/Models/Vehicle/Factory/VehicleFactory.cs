namespace SeventWebApplication.Models.Vehicle
{
    // GoF: Factory Method
    // SOLID: OCP – lägg till ny fabrik utan att ändra denna
    // SOLID: DIP – BookingService beror på abstraktion
    public abstract class VehicleFactory
    {
        public abstract Vehicle CreateVehicle(
            string regNr,
            string brand,
            string model,
            int year,
            int horsePower
        );
    }
}
