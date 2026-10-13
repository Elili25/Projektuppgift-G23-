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
        public bool IsAvailable { get; set; } = true;

        protected Vehicle(string regNr, string brand, string model, int year, int horsePower)
        {
            if (string.IsNullOrWhiteSpace(regNr))
                throw new ArgumentException("RegNr cannot be empty.");
            if (string.IsNullOrWhiteSpace(brand))
                throw new ArgumentException("Brand cannot be empty.");
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("Model cannot be empty.");
            if (year < 1900 || year > DateTime.Now.Year + 1)
                throw new ArgumentException("Invalid year.");
            if (horsePower <= 0)
                throw new ArgumentException("HorsePower must be positive.");

            Id = Guid.NewGuid();
            RegNr = regNr;
            Brand = brand;
            Model = model;
            Year = year;
            HorsePower = horsePower;
        }

        //fixa här nedan senare detta är placeholder för nu
        public abstract decimal GetBasePrice();
        public abstract string GetDescription();

    }
}
