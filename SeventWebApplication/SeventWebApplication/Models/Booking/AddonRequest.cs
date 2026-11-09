namespace SeventWebApplication.Models.Booking
{
    public record AddonRequest
    {
        public string AddonKey { get; init; } = string.Empty;
        public int Quantity { get; init; }
    }
}
