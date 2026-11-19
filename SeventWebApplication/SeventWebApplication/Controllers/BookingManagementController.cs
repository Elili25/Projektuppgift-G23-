using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Admin;
using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.Users;

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

            return View(Rows(BookingStatus.Pending));
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

            return View(Rows(BookingStatus.Confirmed));
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

        // POST /BookingManagement/Approve – Pending → Confirmed
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Approve(string id)
        {
            return RunAndRedirect(staff => _management.Approve(staff, id), "Booking approved.", nameof(Pending));
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

            return View("Pickup", new PickupPageViewModel
            {
                Booking = BookingRowViewModel.From(booking, _users, DateTime.Now),
                Form = form
            });
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
