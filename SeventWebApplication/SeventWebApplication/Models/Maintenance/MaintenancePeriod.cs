using System;

namespace SeventWebApplication.Models.Maintenance;

public record MaintenancePeriod
{
    public Guid MaintenanceId { get; init; } = Guid.NewGuid();
    public Guid VehicleId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public string Reason { get; init; } = string.Empty;



}

