using System.Text.RegularExpressions;

namespace SeventWebApplication.Models.VehicleNS
{
    // Sweden: ABC123 or ABC12D (I, Q, V, Å, Ä, Ö are not used; optional space allowed)
    public class SwedishRegNrValidator : IRegNrValidator
    {
        public void ValidateRegNr(string regNr)
        {
            if (regNr is null || !Regex.IsMatch(regNr.Trim(), @"^[A-HJ-PR-UW-Z]{3}\s?\d{2}[0-9A-HJ-NPR-UW-Z]$"))
                throw new ArgumentException($"Ogiltigt svenskt registreringsnummer: {regNr}");
        }
    }
}
