using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SeventWebApplication.Models.Admin;
using SeventWebApplication.Models.Maintenance;
using SeventWebApplication.Models.Maintenance.Repsitories;
using SeventWebApplication.Models.Maintenance.Services;
using SeventWebApplication.Models.Users;

namespace SeventWebApplication.Controllers
{
    // Underhållshantering i admin (/Maintenance): lista, planera underhåll, ta bil ur drift, avsluta.
    // Bara personal, samma kontroll som övriga adminsidor.
    public class MaintenanceController : StaffController
    {
        private readonly VehicleMaintenanceService _maintenance;
        private readonly MaintenanceRepository _records;
        private readonly FleetManager _fleet;

        public MaintenanceController(UserSession session, VehicleMaintenanceService maintenance, MaintenanceRepository records, FleetManager fleet) : base(session)
        {
            _maintenance = maintenance;
            _records = records;
            _fleet = fleet;
        }

        // GET /Maintenance – alla underhållsposter, pågående och planerade först
        [HttpGet]
        public IActionResult Index()
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            var rows = _records.GetAll()
                .Select(ToRow)
                .OrderBy(r => r.IsCompleted)
                .ThenByDescending(r => r.StartDate)
                .ToList();

            return View(new MaintenanceListViewModel { Rows = rows });
        }

        // GET /Maintenance/Create – formulär, valfritt förvald bil (?vehicleId=)
        [HttpGet]
        public IActionResult Create(Guid? vehicleId)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            FillVehicles();
            return View(new MaintenanceForm { VehicleId = vehicleId });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Create(MaintenanceForm form)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            if (form.Mode == MaintenanceMode.Schedule)
            {
                if (form.StartDate is null)
                    ModelState.AddModelError(nameof(form.StartDate), "Start date is required.");
                if (form.EndDate is null)
                    ModelState.AddModelError(nameof(form.EndDate), "End date is required for scheduled maintenance.");
            }

            if (!ModelState.IsValid)
            {
                FillVehicles();
                return View(form);
            }

            try
            {
                Guid vehicleId = form.VehicleId!.Value;

                if (form.Mode == MaintenanceMode.OutOfService)
                {
                    _maintenance.MarkVehicleOutOfService(vehicleId, form.Description, form.Type);
                    TempData["Success"] = "Vehicle taken out of service.";
                }
                else
                {
                    _maintenance.ScheduleMaintenance(vehicleId, form.Type, form.Description,
                        AsUtc(form.StartDate!.Value), AsUtc(form.EndDate!.Value), form.Mileage, form.Cost, form.Notes);
                    TempData["Success"] = "Maintenance scheduled.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or KeyNotFoundException)
            {
                ModelState.AddModelError("", ex.Message);
                FillVehicles();
                return View(form);
            }
        }

        // POST /Maintenance/Complete – avslutar en post (slutdatum = nu)
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Complete(Guid id)
        {
            return RunAndRedirect(_ => _maintenance.CompleteMaintenance(id, DateTime.UtcNow),
                "Maintenance completed.", nameof(Index));
        }

        // POST /Maintenance/ReturnToService – avslutar allt pågående underhåll för bilen
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ReturnToService(Guid vehicleId)
        {
            return RunAndRedirect(_ => _maintenance.MarkVehicleActive(vehicleId),
                "Vehicle is back in service.", nameof(Index));
        }

        private MaintenanceRowViewModel ToRow(MaintenanceRecord r)
        {
            var vehicle = _fleet.GetVehicle(r.VehicleId);

            return new MaintenanceRowViewModel
            {
                MaintenanceId = r.MaintenanceId,
                VehicleId = r.VehicleId,
                RegNr = vehicle?.RegistrationPlate.RegNr ?? "?",
                Vehicle = vehicle is null ? "Unknown vehicle" : $"{vehicle.Model.Brand.BrandName} {vehicle.Model.ModelName}",
                Type = r.Type,
                Description = r.Description,
                StartDate = r.StartDate,
                EndDate = r.EndDate,
                Mileage = r.Mileage,
                Cost = r.Cost,
                Notes = r.Notes,
                IsCompleted = r.IsCompleted
            };
        }

        private void FillVehicles()
        {
            ViewData["Vehicles"] = _fleet.GetAllVehicles()
                .OrderBy(v => v.RegistrationPlate.RegNr)
                .Select(v => new SelectListItem($"{v.RegistrationPlate.RegNr} – {v.Model.Brand.BrandName} {v.Model.ModelName}", v.VehicleId.ToString()))
                .ToList();
        }

        // Datumfälten kommer utan tidszon, och underhållsposterna räknar i UTC
        private static DateTime AsUtc(DateTime d) => DateTime.SpecifyKind(d, DateTimeKind.Utc);
    }
}
