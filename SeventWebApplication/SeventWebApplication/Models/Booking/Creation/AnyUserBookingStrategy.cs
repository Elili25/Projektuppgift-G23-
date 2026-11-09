namespace SeventWebApplication.Models.Booking
{
    // Anställda och admins: bokar åt vem som helst (kunder, annan personal eller sig själva),
    // och bokningen godkänns direkt (Confirmed).
    public class AnyUserBookingStrategy : IBookingCreationStrategy
    {
        public bool AutoApprove => true;

        public string ResolveTarget(string actorId, string? requestedId)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(actorId);

            // Ingen vald = personalen bokar åt sig själv
            return string.IsNullOrWhiteSpace(requestedId) ? actorId : requestedId.Trim();
        }
    }
}
