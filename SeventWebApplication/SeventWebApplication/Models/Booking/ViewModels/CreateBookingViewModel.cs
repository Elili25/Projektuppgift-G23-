using SeventWebApplication.Models.BookingValidation;

namespace SeventWebApplication.Models.Booking
{
    // Det som skickas in från "Ny bokning"-formuläret.
    // Gör om sig själv till en BookingRequest, så controllern inte behöver veta hur fälten hänger ihop.
    public class CreateBookingForm
    {
        // Bara personal fyller i dessa (kunder bokar alltid åt sig själva).
        // Recipient: "management" / "customer" / "dummy". TargetUserId = vald kollega eller vald befintlig kund.
        public string? Recipient { get; set; }
        public string? TargetUserId { get; set; }

        // Uppgifter för en ny kund eller dummy
        public string? GuestName { get; set; }
        public string? GuestMail { get; set; }
        public string? GuestPhone { get; set; }
        public string? GuestPassword { get; set; }   // bara för en ny kund (inte dummy)

        // "family" = välj en fordonsfamilj, "vehicle" = välj en specifik bil
        public string VehicleChoice { get; set; } = "family";
        public Guid? VehicleFamilyId { get; set; }
        public Guid? VehicleId { get; set; }

        // Standard: imorgon kl 10 -> dagen efter kl 10
        public DateTime Start { get; set; } = DateTime.Today.AddDays(1).AddHours(10);
        public DateTime End { get; set; } = DateTime.Today.AddDays(2).AddHours(10);

        public string? Notes { get; set; }

        // Tilläggets nyckel (t.ex. "gps") -> antal. Binds från fält som heter Form.Addons[gps]
        public Dictionary<string, int> Addons { get; set; } = new();

        public SeventWebApplication.Models.Payment.PaymentMethod? PaymentMethod { get; set; } =
            SeventWebApplication.Models.Payment.PaymentMethod.Card;

        private BookingFor? ParseRecipient() => Recipient?.ToLowerInvariant() switch
        {
            "management" => SeventWebApplication.Models.Booking.BookingFor.Management,
            "customer" => SeventWebApplication.Models.Booking.BookingFor.Customer,
            "dummy" => SeventWebApplication.Models.Booking.BookingFor.Dummy,
            _ => null
        };

        public BookingRequest ToRequest() => new()
        {
            PaymentMethod = PaymentMethod,
            BookingFor = ParseRecipient(),
            // Dummy har ingen riktig kund, så ett kvarglömt id från ett annat val ska inte följa med
            TargetUserId = string.IsNullOrWhiteSpace(TargetUserId) || Recipient == "dummy" ? null : TargetUserId,
            Guest = new GuestDetails(GuestName, GuestMail, GuestPhone, GuestPassword),
            VehicleFamilyId = VehicleChoice == "family" ? VehicleFamilyId : null,
            VehicleId = VehicleChoice == "vehicle" ? VehicleId : null,
            Start = Start,
            End = End,
            Notes = Notes,
            Addons = Addons
                .Where(a => a.Value > 0)
                .Select(a => new AddonRequest { AddonKey = a.Key, Quantity = a.Value })
                .ToList()
        };
    }

    // Ett val i en dropdown
    public record BookingOption(string Value, string Text, string? Hint = null);

    // Ett tillägg som kan väljas i formuläret
    public record AddonOption(string Key, string Name, int MaxLimit, decimal Price);

    // Allt "Ny bokning"-sidan behöver: formuläret + listorna att välja från + eventuella fel
    public class CreateBookingViewModel
    {
        public CreateBookingForm Form { get; init; } = new();

        // Personal får välja kund, kunder ser inte det fältet alls
        public bool IsStaff { get; init; }

        // Personal att välja bland vid "Management". Kunder söks i stället via /Bookings/SearchCustomers.
        public List<BookingOption> StaffMembers { get; init; } = new();
        public List<BookingOption> Families { get; init; } = new();
        public List<BookingOption> Vehicles { get; init; } = new();
        public List<AddonOption> Addons { get; init; } = new();

        public List<ValidationError> Errors { get; init; } = new();

        // Har fältet fel? Används för att göra fältet rött i vyn
        public bool HasError(params string[] properties) =>
            Errors.Any(e => properties.Contains(e.Property));
    }
}
