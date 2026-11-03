namespace SeventWebApplication.Models.BookingValidation
{
    public class BookingValidator
    {
        // En samling för alla strategier
        private readonly IEnumerable<IValidationStrategy> _strategies;

        // Konstruktorn som tar emot alla registrerade strategier via DI
        public BookingValidator(IEnumerable<IValidationStrategy> strategies)
        {
            _strategies = strategies;
        }

        // Metoden som loopar igenom och validerar allt
        public List<ValidationError> ValidateAll(BookingRequest request)
        {
            var allErrors = new List<ValidationError>();

            // Går igenom varje enskild strategi/regel i systemet
            foreach (var strategy in _strategies)
            {
                var strategyErrors = strategy.Validate(request);

                if (strategyErrors != null)
                {
                    allErrors.AddRange(strategyErrors);
                }
            }

            return allErrors;
        }
    }

}
