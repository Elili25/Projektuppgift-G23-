using Microsoft.AspNetCore.Mvc;

namespace SeventWebApplication.Controllers
{
    // Admin-dashboarden (/Admin). Bara layout och tomma sektioner än så länge, ingen data.
    public class AdminController : Controller
    {
        private const string DashboardView = "~/Views/FrontEnd_Admin/Frontend_Dashboard_Admin/Index.cshtml";

        public IActionResult Index()
        {
            return View(DashboardView);
        }
    }
}
