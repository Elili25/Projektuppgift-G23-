using SeventWebApplication.Models.Booking;

namespace SeventWebApplication.Models.Users
{
    // "Den här användaren får skapa bokningar."
    // Samma tanke som IBookingViewer: rollen har med sig sin egen strategi (CreationStrategy)
    // som bestämmer åt vem den får boka och om bokningen godkänns direkt.
    // BookingCreationService tar in IBookingCreator, så den behöver inte veta vilken roll det är (ISP, DIP, OCP).
    public interface IBookingCreator : IActor
    {
        IBookingCreationStrategy CreationStrategy { get; }
    }
}
