using SeventWebApplication.Models.VehicleNS;
using System.Text.RegularExpressions;

namespace SeventWebApplication.Models.VehicleNS
{

    // Finland: ABC-123
    public class FinnishRegNrValidator : IRegNrValidator
    {
        public void ValidateRegNr(string regNr)
        {
            if (!Regex.IsMatch(regNr, @"^[A-ZÄÖÅ]{2,3}-\d{1,3}$"))
                throw new ArgumentException($"Ogiltigt finskt registreringsnummer: {regNr}");
        }
    }
}