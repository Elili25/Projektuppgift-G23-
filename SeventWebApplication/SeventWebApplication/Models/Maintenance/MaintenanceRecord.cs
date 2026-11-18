using System;

namespace SeventWebApplication.Models.Maintenance;

public record MaintenanceRecord
{
	public Guid MaintenanceId { get; init; } = Guid.NewGuid();	
	public required Guid VehicleId { get; init; }
	public required MaintenanceType Type { get; init; }
	// enum 
	public required string Description { get; init; }
	public required DateTime StartDate { get; init; }	
	public required DateTime EndDate { get; init;
	public int? Mileage { get; init; }
	public decimal? Cost { get; init; }
	public string ? Notes { get; init; }
	public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}



//+MaintenanceId : Guid
//+VehicleId : Guid
//+ Description : string
//+StartDate : DateTime
//+ EndDate : DateTime ?
//+Mileage : int ?
//+Cost : decimal ?
//+Notes : string
//+ CreatedAt : DateTime