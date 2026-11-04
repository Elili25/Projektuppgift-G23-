using SeventWebApplication.Models.Addon;
using SeventWebApplication.Models.Addon.Enums;
using SeventWebApplication.Models.Booking; // BookingStatus
using BookingModel = SeventWebApplication.Models.Booking.Booking;

namespace SeventWebApplication.Models.Users
{
    public enum BookingTimeStatus
    {
        Pending,
        Confirmed,
        Active,
        Completed,
        Cancelled
    }

    // Status är ju inte implementerat än men lägger som enum här för att det ska vara lättare att implementera senare.
    // Då kan man bara ändra From-metoden och vyn behöver inte ändras. För tillfället ger vi bara bokningarna random status vid seedning

    // En rad i boknings-tabellen. Vyn ska inte behöva gräva i Booking/Vehicle/Addon
    // själv, så vi plockar ihop allt den behöver här.
    public class BookingRowViewModel
    {
        public string BookingId { get; init; } = "";
        public string ShortId => BookingId.Length > 8 ? BookingId[..8].ToUpper() : BookingId.ToUpper();

        public string CustomerName { get; init; } = "";
        public string CustomerMail { get; init; } = "";

        public string Vehicle { get; init; } = "";
        public string RegNr { get; init; } = "";

        public DateTime Start { get; init; }
        public DateTime End { get; init; }
        public int Days => Math.Max(1, (int)Math.Ceiling((End - Start).TotalDays)); // så att en bokning som startar och slutar samma dag räknas som 1 dag

        public List<string> Addons { get; init; } = new();
        public decimal? TotalPrice { get; init; }

        public BookingTimeStatus Status { get; init; }

        public static BookingRowViewModel From(BookingModel booking, IUserRepository users, DateTime now)
        {
            // Booking.UserId är en string, så vi får parsa den för att hitta kunden
            User? customer = Guid.TryParse(booking.UserId, out Guid id) ? users.GetById(id) : null;

            // Översätter domänens BookingStatus till vyns BookingTimeStatus
            BookingTimeStatus status = booking.Status switch
            {
                BookingStatus.Pending => BookingTimeStatus.Pending,
                BookingStatus.Confirmed => BookingTimeStatus.Confirmed,
                BookingStatus.Active => BookingTimeStatus.Active,
                BookingStatus.Completed => BookingTimeStatus.Completed,
                BookingStatus.Cancelled => BookingTimeStatus.Cancelled,
                _ => BookingTimeStatus.Pending
            };

            return new BookingRowViewModel
            {
                BookingId = booking.BookingId,
                CustomerName = customer?.Name ?? "Okänd kund",
                CustomerMail = customer?.Mail ?? "",
                Vehicle = booking.Vehicle is null
                    ? "–"
                    : $"{booking.Vehicle.Model.Brand.BrandName} {booking.Vehicle.Model.ModelName}",
                RegNr = booking.Vehicle?.RegistrationPlate.RegNr ?? "",
                Start = booking.StartDate,
                End = booking.EndDate,
                Addons = GetAddonNames(booking.Addon),
                TotalPrice = booking.Addon?.GetTotalAddonPrice(),
                Status = status
            };
        }

        // Översätter tillvalen i bokningens AddonPackage till visningsnamn.
        // Fler än en av samma sort visas som t.ex. "Barnstol ×3".
        private static List<string> GetAddonNames(AddonPackage? package)
        {
            if (package is null)
                return new List<string>();

            return package.Items
                .Select(a =>
                {
                    string name = a.AddonType switch
                    {
                        AddonType.Gps => "GPS",
                        AddonType.ChildSeat => "Barnstol",
                        AddonType.ExtraDriver => "Extra förare",
                        _ => a.AddonType.ToString()   // nya tillval syns ändå, bara med enumnamnet
                    };
                    return a.Quantity > 1 ? $"{name} ×{a.Quantity}" : name;
                })
                .ToList();
        }
    }
}
