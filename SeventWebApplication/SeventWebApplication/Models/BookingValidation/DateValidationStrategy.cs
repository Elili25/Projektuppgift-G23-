using SeventWebApplication.Models.Booking;

namespace SeventWebApplication.Models.BookingValidation
{
    public class DateValidationStrategy : IValidationStrategy
    {
        public IEnumerable<ValidationError> Validate(BookingRequest request)
        {
            var errors = new List<ValidationError>();

            if (request.Start.Date < DateTime.Today)  // Riktiga program ska använda <= så man måste boka en dag i förtid, men < behålls för testning
                errors.Add(new ValidationError
                {
                    Property = nameof(request.Start),
                    Message = "The pickup date cannot be in the past."
                });

            if (request.End <= request.Start)
                errors.Add(new ValidationError
                {
                    Property = nameof(request.End),
                    Message = "The return date must be after the pickup date."
                });

            return errors;
        }
    }
}
