using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.BookingValidation;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.Booking
{
    // Bokningsflödet: tar emot en bokningsförfrågan och gör den till en sparad bokning.
    //
    //  1. Är kontot aktivt?
    //  2. Vem gäller bokningen? -> rollens IBookingCreationStrategy bestämmer (kund = sig själv, personal = vem som helst)
    //  3. Kör alla valideringsregler (BookingValidator, se flödesschemat)
    //  4. Bygg bokningen med BookingBuilder
    //  5. Spara i BookingRegistry (som kollar dubbelbokning)
    //  6. AutoApprove? -> tilldela bil om det är en familjebokning och sätt status till Confirmed
    //
    // Allt som går fel returneras som ValidationError i BookingResult istället för att kasta,
    // så att anroparen kan visa felen för användaren.
    public class BookingCreationService
    {
        private readonly BookingValidator _bookingValidator;
        private readonly IUserRepository _users;
        private readonly FleetManager _fleetManager;
        private readonly VehicleAssignmentService _assignmentService;

        public BookingCreationService(
            BookingValidator bookingValidator,
            IUserRepository users,
            FleetManager fleetManager,
            VehicleAssignmentService assignmentService)
        {
            _bookingValidator = bookingValidator;
            _users = users;
            _fleetManager = fleetManager;
            _assignmentService = assignmentService;
        }

        public BookingResult CreateBooking(IBookingCreator actor, BookingRequest request)
        {
            ArgumentNullException.ThrowIfNull(actor);
            ArgumentNullException.ThrowIfNull(request);

            // 1. Inaktiverade konton får inte boka
            if (!actor.IsActive)
                return Fail("Actor", "Inaktiverade konton kan inte skapa bokningar.");

            // 2. Strategin bestämmer vem bokningen gäller
            IBookingCreationStrategy strategy = actor.CreationStrategy;
            string targetUserId;
            try
            {
                targetUserId = strategy.ResolveTarget(actor.UserId.ToString(), request.TargetUserId);
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or ArgumentException)
            {
                return Fail(nameof(request.TargetUserId), ex.Message);
            }

            if (!Guid.TryParse(targetUserId, out Guid targetGuid) || _users.GetById(targetGuid) is not { IsActive: true })
                return Fail(nameof(request.TargetUserId), "Användaren finns inte eller är inaktiverad.");

            // Kopia av förfrågan med rätt kund ifylld, så valideringsreglerna ser samma sak som bokningen får
            BookingRequest resolvedRequest = request with { TargetUserId = targetUserId };

            // 3. Valideringsreglerna (datum, tillägg osv.)
            List<ValidationError> validationErrors = _bookingValidator.ValidateAll(resolvedRequest);
            if (validationErrors.Count > 0)
                return new BookingResult
                {
                    Success = false,
                    Errors = validationErrors
                };

            // 4. Bygg bokningen
            Booking booking;
            try
            {
                var builder = new BookingBuilder()
                    .ForUser(targetUserId)
                    .FromTo(resolvedRequest.Start, resolvedRequest.End)
                    .WithNotes(resolvedRequest.Notes);

                if (resolvedRequest.VehicleId is Guid vehicleId)
                {
                    Vehicle? vehicle = _fleetManager.GetVehicle(vehicleId);
                    if (vehicle is null)
                        return Fail(nameof(request.VehicleId), "Bilen finns inte.");

                    if (!_fleetManager.IsAvailable(vehicle, resolvedRequest.Start, resolvedRequest.End))
                        return Fail(nameof(request.VehicleId), "Bilen är redan bokad eller ur drift under perioden.");

                    builder.WithVehicle(vehicle);
                }

                if (resolvedRequest.VehicleFamilyId is Guid familyId)
                {
                    if (!_assignmentService.IsFamilyAvailable(familyId, resolvedRequest.Start, resolvedRequest.End))
                        return Fail(nameof(request.VehicleFamilyId), "Det finns inga lediga bilar i den här familjen under perioden.");

                    builder.WithVehicleFamily(familyId);
                }

                foreach (AddonRequest addonRequest in resolvedRequest.Addons)
                    builder.WithAddon(AddonFactory.Create(addonRequest.AddonKey, addonRequest.Quantity));

                booking = builder.Build();
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return Fail("Booking", ex.Message);
            }

            // 5. Spara. RegisterBooking kollar bl.a. att en vald bil inte redan är bokad.
            try
            {
                BookingRegistry.Instance.RegisterBooking(booking);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return Fail("Booking", ex.Message);
            }

            // 6. Personal: godkänn direkt. Familjebokningar får en bil först.
            // Går det inte att tilldela en bil ligger bokningen kvar som Pending och kan godkännas senare.
            if (strategy.AutoApprove
                && (booking.Vehicle is not null || _assignmentService.ConfirmVehicleAssignment(booking)))
            {
                booking.Confirm();
            }

            return new BookingResult
            {
                Success = true,
                CreatedBooking = booking
            };
        }

        // Ett misslyckat resultat med ETT felmeddelande.
        // Samlat på ett ställe så att flödet i CreateBooking går att läsa uppifrån och ned (DRY).
        // property = vilket fält felet gäller (t.ex. "VehicleId"), så formuläret vet vilket fält som ska bli rött.
        private static BookingResult Fail(string property, string message) => new BookingResult
        {
            Success = false,
            Errors = [new ValidationError { Property = property, Message = message }]
        };
    }
}
