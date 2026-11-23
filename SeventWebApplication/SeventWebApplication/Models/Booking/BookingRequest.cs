namespace SeventWebApplication.Models.Booking
{
    // Det som kommer in från formuläret/klienten när någon vill boka.
    // Record så att BookingCreationService kan göra en kopia med rätt kund ifylld (request with { ... })
    // utan att ändra originalet.
    // Vem personalen bokar åt
    public enum BookingFor
    {
        Management, // personal (sig själv eller en kollega)
        Customer,   // befintlig kund (vald via sökning) eller en ny kund
        Dummy       // testbokning utan riktig kund
    }

    // Password: bara för en ny riktig kund. Med lösenord kan kunden logga in och boka själv senare.
    public record GuestDetails(string? Name, string? Mail, string? Phone, string? Password = null);

    public record BookingRequest
    {
        // Vilken kund bokningen gäller. Kunder lämnar den tom (SelfBookingStrategy fyller i deras eget id),
        // personal måste ange den (AnyUserBookingStrategy).
        public string? TargetUserId { get; init; }

        // Bara personal: vem bokningen gäller. Null = som förut (TargetUserId, annars personalen själv).
        public BookingFor? BookingFor { get; init; }

        // Uppgifter om en ny kund/dummy. Används när BookingFor är Customer utan vald kund, eller Dummy.
        public GuestDetails? Guest { get; init; }

        // Antingen en specifik bil ELLER en fordonsfamilj
        public Guid? VehicleId { get; init; }
        public Guid? VehicleFamilyId { get; init; }

        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public string? Notes { get; init; }
        public List<AddonRequest> Addons { get; init; } = new();

        // Betalsätt (krävs). Betalningen görs när bokningen skapas.
        public SeventWebApplication.Models.Payment.PaymentMethod? PaymentMethod { get; init; }
    }
}
