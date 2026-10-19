using System.Text.RegularExpressions;

namespace SeventWebApplication.Models.VehicleNS
{
    // Sweden: ABC123
    public class SwedishRegNrValidator : IRegNrValidator
    {
        public void ValidateRegNr(string regNr)
        {
            if (!Regex.IsMatch(regNr, @"^[A-ZÅÄÖ]{3}\d{3}$"))
                throw new ArgumentException($"Ogiltigt svenskt registreringsnummer: {regNr}");
        }
    }
}
