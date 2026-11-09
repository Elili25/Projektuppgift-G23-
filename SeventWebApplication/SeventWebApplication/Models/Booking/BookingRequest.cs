namespace SeventWebApplication.Models.Booking
{
    // Det som kommer in från formuläret/klienten när någon vill boka.
    // Record så att BookingCreationService kan göra en kopia med rätt kund ifylld (request with { ... })
    // utan att ändra originalet.
    public record BookingRequest
    {
        // Vilken kund bokningen gäller. Kunder lämnar den tom (SelfBookingStrategy fyller i deras eget id),
        // personal måste ange den (AnyUserBookingStrategy).
        public string? TargetUserId { get; init; }

        // Antingen en specifik bil ELLER en fordonsfamilj
        public Guid? VehicleId { get; init; }
        public Guid? VehicleFamilyId { get; init; }

        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public string? Notes { get; init; }
        public List<AddonRequest> Addons { get; init; } = new();
    }
}
