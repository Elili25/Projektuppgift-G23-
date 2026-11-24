namespace SeventWebApplication.Models.VehicleNS
{
    // En period med en viss status för en bil. EndDate = null betyder att perioden pågår nu.
    // Record = oföränderlig. En period stängs genom att en kopia skapas med "with { EndDate = ... }".
    public record VehicleStatusHistory
    {
        public Guid VehicleStatusHistoryId { get; init; } = Guid.NewGuid();
        public Guid VehicleId { get; init; }
        public VehicleStatus Status { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public string? Notes { get; init; }

        public bool IsCurrent => EndDate == null;

        public VehicleStatusHistory(Guid vehicleId, VehicleStatus status, DateTime startDate, string? notes = null)
        {
            VehicleId = vehicleId;
            Status = status;
            StartDate = startDate;
            Notes = notes;
        }
    }
}