using Microsoft.AspNetCore.Mvc;
using SeventWebApplication.Models.Users;
using SeventWebApplication.Models.VehicleNS;

namespace SeventWebApplication.Controllers
{
    // Admin: märken och modeller (/Brands). Registret som Fleet/Create väljer ur.
    public class BrandsController : StaffController
    {
        private readonly VehicleCatalogService _catalog;

        public BrandsController(UserSession session, VehicleCatalogService catalog) : base(session)
        {
            _catalog = catalog;
        }

        // GET /Brands – alla märken och modeller + formulär för att lägga till
        [HttpGet]
        public IActionResult Index()
        {
            if (GetStaff(out _) is IActionResult denied)
                return denied;

            return View(_catalog.GetCatalog());
        }

        // POST /Brands/Add – lägger till en märke/modell-kombination i registret
        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult Add(string brand, string model, VehicleType type)
        {
            return RunAndRedirect(_ => _catalog.AddCombination(brand, model, type),
                $"Added {brand?.Trim()} {model?.Trim()} ({type}) to the brand list.", nameof(Index));
        }
    }
}
