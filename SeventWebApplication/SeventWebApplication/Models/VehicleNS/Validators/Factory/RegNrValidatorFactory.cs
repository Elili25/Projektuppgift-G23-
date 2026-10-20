using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.VehicleNS
{
    public static class RegNrValidatorFactory
    {
        private static readonly Dictionary<CountryCode, IRegNrValidator> _validators = new()
        {
            { CountryCode.SE, new SwedishRegNrValidator() },
            { CountryCode.DE, new GermanRegNrValidator() },
            { CountryCode.UK, new UKRegNrValidator() },
            { CountryCode.NO, new NorwegianRegNrValidator() },
            { CountryCode.DK, new DanishRegNrValidator() },
            { CountryCode.FI, new FinnishRegNrValidator() },
        };

        public static IRegNrValidator Create(CountryCode country)
        {
            if (!_validators.TryGetValue(country, out var validator))
                throw new NotSupportedException($"Inget stöd för land: {country}");

            return validator;
        }
    }
}