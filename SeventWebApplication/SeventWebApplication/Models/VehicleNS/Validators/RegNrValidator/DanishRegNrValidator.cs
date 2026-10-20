using SeventWebApplication.Models.VehicleNS;
using System.Text.RegularExpressions;

namespace SeventWebApplication.Models.VehicleNS
{

    // Danmark: AB 12 345
    public class DanishRegNrValidator : IRegNrValidator
    {
        public void ValidateRegNr(string regNr)
        {
            if (!Regex.IsMatch(regNr, @"^[A-Z]{2}\s\d{2}\s\d{3}$"))
                throw new ArgumentException($"Ogiltigt danskt registreringsnummer: {regNr}");
        }
    }
}