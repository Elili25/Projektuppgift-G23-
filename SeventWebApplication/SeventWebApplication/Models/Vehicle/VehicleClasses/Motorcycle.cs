namespace SeventWebApplication.Models.Vehicle
{
    public class Motorcycle : Vehicle, IVehicleWithEngine
    {
        public int EngineDisplacementCc { get; set; }
        public string FuelType { get; set; }

        public Motorcycle(string regNr, string brand, string model, int year, int horsePower,
                          int engineDisplacementCc, string fuelType)
            : base(regNr, brand, model, year, horsePower)
        {
            EngineDisplacementCc = engineDisplacementCc;
            FuelType = fuelType;
        }

        public override decimal GetBasePrice() => 350m;

        public override string GetDescription() =>
            $"{Brand} {Model} ({Year}) - {EngineDisplacementCc}cc - {FuelType}";
    }
}
