namespace SeventWebApplication.Models.Booking
{
    // Kunder: får bara boka åt sig själva, och bokningen måste godkännas av personal (Pending).
    public class SelfBookingStrategy : IBookingCreationStrategy
    {
        public bool AutoApprove => false;

        public string ResolveTarget(string actorId, string? requestedId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(actorId);

            // Försöker kunden boka åt någon annan stoppas det här
            if (!string.IsNullOrWhiteSpace(requestedId)
                && !string.Equals(requestedId.Trim(), actorId, StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Kunder kan bara skapa bokningar åt sig själva.");
            }

            return actorId;
        }
    }
}
