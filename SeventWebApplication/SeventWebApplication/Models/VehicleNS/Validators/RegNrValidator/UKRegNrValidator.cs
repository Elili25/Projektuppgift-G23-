using System.Text.RegularExpressions;

namespace SeventWebApplication.Models.VehicleNS
{
    // UK: AB12 CDE
    public class UKRegNrValidator : IRegNrValidator
    {
        public void ValidateRegNr(string regNr)
        {
            if (!Regex.IsMatch(regNr, @"^[A-Z]{2}\d{2}\s[A-Z]{3}$"))
                throw new ArgumentException($"Ogiltigt brittiskt registreringsnummer: {regNr}");
        }
    }
}
