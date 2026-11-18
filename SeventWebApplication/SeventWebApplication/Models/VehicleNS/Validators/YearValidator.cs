namespace SeventWebApplication.Models.VehicleNS
{
    public class YearValidator : IYearValidator
    {
        public void ValidateYear(int year)
        {
            if (year > DateTime.Now.Year)
                throw new ArgumentOutOfRangeException(nameof(year), "The year cannot be in the future");

            if (year < 1886)
                throw new ArgumentOutOfRangeException(nameof(year), "The year cannot be earlier than 1886");
        }
    }
}
