namespace SeventWebApplication.Models.Vehicle
{
    public abstract class Vehicle
    {
        public Guid Id { get; private set; }
        public string RegNr { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public int HorsePower { get; set; }

        protected Vehicle(string regNr, string brand, string model, int year, int horsePower)
        {
            Id = Guid.NewGuid();
            RegNr = regNr;
            Brand = brand;
            Model = model;
            Year = year;
            HorsePower = horsePower;
        }

        // Behöver input från hur vi vill göra här Tyko
        public abstract decimal GetBasePrice();
        public abstract string GetDescription();

    }
}
