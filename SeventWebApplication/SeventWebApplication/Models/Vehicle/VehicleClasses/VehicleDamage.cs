namespace SeventWebApplication.Models.Vehicle
{
    public class VehicleDamage
    {
        public Guid Id { get; set; }
        public required string Description { get; set; }
        public DateTime ReportedAt { get; set; }
        public string? PhotoUrl { get; set; } = null; // valfritt
    }
}
