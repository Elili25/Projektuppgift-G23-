using Microsoft.AspNetCore.Mvc;
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
