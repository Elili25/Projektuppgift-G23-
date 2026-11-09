namespace SeventWebApplication.Models.Booking
{
    // Strategy för HUR en användare får skapa bokningar.
    // Varje roll som implementerar IBookingCreator har med sig sin egen strategi, så
    // BookingCreationService behöver aldrig fråga "är du kund eller anställd?" (OCP, ingen if-sats på roller).
    public interface IBookingCreationStrategy
    {
        // Avgör vem bokningen gäller.
        // actorId = den som är inloggad och skapar bokningen
        // requestedId = den kund som valts i formuläret (kan vara null)
        // Kastar UnauthorizedAccessException/ArgumentException om kombinationen inte är tillåten.
        string ResolveTarget(string actorId, string? requestedId);

        // true = bokningen godkänns direkt (status Confirmed) istället för att vänta på personal
        bool AutoApprove { get; }
    }
}
