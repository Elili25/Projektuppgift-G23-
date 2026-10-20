namespace SeventWebApplication.Models.VehicleNS
{
    public sealed record RegistrationPlate
    {
        public string RegNr { get; }
        public CountryCode CountryCode { get; }

        public RegistrationPlate(
            string regNr,
            CountryCode countryCode)
        {
            if (string.IsNullOrWhiteSpace(regNr))
                throw new ArgumentException(
                    "Registration number is required.");

            RegNr = regNr;
            CountryCode = countryCode;
        }
    }
}
