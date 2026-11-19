using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Admin;
using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Controllers
{
    // Admin: returer och skador (UC4). Returformuläret går via BookingManagementService.CompleteRental,
    // skador åtgärdas via VehicleDamageService.ResolveDamage (då går bilen InRepair → Active).
    public class ReturnsController : StaffController
    {
        private readonly BookingRepository _bookings;
        private readonly BookingManagementService _management;
        private readonly VehicleDamageService _damages;
        private readonly FleetManager _fleet;
        private readonly IUserRepository _users;

        public ReturnsController(
            UserSession session,
            BookingRepository bookings,
            BookingManagementService management,
            VehicleDamageService damages,
            FleetManager fleet,
            IUserRepository users) : base(session)
        {
            _bookings = bookings;
            _management = management;
            _damages = damages;
            _fleet = fleet;
            _users = users;
        }

        // GET /Returns/Register?bookingId=... (bookingId förvalt när man kommer från Active rentals)
        [HttpGet]
        public IActionResult Register(string? bookingId)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return View(BuildPage(new ReturnForm { BookingId = bookingId ?? "" }));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Register(ReturnForm form)
        {
            if (GetStaff(out IBookingManager staff) is IActionResult denied)
                return denied;

            if (!ModelState.IsValid)
                return View(BuildPage(form));

            try
            {
                // En skada per rad i textrutan, tomma rader hoppas över
                var damages = (form.Damages ?? "")
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToList();

                decimal lateFee = _management.CompleteRental(
                    staff, form.BookingId, form.Mileage, form.FuelLevel, form.BatteryLevel,
                    form.InspectionResult, damages, form.Notes ?? "", form.DamageFee, form.ReturnedAt);

                var fees = new List<string>();
                if (lateFee > 0) fees.Add($"Late fee: {lateFee:0.##} kr");
                if (form.DamageFee > 0) fees.Add($"Damage fee: {form.DamageFee:0.##} kr");

                TempData["Success"] = fees.Count > 0
                    ? $"Return registered. {string.Join(". ", fees)}. The fees are saved on the booking."
                    : "Return registered.";

                return RedirectToAction(nameof(Register));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or UnauthorizedAccessException)
            {
                ModelState.AddModelError("", ex.Message);
                return View(BuildPage(form));
            }
        }

        // GET /Returns/Damages – öppna skador först, sedan åtgärdade
        [HttpGet]
        public IActionResult Damages()
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            var rows = _fleet.GetAllVehicles()
                .SelectMany(v => _damages.GetDamagesByVehicle(v.VehicleId).Select(d => new DamageRowViewModel
                {
                    DamageId = d.DamageId,
                    RegNr = v.RegistrationPlate.RegNr,
                    Vehicle = $"{v.Model.Brand.BrandName} {v.Model.ModelName}",
                    Description = d.Description,
                    ReportedAt = d.ReportedAt,
                    IsResolved = d.IsResolved
                }))
                .OrderBy(r => r.IsResolved)
                .ThenByDescending(r => r.ReportedAt)
                .ToList();

            return View(rows);
        }

        // POST /Returns/Resolve – markerar skadan som lagad
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Resolve(Guid id)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            if (_damages.ResolveDamage(id))
                TempData["Success"] = "Damage marked as repaired.";
            else
                TempData["Error"] = "Damage not found or already repaired.";

            return RedirectToAction(nameof(Damages));
        }

        private ReturnPageViewModel BuildPage(ReturnForm form)
        {
            DateTime now = DateTime.Now;
            return new ReturnPageViewModel
            {
                Form = form,
                ActiveBookings = _bookings.GetByStatus(BookingStatus.Active)
                    .OrderBy(b => b.EndDate)
                    .Select(b => BookingRowViewModel.From(b, _users, now))
                    .ToList()
            };
        }
    }
}
