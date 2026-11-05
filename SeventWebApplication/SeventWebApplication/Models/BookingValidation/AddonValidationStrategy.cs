using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Addon.Interfaces;
using SeventWebApplication.Models.Bookings;

namespace SeventWebApplication.Models.BookingValidation
{
    public class AddonValidationStrategy : IValidationStrategy
    {
        public IEnumerable<ValidationError> Validate(BookingRequest request)
        {
            var errors = new List<ValidationError>();

            if (request.Addons == null || !request.Addons.Any())
                return errors;

            // 1. Gruppera först på AddonKey (eftersom det är det som skickas in i vår DTO/Request)
            var totals = request.Addons
                .Where(a => !string.IsNullOrWhiteSpace(a.AddonKey))
                .GroupBy(a => a.AddonKey.Trim().ToLower())
                .Select(g => new { AddonKey = g.Key, Total = g.Sum(a => a.Quantity) });

            foreach (var addon in totals)
            {
                try
                {
                    // 2. Skapa en temporär instans för att läsa ut reglerna (kvantitet sätts till 1)
                    IAddon rules = AddonFactory.Create(addon.AddonKey, 1);

                    // 3. Jämför den inskickade totalen mot klassens MaxLimit
                    if (addon.Total > rules.MaxLimit)
                    {
                        errors.Add(new ValidationError
                        {
                            Property = nameof(request.Addons),
                            Message = $"Du kan lägga till max {rules.MaxLimit} {rules.DisplayName}. Du har angett {addon.Total} st.",
                        });
                    }
                }
                catch (ArgumentException)
                {
                    // Fångar upp om någon manipulerat HTML/JSON och skickat en ogiltig nyckel
                    errors.Add(new ValidationError
                    {
                        Property = nameof(request.Addons),
                        Message = $"Tillägget '{addon.AddonKey}' är inte ett giltigt alternativ i systemet.",
                    });
                }
            }

            return errors;
        }
    }
}
