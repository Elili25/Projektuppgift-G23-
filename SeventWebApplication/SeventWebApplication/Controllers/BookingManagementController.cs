using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Admin;
using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Controllers
{
    // Admin: bokningar som väntar på beslut (Pending) och pågående uthyrningar (Active).
    // Själva reglerna ligger i BookingManagementService, här kallar vi bara på den.
    public class BookingManagementController : StaffController
    {
        private readonly BookingRepository _bookings;
        private readonly BookingManagementService _management;
        private readonly IUserRepository _users;

        public BookingManagementController(
            UserSession session,
            BookingRepository bookings,
            BookingManagementService management,
            IUserRepository users) : base(session)
        {
            _bookings = bookings;
            _management = management;
            _users = users;
        }

        // GET /BookingManagement/Pending
        [HttpGet]
        public IActionResult Pending()
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return View(AssignableRows(BookingStatus.Pending));
        }

        // GET /BookingManagement/Active – härifrån går man vidare till "Avsluta uthyrning" (Returns/Register)
        [HttpGet]
        public IActionResult Active()
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return View(Rows(BookingStatus.Active));
        }

        // GET /BookingManagement/Confirmed – bekräftade bokningar som väntar på upphämtning
        [HttpGet]
        public IActionResult Confirmed()
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return View(AssignableRows(BookingStatus.Confirmed));
        }

        // GET /BookingManagement/Pickup?id=... – formulär för upphämtning (Confirmed → Active)
        [HttpGet]
        public IActionResult Pickup(string id)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return PickupView(new PickupForm { BookingId = id ?? "" });
        }

        // POST /BookingManagement/Pickup – registrerar upphämtningen via StartRental
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Pickup(PickupForm form)
        {
            if (GetStaff(out IBookingManager staff) is IActionResult denied)
                return denied;

            if (!ModelState.IsValid)
                return PickupView(form);

            try
            {
                _management.StartRental(staff, form.BookingId, form.Mileage, form.FuelLevel, form.BatteryLevel, form.Notes ?? "");
                TempData["Success"] = "Pickup registered. The rental is now active.";
                return RedirectToAction(nameof(Active));
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or UnauthorizedAccessException)
            {
                ModelState.AddModelError("", ex.Message);
                return PickupView(form);
            }
        }

        // POST /BookingManagement/Approve – Pending → Confirmed.
        // vehicle: tomt = välj en bil automatiskt, "later" = tilldela först 2 timmar före start, annars id på vald bil
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Approve(string id, string? vehicle)
        {
            if (!TryParseVehicleChoice(vehicle, out Guid? vehicleId, out bool assignLater))
            {
                TempData["Error"] = "Choose a valid vehicle option.";
                return RedirectToAction(nameof(Pending));
            }

            string text = vehicleId is not null ? "Booking approved and the vehicle is assigned."
                : assignLater ? "Booking approved. A vehicle is assigned 2 hours before the start, or earlier from Awaiting pickup."
                : "Booking approved.";

            return RunAndRedirect(staff => _management.Approve(staff, id, vehicleId, assignLater), text, nameof(Pending));
        }

        // POST /BookingManagement/ChangeVehicle – byt bil på en Pending/Confirmed-bokning, eller välj "later" för att ta bort den
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult ChangeVehicle(string id, string? vehicle, string? returnTo)
        {
            // "later" betyder ingen bil än, ett id betyder just den bilen. Tomt är inte ett giltigt val här.
            if (!TryParseVehicleChoice(vehicle, out Guid? vehicleId, out bool assignLater) || (vehicleId is null && !assignLater))
            {
                TempData["Error"] = "Choose a vehicle, or Not assigned yet.";
                return RedirectToAction(returnTo == nameof(Pending) ? nameof(Pending) : nameof(Confirmed));
            }

            return RunAndRedirect(
                staff => _management.ChangeVehicle(staff, id, vehicleId),
                vehicleId is null ? "The vehicle was removed. Assign one before the pickup." : "The vehicle was changed.",
                returnTo == nameof(Pending) ? nameof(Pending) : nameof(Confirmed));
        }

        // POST /BookingManagement/Cancel – Pending/Confirmed → Cancelled
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Cancel(string id)
        {
            return RunAndRedirect(staff => _management.Cancel(staff, id), "Booking cancelled.", nameof(Pending));
        }

        private IActionResult PickupView(PickupForm form)
        {
            var booking = _bookings.GetById(form.BookingId);
            if (booking is null)
                return NotFound();

            // Upphämtning kräver en bil. Saknas den väljs en först i listan.
            if (booking.Vehicle is null)
            {
                TempData["Error"] = "Assign a vehicle to the booking before the pickup.";
                return RedirectToAction(nameof(Confirmed));
            }

            var now = DateTime.Now;
            var earliest = RentalReturnService.EarliestPickupTime(booking.StartDate);

            return View("Pickup", new PickupPageViewModel
            {
                Booking = BookingRowViewModel.From(booking, _users, now),
                Form = form,
                EarliestPickup = earliest,
                CanPickupNow = now >= earliest
            });
        }

        // "" = automatiskt, "later" = ingen bil än, annars en bils id. Allt annat är ogiltigt.
        private static bool TryParseVehicleChoice(string? choice, out Guid? vehicleId, out bool assignLater)
        {
            vehicleId = null;
            assignLater = false;

            if (string.IsNullOrWhiteSpace(choice))
                return true;

            if (string.Equals(choice, "later", StringComparison.OrdinalIgnoreCase))
            {
                assignLater = true;
                return true;
            }

            if (Guid.TryParse(choice, out Guid parsed))
            {
                vehicleId = parsed;
                return true;
            }

            return false;
        }

        // Rader med de bilar som går att välja för varje bokning (Pending och Confirmed)
        private List<AssignableBookingRow> AssignableRows(BookingStatus status)
        {
            DateTime now = DateTime.Now;
            return _bookings.GetByStatus(status)
                .OrderBy(b => b.StartDate)
                .Select(b => new AssignableBookingRow
                {
                    Booking = BookingRowViewModel.From(b, _users, now),
                    CurrentVehicleId = b.VehicleId?.ToString() ?? "",
                    Vehicles = _management.GetAssignableVehicles(b)
                        .Select(v => new BookingOption(
                            v.VehicleId.ToString(),
                            $"{v.Model.Brand.BrandName} {v.Model.ModelName}",
                            v.RegistrationPlate.RegNr))
                        .ToList(),
                    CanAssignLater = BookingManagementService.CanLeaveUnassigned(b, now),
                    // Bekräftad utan bil och det är dags: ska väljas nu (annars gör bakgrundsjobbet det)
                    NeedsVehicleNow = b.Vehicle is null && b.VehicleFamilyId is not null
                        && now >= b.StartDate - BookingManagementService.VehicleAssignDeadline
                })
                .ToList();
        }

        private List<BookingRowViewModel> Rows(BookingStatus status)
        {
            DateTime now = DateTime.Now;
            return _bookings.GetByStatus(status)
                .OrderBy(b => b.StartDate)
                .Select(b => BookingRowViewModel.From(b, _users, now))
                .ToList();
        }
    }
}
