using SeventWebApplication.Models.Booking;

namespace SeventWebApplication.Models.Users
{
    // Kunden får bara kolla på sina egna bokningar,
    // och bara skicka bokningsförfrågningar åt sig själv (SelfBookingStrategy, väntar på godkännande).
    public class Customer : User, IBookingViewer, IBookingCreator
    {
        public override Role Role => Role.Customer;

        // Filtrerar bort allt som inte är kundens
        public IBookingVisibility Visibility { get; }

        // Bokar bara åt sig själv, bokningen blir Pending
        public IBookingCreationStrategy CreationStrategy { get; } = new SelfBookingStrategy();

        public Customer(Guid userId, string name, string phone, string mail, string password, bool isActive = true)
            : base(userId, name, phone, mail, password, isActive)
        {
            Visibility = new OwnBookingVisibility(userId);
        }
    }
}
