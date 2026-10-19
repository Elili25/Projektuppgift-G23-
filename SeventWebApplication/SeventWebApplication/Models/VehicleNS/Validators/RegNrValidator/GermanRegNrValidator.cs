using System.Text.RegularExpressions;

namespace SeventWebApplication.Models.VehicleNS
{
    // Germany: AB CD 1234
    public class GermanRegNrValidator : IRegNrValidator
    {
        public void ValidateRegNr(string regNr)
        {
            if (!Regex.IsMatch(regNr, @"^[A-Z]{1,3}\s[A-Z]{1,2}\s\d{1,4}$"))
                throw new ArgumentException($"Ogiltigt tyskt registreringsnummer: {regNr}");
        }
    }
}
