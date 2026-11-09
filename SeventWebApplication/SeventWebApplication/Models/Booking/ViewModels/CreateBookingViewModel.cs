using SeventWebApplication.Models.BookingValidation;

namespace SeventWebApplication.Models.Booking
{
    // Det som skickas in från "Ny bokning"-formuläret.
    // Gör om sig själv till en BookingRequest, så controllern inte behöver veta hur fälten hänger ihop.
    public class CreateBookingForm
    {
        // Bara personal fyller i den (kunder bokar alltid åt sig själva)
        public string? TargetUserId { get; set; }

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

        public BookingRequest ToRequest() => new()
        {
            TargetUserId = string.IsNullOrWhiteSpace(TargetUserId) ? null : TargetUserId,
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

        public List<BookingOption> Customers { get; init; } = new();
        public List<BookingOption> Families { get; init; } = new();
        public List<BookingOption> Vehicles { get; init; } = new();
        public List<AddonOption> Addons { get; init; } = new();

        public List<ValidationError> Errors { get; init; } = new();

        // Har fältet fel? Används för att göra fältet rött i vyn
        public bool HasError(params string[] properties) =>
            Errors.Any(e => properties.Contains(e.Property));
    }
}
