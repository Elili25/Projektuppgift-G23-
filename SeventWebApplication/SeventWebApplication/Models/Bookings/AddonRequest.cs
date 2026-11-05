
namespace SeventWebApplication.Models.Bookings
{
    public record AddonRequest  // Record klass skrivskyddar innehållet
    {
        public string AddonKey { get; init; } = string.Empty;
        public int Quantity { get; init; }
    }
}
