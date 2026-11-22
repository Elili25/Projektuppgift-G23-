using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Admin;
using SeventWebApplication.Models.Booking;
using SeventWebApplication.Models.Pricing;
using SeventWebApplication.Models.Users;

namespace SeventWebApplication.Controllers
{
    // Admin-dashboarden (/Admin): Overview med nyckeltal, kommande bokningar och flottans läge.
    // Bara personal (IBookingManager), samma kontroll som övriga adminsidor.
    public class AdminController : StaffController
    {
        private const string DashboardView = "~/Views/FrontEnd_Admin/Frontend_Dashboard_Admin/Index.cshtml";

        private readonly BookingRepository _bookings;
        private readonly IUserRepository _users;
        private readonly FleetManager _fleet;
        private readonly PricingServices _pricing;

        public AdminController(
            UserSession session,
            BookingRepository bookings,
            IUserRepository users,
            FleetManager fleet,
            PricingServices pricing) : base(session)
        {
            _bookings = bookings;
            _users = users;
            _fleet = fleet;
            _pricing = pricing;
        }

        public IActionResult Index()
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return View(DashboardView, DashboardViewModel.Build(_bookings.GetAll(), _fleet.GetAllVehicles(), _users, _pricing));
        }
    }
}
