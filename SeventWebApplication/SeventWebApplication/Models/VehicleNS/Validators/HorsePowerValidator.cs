namespace SeventWebApplication.Models.VehicleNS
{
    public class HorsePowerValidator : IHorsePowerValidator
    {
        public void ValidateHorsePower(int horsePower)
        {
            if (horsePower <= 0)
                throw new ArgumentOutOfRangeException(nameof(horsePower), "Horsepower must be greater than 0");
        }
    }
}
