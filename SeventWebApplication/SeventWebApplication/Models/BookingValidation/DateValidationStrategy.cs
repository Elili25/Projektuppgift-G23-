using SeventWebApplication.Models.Booking;

namespace SeventWebApplication.Models.BookingValidation
{
    public class DateValidationStrategy : IValidationStrategy
    {
        public IEnumerable<ValidationError> Validate(BookingRequest request)
        {
            var errors = new List<ValidationError>();

            if (request.Start.Date < DateTime.Today)
                errors.Add(new ValidationError
                {
                    Property = nameof(request.Start),
                    Message = "The start date cannot be in the past."
                });

            if (request.End <= request.Start)
                errors.Add(new ValidationError
                {
                    Property = nameof(request.End),
                    Message = "The end date must be after the start date."
                });

            return errors;
        }
    }
}
