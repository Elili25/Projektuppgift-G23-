using System;

namespace SeventWebApplication.Models.Maintenance;

public class MaintenancePeriod
{
    public Guid MaintenanceId { get; init; }
    public Guid VehicleId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public string Reason { get; init; } = string.Empty;



}



//+MaintenanceId : Guid
//+VehicleId : Guid
//+StartDate : DateTime
//+EndDate : DateTime
//+Reason : string