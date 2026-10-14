namespace SeventWebApplication.Models.VehicleNs
{
    public class Motorcycle : Vehicle, IVehicleWithEngine
    {
        public int EngineDisplacementCc { get; set; }
        public FuelType FuelType { get; set; }

        public Motorcycle
            (
            string regNr,
            string brand,
            string model,
            int year,
            int horsePower,
            int engineDisplacementCc,
            FuelType fuelType
            )
            : base(regNr, brand, model, year, horsePower)
        {
            EngineDisplacementCc = engineDisplacementCc;
            FuelType = fuelType;
        }


    }
}
