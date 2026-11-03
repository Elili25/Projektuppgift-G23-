using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Addon.Enums;

namespace SeventWebApplication.Models.BookingValidation
{
    public class AddonValidationStrategy : IValidationStrategy
    {
        // Definierar maxgränserna för addons
        private readonly Dictionary<AddonType, int> _maxLimits = new()
    {
        { AddonType.Gps, 1 },
        { AddonType.ExtraDriver, 1 },
        { AddonType.ChildSeat, 3 }
        // Här läggs nya addons och dess maxantal till
    };
        
        public IEnumerable<ValidationError> Validate(BookingRequest request)
        {
            var errors = new List<ValidationError>();

            if (request.Addons == null) return errors;

            // Grupperar och summerar alla inskickade addons samtidigt
            var totals = request.Addons
                .GroupBy(a => a.AddonType)
                .Select(g => new { AddonType = g.Key, Total = g.Sum(a => a.Quantity) });

            // Loopar igenom de summerade tilläggen och jämför mot reglerna
            foreach (var addon in totals)
            {
                // Kontrollerar om det finns en maxgräns definierad för denna typ och om den har överskridits
                if (_maxLimits.TryGetValue(addon.AddonType, out int maxAllowed) && addon.Total > maxAllowed)
                {
                    // Här översätts enumen till svenska för felmeddelandet
                    string svensktNamn = addon.AddonType switch
                    {
                        AddonType.Gps => "GPS",
                        AddonType.ExtraDriver => "extra förare",
                        AddonType.ChildSeat => "barnstolar",
                        _ => addon.AddonType.ToString() // Fallback om man glömmer översätta nya enums i framtiden
                    };

                    errors.Add(new ValidationError
                    {
                        Property = nameof(request.Addons),
                        Message = $"Du kan lägga till max {maxAllowed} {svensktNamn}. Du har angett {addon.Total} st.",
                    });
                }
            }

            return errors;
        }
    }


}
