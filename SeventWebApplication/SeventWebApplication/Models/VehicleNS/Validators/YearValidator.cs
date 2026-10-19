namespace SeventWebApplication.Models.VehicleNS
{
    public class YearValidator : IYearValidator
    {
        public void ValidateYear(int year)
        {
            if (year < 1000 || year > 9999)
                throw new ArgumentException("År måste vara ett 4-siffrigt tal");

            if (year > DateTime.Now.Year)
                throw new ArgumentOutOfRangeException("År kan inte vara i framtiden");

            if (year < 1886)
                throw new ArgumentOutOfRangeException("År kan inte vara tidigare än 1886");
        }
    }
}
