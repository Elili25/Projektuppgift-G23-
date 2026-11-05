using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Addon.Interfaces;
using SeventWebApplication.Models.BookingValidation;

namespace SeventWebApplication.Models.Bookings
{
    // Exempel på hur creationService kan se ut med validators
    public class BookingCreationService
    {
        private readonly BookingValidator _bookingValidator;

        public BookingCreationService(BookingValidator bookingValidator)
        {
            _bookingValidator = bookingValidator;
        }

        public BookingResult CreateBooking(BookingRequest req)
        {
            // 1. Kör alla valideringsstrategier via validatorn
            List<ValidationError> validationErrors = _bookingValidator.ValidateAll(req);

            // 2. Om det finns några valideringsfel, avbryts direkt och returnerar ett misslyckat BookingResult
            if (validationErrors.Any())
            {
                return new BookingResult
                {
                    Success = false,
                    CreatedBooking = null, // Tillåtet att vara null
                    Errors = validationErrors
                };
            }

            try
            {
                // 3. Skapar bokningsobjektet eftersom valideringen godkände allt
                var newBooking = new Booking
                {
                    StartDate = req.Start,
                    EndDate = req.End,
                    // Lägg till annat nödvändigt
                };

                // 4. Transformerar AddonRequests till IAddon-domänobjekt via AddonFactory
                if (req.Addons != null)
                {
                    foreach (var addonReq in req.Addons)
                    {                       
                        // Skapar dynamiskt den korrekta konkreta addonklassen
                        IAddon concreteAddon = AddonFactory.Create(addonReq.AddonKey, addonReq.Quantity);

                        // Lägger till bokningens AddonPackage i newBooking
                        newBooking.Addons.Add(concreteAddon);
                    }
                }

                // 5. Returnera ett lyckat BookingResult med den färdiga bokningen
                return new BookingResult
                {
                    Success = true,
                    CreatedBooking = newBooking,
                    Errors = new List<ValidationError>() // Tom lista indikerar framgång
                };
            }
            catch (ArgumentException ex)
            {
                // Extra säkerhetsspärr om något oväntat kommer förbi valideringen
                return new BookingResult
                {
                    Success = false,
                    CreatedBooking = null,
                    Errors = new List<ValidationError>
                    {
                        new ValidationError
                        {
                            Property = "System",
                            Message = "Ett oväntat systemfel inträffade vid skapandet av tillägg."
                        }
                    }
                };
            }
        }
    }
}
