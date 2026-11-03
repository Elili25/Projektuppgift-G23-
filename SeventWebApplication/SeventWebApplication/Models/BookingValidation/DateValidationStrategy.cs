namespace SeventWebApplication.Models.BookingValidation
{
    public class DateValidationStrategy : IValidationStrategy
    {
        // Samling för validationsfelen
        public IEnumerable<ValidationError> Validate(BookingRequest request)
        {
            var errors = new List<ValidationError>();

            // Regel 1: Startdatumet får inte ha passerat
            if (request.Start.Date < DateTime.Today)
            {
                errors.Add(new ValidationError
                {
                    Property = nameof(request.Start),
                    Message = "Startdatumet kan inte ha passserat.",
                });
            }

            // Regel 2: Slutdatumet måste vara efter startdatumet
            if (request.End <= request.Start)
            {
                errors.Add(new ValidationError
                {
                    Property = nameof(request.End),
                    Message = "Slutdatumet måste vara efter startdatumet.",
                });
            }

            return errors;
        }

    }
}
