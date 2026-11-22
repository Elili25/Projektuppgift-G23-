using SeventWebApplication.Models.Booking;

namespace SeventWebApplication.Models.BookingValidation
{
    public class BookingValidator
    {
        private readonly IEnumerable<IValidationStrategy> _strategies;

        public BookingValidator(IEnumerable<IValidationStrategy> strategies)
        {
            _strategies = strategies;
        }

        public List<ValidationError> ValidateAll(BookingRequest request)
        {
            var allErrors = new List<ValidationError>();

            foreach (var strategy in _strategies)
                allErrors.AddRange(strategy.Validate(request));

            return allErrors;
        }
    }
}
