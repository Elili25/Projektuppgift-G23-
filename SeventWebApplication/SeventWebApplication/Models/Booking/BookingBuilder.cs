using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Addon.Interfaces;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Models.Booking
{
    // Builder (GoF) för Booking.
    // En bokning har många delar där vissa är valfria (tillägg, anteckningar) och bilen kan vara
    // antingen en specifik bil ELLER en familj. Istället för en massa konstruktorer bygger man steg för steg:
    //
    //   new BookingBuilder()
    //       .ForUser(userId)
    //       .WithVehicleFamily(familyId)
    //       .FromTo(start, end)
    //       .WithAddon(gps)
    //       .WithNotes("Hämtas kl 08")
    //       .Build();
    //
    // Build() kollar att allt som krävs finns innan bokningen skapas, så en halvfärdig bokning kan aldrig skapas.
    public class BookingBuilder
    {
        private string? _userId;
        private Vehicle? _vehicle;
        private Guid? _vehicleFamilyId;
        private DateTime? _start;
        private DateTime? _end;
        private AddonPackage _addons = new();
        private string? _notes;

        public BookingBuilder ForUser(string userId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(userId);
            _userId = userId;
            return this;
        }

        // Kunden har valt en specifik bil
        public BookingBuilder WithVehicle(Vehicle vehicle)
        {
            _vehicle = vehicle ?? throw new ArgumentNullException(nameof(vehicle));
            return this;
        }

        // Kunden har valt en familj, bilen tilldelas senare (VehicleAssignmentService)
        public BookingBuilder WithVehicleFamily(Guid vehicleFamilyId)
        {
            if (vehicleFamilyId == Guid.Empty)
                throw new ArgumentException("Ogiltig fordonsfamilj.");

            _vehicleFamilyId = vehicleFamilyId;
            return this;
        }

        public BookingBuilder FromTo(DateTime start, DateTime end)
        {
            if (end <= start)
                throw new ArgumentException("Slutdatumet måste vara efter startdatumet.");

            _start = start;
            _end = end;
            return this;
        }

        public BookingBuilder WithAddon(IAddon addon)
        {
            ArgumentNullException.ThrowIfNull(addon);
            _addons.Add(addon);   // AddonPackage slår ihop tillägg av samma typ
            return this;
        }

        public BookingBuilder WithNotes(string? notes)
        {
            _notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
            return this;
        }

        // TODO: WithPricing(IPricingStrategy) enligt UML:en när prisstrategin finns i koden

        public Booking Build()
        {
            if (_userId is null)
                throw new InvalidOperationException("Bokningen saknar användare (ForUser).");

            if (_start is null || _end is null)
                throw new InvalidOperationException("Bokningen saknar datum (FromTo).");

            if (_vehicle is null && _vehicleFamilyId is null)
                throw new InvalidOperationException("Välj en bil eller en fordonsfamilj.");

            if (_vehicle is not null && _vehicleFamilyId is not null)
                throw new InvalidOperationException("Välj antingen en bil eller en fordonsfamilj, inte båda.");

            string bookingId = Guid.NewGuid().ToString();

            Booking booking = _vehicle is not null
                ? new Booking(bookingId, _userId, _vehicle, _start.Value, _end.Value, _addons)
                : new Booking(bookingId, _userId, _vehicleFamilyId!.Value, _start.Value, _end.Value, _addons);

            booking.Notes = _notes;

            Reset();   // så att samma builder kan återanvändas utan att gammal data följer med
            return booking;
        }

        public void Reset()
        {
            _userId = null;
            _vehicle = null;
            _vehicleFamilyId = null;
            _start = null;
            _end = null;
            _addons = new AddonPackage();
            _notes = null;
        }
    }
}
