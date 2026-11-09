using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Addon.Interfaces;
using SeventWebApplication.Models.Booking;

namespace SeventWebApplication.Models.BookingValidation
{
    public class AddonValidationStrategy : IValidationStrategy
    {
        public IEnumerable<ValidationError> Validate(BookingRequest request)
        {
            var errors = new List<ValidationError>();

            if (request.Addons == null || !request.Addons.Any())
                return errors;

            var totals = request.Addons
                .Where(addon => !string.IsNullOrWhiteSpace(addon.AddonKey))
                .GroupBy(addon => addon.AddonKey.Trim().ToLowerInvariant())
                .Select(group => new { AddonKey = group.Key, Total = group.Sum(addon => addon.Quantity) });

            foreach (var addon in totals)
            {
                try
                {
                    IAddon rules = AddonFactory.Create(addon.AddonKey, 1);
                    if (addon.Total > rules.MaxLimit)
                    {
                        errors.Add(new ValidationError
                        {
                            Property = nameof(request.Addons),
                            Message = $"Du kan lägga till max {rules.MaxLimit} {rules.DisplayName}. Du har angett {addon.Total} st."
                        });
                    }
                }
                catch (ArgumentException)
                {
                    errors.Add(new ValidationError
                    {
                        Property = nameof(request.Addons),
                        Message = $"Tillägget '{addon.AddonKey}' är inte ett giltigt alternativ i systemet."
                    });
                }
            }

            return errors;
        }
    }
}
