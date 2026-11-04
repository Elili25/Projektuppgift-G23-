using System.Text.RegularExpressions;

namespace SeventWebApplication.Models.VehicleNS
{
    // Germany: B AB 1234 (district code may contain ÄÖÜ; optional E/H suffix; spaces optional)
    public class GermanRegNrValidator : IRegNrValidator
    {
        public void ValidateRegNr(string regNr)
        {
            if (regNr is null || !Regex.IsMatch(regNr.Trim(), @"^[A-ZÄÖÜ]{1,3}\s?[A-Z]{1,2}\s?[1-9]\d{0,3}[EH]?$"))
                throw new ArgumentException($"Ogiltigt tyskt registreringsnummer: {regNr}");
        }
    }
}
