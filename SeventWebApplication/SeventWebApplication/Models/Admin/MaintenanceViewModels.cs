using System.ComponentModel.DataAnnotations;
using SeventWebApplication.Models.Maintenance;

namespace SeventWebApplication.Models.Admin
{
    // En rad i underhållslistan i admin
    public class MaintenanceRowViewModel
    {
        public Guid MaintenanceId { get; init; }
        public Guid VehicleId { get; init; }
        public string RegNr { get; init; } = "";
        public string Vehicle { get; init; } = "";
        public MaintenanceType Type { get; init; }
        public string Description { get; init; } = "";
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public int? Mileage { get; init; }
        public decimal? Cost { get; init; }
        public string? Notes { get; init; }
        public bool IsCompleted { get; init; }

        // Completed / Ongoing / Scheduled, används för statusbrickan
        public string Status => IsCompleted ? "Completed" : StartDate > DateTime.UtcNow ? "Scheduled" : "Ongoing";
    }

    public class MaintenanceListViewModel
    {
        public List<MaintenanceRowViewModel> Rows { get; init; } = new();
        public int OngoingCount => Rows.Count(r => r.Status == "Ongoing");
        public int ScheduledCount => Rows.Count(r => r.Status == "Scheduled");
    }

    public enum MaintenanceMode
    {
        Schedule,       // planerat underhåll med start- och slutdatum
        OutOfService    // bilen tas ur drift nu, utan slutdatum
    }

    // Formuläret "New maintenance"
    public class MaintenanceForm
    {
        public MaintenanceMode Mode { get; set; } = MaintenanceMode.Schedule;

        [Required(ErrorMessage = "Select a vehicle.")]
        public Guid? VehicleId { get; set; }

        public MaintenanceType Type { get; set; } = MaintenanceType.Service;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = "";

        public DateTime? StartDate { get; set; } = DateTime.Today;
        public DateTime? EndDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Mileage cannot be negative.")]
        public int? Mileage { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Cost cannot be negative.")]
        public decimal? Cost { get; set; }

        public string? Notes { get; set; }
    }
}
