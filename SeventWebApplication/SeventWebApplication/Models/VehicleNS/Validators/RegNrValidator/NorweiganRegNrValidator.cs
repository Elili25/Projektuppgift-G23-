using SeventWebApplication.Models.VehicleNS;
using System.Text.RegularExpressions;

namespace SeventWebApplication.Models.VehicleNS
{
    // Norge: AB 12345
    public class NorwegianRegNrValidator : IRegNrValidator
    {
        public void ValidateRegNr(string regNr)
        {
            if (!Regex.IsMatch(regNr, @"^[A-Z]{2}\s\d{5}$"))
                throw new ArgumentException($"Ogiltigt norskt registreringsnummer: {regNr}");
        }
    }

}