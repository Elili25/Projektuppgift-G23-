using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Admin;
using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Controllers
{
    // Admin: tidslinje över bokningarna (/Calendar?view=month&date=2026-11-01). Bara personal.
    // view = week | month | 2months, date = valfri dag i det som ska visas.
    public class CalendarController : StaffController
    {
        private readonly BookingRepository _bookings;
        private readonly IUserRepository _users;
        private readonly FleetManager _fleet;
        private readonly VehicleFamilyRepository _families;

        public CalendarController(
            UserSession session,
            BookingRepository bookings,
            IUserRepository users,
            FleetManager fleet,
            VehicleFamilyRepository families) : base(session)
        {
            _bookings = bookings;
            _users = users;
            _fleet = fleet;
            _families = families;
        }

        [HttpGet]
        public IActionResult Index(string? view, DateTime? date)
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return View(TimelineViewModel.Build(
                _bookings.GetAll(), _fleet.GetAllVehicles(), _users, _families,
                view, date ?? DateTime.Today));
        }
    }
}
