namespace SeventWebApplication.Models.VehicleNS
{
    public class YearValidator : IYearValidator
    {
        public void ValidateYear(int year)
        {
            if (year > DateTime.Now.Year)
                throw new ArgumentOutOfRangeException(nameof(year), "År kan inte vara i framtiden");

            if (year < 1886)
                throw new ArgumentOutOfRangeException(nameof(year), "År kan inte vara tidigare än 1886");
        }
    }
}
