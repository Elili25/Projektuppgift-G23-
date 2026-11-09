using SeventWebApplication.Models.Booking;

namespace SeventWebApplication.Models.Users
{
    // Admin kan allt en anställd kan + fixa med konton.
    // Sket i att köra på arv, den har bara samma interface som employee plus IAccountManager.
    // Då kan en Admin aldrig råka behandlas som "bara" en Employee nånstans = mindre points of failure eller nåt sånt.
    public class Admin : User, IBookingViewer, IBookingManager, IAccountManager, IBookingCreator
    {
        public override Role Role => Role.Admin;

        public IBookingVisibility Visibility { get; } = new AllBookingVisibility();

        // Bokar åt valfri kund, bokningen godkänns direkt
        public IBookingCreationStrategy CreationStrategy { get; } = new AnyUserBookingStrategy();

        public Admin(Guid userId, string name, string phone, string mail, string password, bool isActive = true)
            : base(userId, name, phone, mail, password, isActive)
        {
        }
    }
}
