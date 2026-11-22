namespace SeventWebApplication.Models.VehicleNS
{
    // Läsbar beskrivning av regnummerformatet per land. Används både i felmeddelandena
    // från validatorerna och som placeholder/hjälptext i formuläret.
    public static class RegNrFormatHints
    {
        public const string Sweden = "ABC 123 eller ABC 12A";
        public const string Germany = "B AB 1234";
        public const string UK = "AB12 CDE";
        public const string Norway = "AB 12345";
        public const string Denmark = "AB 12 345";
        public const string Finland = "ABC-123";

        public static string For(CountryCode country) => country switch
        {
            CountryCode.SE => Sweden,
            CountryCode.DE => Germany,
            CountryCode.UK => UK,
            CountryCode.NO => Norway,
            CountryCode.DK => Denmark,
            CountryCode.FI => Finland,
            _ => ""
        };

        public static string Message(string hint) => $"Ange i formatet {hint}";
    }
}
